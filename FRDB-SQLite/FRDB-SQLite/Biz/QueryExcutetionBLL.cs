using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;
using FRDB_SQLite.Biz;

namespace FRDB_SQLite
{
    public class QueryExcutetionBLL
    {
        #region 1. Fields
        private static String[] _operators = { "<", ">", "<=", ">=", "!=", "<>", "=" };
        private String[] _selectedAttributeTexts = null;
        private String[] _selectedRelationTexts = null;
        public String _conditionText = String.Empty;
        private List<String> dataTypeForOrderBy = new List<string>();
        List<Item> itemSelects = new List<Item>();

        private List<FzAttributeEntity> _selectedAttributes = new List<FzAttributeEntity>();
        private List<FzRelationEntity> _selectedRelations = new List<FzRelationEntity>();
        List<int> _index = new List<int>();

        private String _queryText;
        private String _queryType;
        private String _combinationType;
        private List<String> _singleQueries;
        private List<FzRelationEntity> _relationSet;
        private String _errorMessage;
        private Boolean _error;
        private FdbEntity _fdbEntity;

        #endregion

        #region 2. Properties

        public String QueryText
        {
            get { return _queryText; }
            set { _queryText = value; }
        }

        public List<FzRelationEntity> RelationSet
        {
            get { return _relationSet; }
            set
            {
                _relationSet = value;
            }
        }

        public String ErrorMessage
        {
            get { return _errorMessage; }
            set { this._errorMessage = value; }
        }

        public Boolean Error
        {
            get { return _error; }
            set { _error = value; }
        }

        #endregion

        #region 3. Contructors
        public QueryExcutetionBLL() { }
        public QueryExcutetionBLL(String queryText, FdbEntity fdbEntity)
        {
            this._queryText = queryText.ToLower();
            this._relationSet = fdbEntity.Relations;
            this._fdbEntity = fdbEntity;
            this._error = false;
            this._errorMessage = String.Empty;

            QueryAnalyze();//prepare for query processing

        }
        #endregion

        #region 4. Publics
        public FzRelationEntity ExecuteQuery()
        {
            FzRelationEntity result = null;
            try
            {
                if (this._queryType == Constants.QUERY_TYPE.COMBINATION)
                {
                    // The SQL statement with Union is now ready to process
                    QueryExcutetionBLL fstExecution = new QueryExcutetionBLL(this._singleQueries[0], this._fdbEntity);
                    QueryExcutetionBLL sndExecution = new QueryExcutetionBLL(this._singleQueries[1], this._fdbEntity);
                    FzRelationEntity fstQueryResult = fstExecution.ExecuteQuery();
                    FzRelationEntity sndQueryResult = sndExecution.ExecuteQuery();
                    if (fstExecution.Error)
                    {
                        throw new Exception(fstExecution.ErrorMessage);
                    }
                    if (sndExecution.Error)
                    {
                        throw new Exception(sndExecution.ErrorMessage);
                    }
                    FzRelationEntity unionResult = new FzRelationEntity()
                    {
                        RelationName = $"{fstQueryResult.RelationName}_{sndQueryResult.RelationName}",
                        Scheme = fstQueryResult.Scheme,
                    };
                    List<FzTupleEntity> resultTuples;
                    if (this._combinationType == Constants.COMBINATION_TYPE.UNION)
                    {
                        resultTuples = GetUnion(fstQueryResult.Tuples, sndQueryResult.Tuples);
                    }
                    else if (this._combinationType == Constants.COMBINATION_TYPE.INTERSECT)
                    {
                        resultTuples = GetIntersect(fstQueryResult.Tuples, sndQueryResult.Tuples);
                    }
                    else if (this._combinationType == Constants.COMBINATION_TYPE.EXCEPT)
                    {
                        resultTuples = GetExcept(fstQueryResult.Tuples, sndQueryResult.Tuples);
                    }
                    else
                    {
                        resultTuples = new List<FzTupleEntity>();
                    }
                    result = new FzRelationEntity()
                    {
                        RelationName = $"{fstQueryResult.RelationName}_{sndQueryResult.RelationName}",
                        Scheme = fstQueryResult.Scheme,
                        Tuples = resultTuples
                    };

                    //order by
                    if (this._queryText.Contains(" order by"))
                    {
                        this._selectedRelations.Add(result);
                        result.Tuples = ProcessOrderBy(result.Tuples);
                        result.Tuples.RemoveRange(0, result.Tuples.Count() / 2);
                    }
                }
                else
                {
                    result = new FzRelationEntity();
                    List<FzTupleEntity> resultTmp = new List<FzTupleEntity>();
                    this.GetSectedRelation(); if (this._error) throw new Exception(this._errorMessage);
                    this.GetSelectedAttr(); if (this._error) throw new Exception(this._errorMessage);

                    // _errorMessage = ExistsAttribute();
                    if (ErrorMessage != "") { this.Error = true; throw new Exception(_errorMessage); }


                    if (this._queryText.Contains("where"))
                    {
                        List<Item> items = FormatCondition(this._conditionText);
                        //Check fuzzy set and object here
                        this.ErrorMessage = ExistsFuzzySet(items);
                        if (ErrorMessage != "") { this.Error = true; return result; }

                        QueryConditionBLL condition = new QueryConditionBLL(items, this._selectedRelations, _fdbEntity);


                        foreach (FzTupleEntity tuple in this._selectedRelations[0].Tuples)
                        {
                            if (condition.Satisfy(items, tuple) != "0")
                            {
                                if (this._selectedAttributeTexts != null)
                                    resultTmp.Add(condition.ResultTuple);//done
                                else
                                    result.Tuples.Add(condition.ResultTuple);//done
                            }
                        }
                        if (this._queryText.Contains(" group by "))//done with having
                        {
                            result.Scheme.Attributes = this._selectedRelations[0].Scheme.Attributes;
                            result.Tuples = resultTmp;
                            result = ProcessGroupBy(result);// process group by and having            
                        }
                        else if (this._selectedAttributeTexts != null)
                        {
                            if (this._queryText.Contains(" order by"))
                                resultTmp = ProcessOrderBy(resultTmp);
                            result.Tuples.AddRange(GetSelectedAttributes(resultTmp, _fdbEntity));//Như add
                        }
                        result.Scheme.Attributes = this._selectedAttributes;
                    }
                    if (!this._queryText.Contains("where"))
                    {
                        result.Scheme.Attributes = this._selectedAttributes;
                        result.RelationName = this._selectedRelations[0].RelationName;
                        if (this._queryText.Contains(" group by "))//done
                        {
                            //result.Scheme.Attributes = this._selectedAttributes;
                            //foreach (var item in this._selectedRelations[0].Tuples)
                            //    result.Tuples.Add(item);
                            //result = this._selectedRelations[0];
                            result = ProcessGroupBy(this._selectedRelations[0]);// process group by and having            
                        }
                        else if (this._selectedAttributeTexts != null)
                        {
                            //foreach (var item in this._selectedRelations[0].Tuples)
                            //    result.Tuples.Add(GetSelectedAttributes(item));
                            if (this._queryText.Contains(" order by"))
                            {
                                resultTmp = ProcessOrderBy(this._selectedRelations[0].Tuples);
                                //result.Tuples.RemoveRange(0, this._selectedRelations[0].Tuples.Count() / 2);
                            }
                            else
                            {
                                foreach (var item in this._selectedRelations[0].Tuples)
                                    resultTmp.Add(item);
                            }

                            result.Tuples.AddRange(GetSelectedAttributes(resultTmp, _fdbEntity));//Như add false
                        }
                        else if (this._selectedAttributeTexts == null)
                        {
                            foreach (var item in this._selectedRelations[0].Tuples)
                                result.Tuples.Add(item);
                        }
                    }

                    //distinct
                    if (this._queryText.Contains("distinct "))
                    {
                        int countTupleOriginal = result.Tuples.Count();
                        result.Tuples = ProcessDistinct(result.Tuples);
                        result.Tuples.RemoveRange(0, countTupleOriginal);
                    }

                    //order by
                    if (this._queryText.Contains(" order by") && (this._selectedAttributeTexts == null || (this._selectedAttributeTexts != null && this._queryText.Contains(" group by "))))
                    {
                        result.Tuples = ProcessOrderBy(result.Tuples);
                        result.Tuples.RemoveRange(0, result.Tuples.Count() / 2);
                    }
                    else if(!this._queryText.Contains(" order by"))
                    {
                        result = ProcessNoOrderBy(result);
                    }
                }
            }
            catch (Exception ex)
            {
                this._error = true;
                this._errorMessage = ex.Message;
                //Như add return result;
            }

            return result;
        }

        private List<FzTupleEntity> GetUnion(List<FzTupleEntity> fstRelationTuples, List<FzTupleEntity> sndRelationTuples)
        {
            QueryConditionBLL condition = new QueryConditionBLL();
            List<FzTupleEntity> result = new List<FzTupleEntity>(fstRelationTuples);
            List<FzTupleEntity> tmpTuples = new List<FzTupleEntity>();
            foreach (FzTupleEntity sndRelationTuple in sndRelationTuples)
            {
                bool isTupleEqual = false;
                foreach (FzTupleEntity fstRelationTuple in result)
                {
                    if (fstRelationTuple.Equals(sndRelationTuple))
                    {
                        isTupleEqual = true;
                        String newFS = String.Empty;
                        String fstMemberShip = fstRelationTuple.MemberShip;
                        String sndMemberShip = sndRelationTuple.MemberShip;
                        DisFS fstDisFS = condition.getDisFS(fstMemberShip, _fdbEntity);
                        DisFS sndDisFS = condition.getDisFS(sndMemberShip, _fdbEntity);
                        if (fstDisFS == null && sndDisFS == null)
                        {
                            newFS = Math.Max(Convert.ToDouble(fstMemberShip), Convert.ToDouble(sndMemberShip)).ToString();
                        }
                        else if (fstDisFS == null)
                        {
                            fstDisFS = new DisFS(Convert.ToDouble(fstMemberShip));
                            newFS = condition.Max_DisFS(fstDisFS, sndDisFS);
                        }
                        else
                        {
                            sndDisFS = new DisFS(Convert.ToDouble(sndMemberShip));
                            newFS = condition.Max_DisFS(fstDisFS, sndDisFS);
                        }
                        fstRelationTuple.MemberShip = newFS;
                    }
                }
                if (!isTupleEqual)
                {
                    tmpTuples.Add(sndRelationTuple);
                }
            }
            result = result.Concat(tmpTuples).ToList();
            return result;
        }

        private List<FzTupleEntity> GetIntersect(List<FzTupleEntity> fstRelationTuples, List<FzTupleEntity> sndRelationTuples)
        {
            QueryConditionBLL condition = new QueryConditionBLL();
            List<FzTupleEntity> result = new List<FzTupleEntity>();
            foreach (FzTupleEntity sndRelationTuple in sndRelationTuples)
            {
                foreach (FzTupleEntity fstRelationTuple in fstRelationTuples)
                {
                    if (fstRelationTuple.Equals(sndRelationTuple))
                    {
                        String newFS = String.Empty;
                        String fstMemberShip = fstRelationTuple.MemberShip;
                        String sndMemberShip = sndRelationTuple.MemberShip;
                        DisFS fstDisFS = condition.getDisFS(fstMemberShip, _fdbEntity);
                        DisFS sndDisFS = condition.getDisFS(sndMemberShip, _fdbEntity);
                        if (fstDisFS == null && sndDisFS == null)
                        {
                            newFS = Math.Min(Convert.ToDouble(fstMemberShip), Convert.ToDouble(sndMemberShip)).ToString();
                        }
                        else if (fstDisFS == null)
                        {
                            fstDisFS = new DisFS(Convert.ToDouble(fstMemberShip));
                            newFS = condition.Min_DisFS(fstDisFS, sndDisFS);
                        }
                        else
                        {
                            sndDisFS = new DisFS(Convert.ToDouble(sndMemberShip));
                            newFS = condition.Min_DisFS(fstDisFS, sndDisFS);
                        }
                        result.Add(new FzTupleEntity(fstRelationTuple, newFS));
                    }
                }
            }
            return result;
        }

        private List<FzTupleEntity> GetExcept(List<FzTupleEntity> fstRelationTuples, List<FzTupleEntity> sndRelationTuples)
        {
            QueryConditionBLL condition = new QueryConditionBLL();
            List<FzTupleEntity> result = new List<FzTupleEntity>();
            foreach (FzTupleEntity fstRelationTuple in fstRelationTuples)
            {
                bool isTupleEqual = false;
                foreach (FzTupleEntity sndRelationTuple in sndRelationTuples)
                {
                    if (fstRelationTuple.Equals(sndRelationTuple))
                    {
                        isTupleEqual = true;
                        break;
                    }
                }
                if (!isTupleEqual)
                {
                    result.Add(fstRelationTuple);
                }
            }
            return result;
        }


        /// <summary>
        /// Return the List of string[]: 0.index of attribute; 1. operator; 2.value(maybe fuzzy); 3the logicality (and or not)
        /// </summary>
        public List<Item> FormatCondition(String condition)
        {
            List<Item> result = new List<Item>();
            int i = 0, j = 0, k = 0;
            bool flag = false;
            while (i < condition.Length - 1)
            {
                Item item = new Item();
                if (condition.Substring(i, 4) == "not ")
                {
                    item.notElement = true;
                    i += 4;
                }
                    
                if (condition.Substring(i, 4) == "min(")
                    item.aggregateFunction = "min";
                else if (condition.Substring(i, 4) == "max(")
                    item.aggregateFunction = "max";
                else if (condition.Substring(i, 4) == "avg(")
                    item.aggregateFunction = "avg";
                else if (condition.Substring(i, 4) == "sum(")
                    item.aggregateFunction = "sum";
                else if (condition.Substring(i, 6) == "count(")
                {
                    item.aggregateFunction = "count";
                    i += 5;
                }
                if (condition.Substring(i, 4) == "min(" || condition.Substring(i, 4) == "max(" || condition.Substring(i, 4) == "avg(" || condition.Substring(i, 4) == "sum(")
                    i += 3;
                if (condition[i] == '(')//(young=age) and (weight>=20 or height<=60)
                {
                    j = i + 1;
                    while (condition[j] != ')')
                    { 
                        j++; // Get index of ')'
                    }
                    if(j < condition.Length - 1)
                    {
                        if (condition[j] == ')' && condition[j + 1] == ')') j++;
                    } 
                    String exps = condition.Substring(i + 1, j - i - 1);
                    item.elements = SplitExpressions(exps);

                    // j < length, mean still expression in (...), get logicality
                    if (j != condition.Length - 1)
                    {
                        k = j + 1;
                        while (condition[k] != '(') k++;// Get index of next '('
                        if (condition.Substring(k - 4, 4) == "not ")
                        {
                            item.nextLogic = condition.Substring(j + 1, k - j - 5);
                            flag = true;
                            i = k - 5;

                        }
                        else if(condition.Substring(k - 3, 3) == "min" || condition.Substring(k - 3, 3) == "max" || condition.Substring(k - 3, 3) == "avg " || condition.Substring(k - 3, 3) == "sum" || condition.Substring(k - 4, 4) == "count")
                        {
                            item.nextLogic = condition.Substring(j + 1, k - j - 4);
                            flag = true;
                            i = k - 4;
                        }
                        else if (condition.Substring(k - 5, 5) == "count")
                        {
                            item.nextLogic = condition.Substring(j + 1, k - j - 6);
                            flag = true;
                            i = k - 6;
                        }
                        else
                        {
                            item.nextLogic = condition.Substring(j + 1, k - j - 1);
                            i = k - 1;
                        }
                            
                    }
                    else i = j - 1;// end of the condition

                    result.Add(item);
                    //if (j != condition.Length - 1)
                    //{
                    //    if (flag)
                    //        i = k - 5;
                    //    else
                    //        i = k - 1;
                    //}
                    //else i = j - 1;// end of the condition
                }
                i++;
            }

            return result;
        }

        public bool IsNumeric(object Expression)
         {
             double retNum;
             bool isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
             return isNum;
        }
        public Boolean IsNumericType(string o)
        {
            switch (o)
            {
                case "Byte":
                case "Binary":
                case "Currency":
                case "Int16":
                case "Int32":
                case "Int64":
                case "Decimal":
                case "Double":
                case "Single":
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region 5. Privates
        private void QueryAnalyze()
        {
            try
            {
                ///Get selected attributes which user input
                this._selectedAttributeTexts = GetAttributeTexts(this._queryText);

                if (this._queryText.Contains("union") || this._queryText.Contains("intersect") || this._queryText.Contains("except"))
                {
                    this._queryType = Constants.QUERY_TYPE.COMBINATION;
                }
                else
                {
                    this._queryType = Constants.QUERY_TYPE.SINGLE;
                }
                if (this._queryType == Constants.QUERY_TYPE.COMBINATION)
                {
                    if (this._queryText.Contains("union"))
                    {
                        this._combinationType = Constants.COMBINATION_TYPE.UNION;
                    }
                    else if (this._queryText.Contains("intersect"))
                    {
                        this._combinationType = Constants.COMBINATION_TYPE.INTERSECT;
                    }
                    else if (this._queryText.Contains("except"))
                    {
                        this._combinationType = Constants.COMBINATION_TYPE.EXCEPT;
                    }
                    this._singleQueries = SplitQuery(this._queryText, this._combinationType);
                    if (this._singleQueries[0].Contains("order by"))
                    {
                        this._errorMessage = "Query syntax is wrong";
                        throw new Exception(this._errorMessage);
                    }
                    String[] fstQueryAttributes = GetAttributeTexts(this._singleQueries[0]);
                    String[] sndQueryAttributes = GetAttributeTexts(this._singleQueries[1]);
                    bool isEqual = fstQueryAttributes.SequenceEqual(sndQueryAttributes);
                    if (!isEqual)
                    {
                        this._errorMessage = "The select statements do not have the same result columns";
                        throw new Exception(this._errorMessage);
                    }
                    String[] fstRelationTexts = GetRelationTexts(this._singleQueries[0]);
                    String[] sndRelationTexts = GetRelationTexts(this._singleQueries[1]);
                    FzRelationEntity fstRelation = this._fdbEntity.Relations
                        .Find(item => item.RelationName.Equals(fstRelationTexts[0], StringComparison.InvariantCultureIgnoreCase));
                    FzRelationEntity sndRelation = this._fdbEntity.Relations
                        .Find(item => item.RelationName.Equals(sndRelationTexts[0], StringComparison.InvariantCultureIgnoreCase));
                    for (int i = 0; i < fstQueryAttributes.Length; i++)
                    {
                        FzAttributeEntity fstAttr = fstRelation.Scheme.Attributes
                            .Find(attr => attr.AttributeName.Equals(fstQueryAttributes[i], StringComparison.InvariantCultureIgnoreCase));
                        FzAttributeEntity sndAttr = sndRelation.Scheme.Attributes
                            .Find(attr => attr.AttributeName.Equals(sndQueryAttributes[i], StringComparison.InvariantCultureIgnoreCase));
                        if (fstAttr.DataType.DataType != sndAttr.DataType.DataType)
                        {
                            this._errorMessage = "The select statements do not have the same result columns";
                            throw new Exception(_errorMessage);
                        }
                    }
                }
                else
                {
                    ///Get selected relations which user input
                    this._selectedRelationTexts = GetRelationTexts(this._queryText);
                    _errorMessage = ExistsRelation();
                    if (ErrorMessage != "") { this.Error = true; throw new Exception(_errorMessage); }

                    ///Get condition text user input
                    this._conditionText = GetConditionText(this._queryText);

                    // Add quotes mark for condition
                    if (_conditionText != String.Empty)
                        this._conditionText = AddParenthesis(this._conditionText);
                    //Format condition
                }
            }
            catch (Exception ex)
            {
                this._errorMessage = ex.Message;
                this._error = true;
            }
        }

        private void GetSectedRelation()
        {
            try
            {
                if (this._selectedRelationTexts != null)
                {
                    foreach (FzRelationEntity item in RelationSet)
                    {
                        if (item.RelationName.ToLower().Equals(this._selectedRelationTexts[0]))
                        {
                            this._selectedRelations.Add(item);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this._error = true;
                this._errorMessage = ex.Message;
            }
        }

        private void GetSelectedAttr()
        {
            try
            {
                int indexParenThesis1 = 0, indexParenThesis2 = 0;
                Boolean flag = false;
                string textTmp = "", textAs;
                if (this._selectedAttributeTexts != null)
                {
                    foreach (String text in this._selectedAttributeTexts)
                    {
                        int i = 0;
                        int count = 0;
                        foreach (FzAttributeEntity attr in this._selectedRelations[0].Scheme.Attributes)
                        {
                            //process aggregation: select min, max, count
                            if(text.Contains("min(") || text.Contains("max(") || text.Contains("avg(") || text.Contains("sum(") || text.Contains("count("))
                            {
                                
                                indexParenThesis1 = text.IndexOf("(");
                                indexParenThesis2 = text.IndexOf(")");
                                textTmp = text.Substring(indexParenThesis1 + 1, indexParenThesis2 - indexParenThesis1 - 1);
                                //string jj = textTmp.Substring(10);
                               if (textTmp.IndexOf("-distinct-") > -1 && textTmp.Substring(10).Equals(attr.AttributeName.ToLower()))
                                {
                                    this._errorMessage = "We do not support aggregate_function(distinct xxx), sorry for this inconvinience.";
                                    if (ErrorMessage != "") { this.Error = true; throw new Exception(_errorMessage); }
                                }
                                if (textTmp.Equals(attr.AttributeName.ToLower()) || ((textTmp == "*" && text.Contains("count("))|| (textTmp.IndexOf("-distinct-") > -1 && textTmp.Substring(10).Equals(attr.AttributeName.ToLower()))))
                                {
                                    Item itemSelect = new Item();

                                    if (text.Contains("-distinct-"))//count(distinct xxx)
                                    {
                                        //if (text.IndexOf("-distinct-") == 0)
                                        //    itemSelect.aggregateFunction = text.Substring(10, indexParenThesis1 - 10);
                                        //else if (text.IndexOf("(-distinct-") > 0)
                                        //    itemSelect.aggregateFunction = text.Substring(0, indexParenThesis1);
                                        itemSelect.aggregateFunction = text.Substring(10, indexParenThesis1 - 10);
                                    } 
                                    else itemSelect.aggregateFunction = text.Substring(0, indexParenThesis1);

                                    FzAttributeEntity attrText = new FzAttributeEntity();
                                    //if (textTmp.IndexOf("-distinct-") > -1)//sum(-distinct-Age)...
                                    //{
                                    //    itemSelect.isDistinct = true;
                                    //}  
                                    //else if ((text.IndexOf("min(") > 0 || text.IndexOf("max(") > 0 || text.IndexOf("count(") > 0 || text.IndexOf("sum(") > 0 || text.IndexOf("avg(") > 0) && !text.Contains("-distinct-"))
                                    //    count = -1;
                                    if (text.IndexOf("-as-") > 0)
                                    {
                                        textAs = text.Substring(text.IndexOf("-as-") + 4, text.Length - text.IndexOf("-as-") - 4);
                                        attrText.AttributeName = textAs;
                                        itemSelect.attributeNameAs = textAs;
                                    }
                                    else
                                    {
                                        if (text.IndexOf("-distinct-") == 0)
                                            textAs = text.Substring(10, indexParenThesis2 - 9);
                                        else textAs = text;
                                        attrText.AttributeName = textAs;
                                        itemSelect.attributeNameAs = textAs;
                                    }    
                                    attrText.DataType.DomainString = "[5.0 x 10^-324  ...  1.7 x 10^308]";
                                    attrText.DataType.DataType = "Double";
                                    attrText.DataType.TypeName = "Double";
                                    this._selectedAttributes.Add(attrText);
                                    itemSelect.elements.Add(i.ToString());
                                    itemSelects.Add(itemSelect);
                                    count++;
                                    if ((textTmp != "" && text.Contains("count(")))
                                        break;
                                    else if (!text.Contains("count("))
                                    {
                                        //int index = this._selectedRelations[0].Scheme.Attributes.FindIndex
                                        Boolean error = IsNumericType((this._selectedRelations[0].Scheme.Attributes[i].DataType.DataType).ToString());
                                        if (!error) throw new Exception("Data type of aggregate function is not valid, we only support aggregate function with numerical data");


                                    }

                                }
                                
                            }
                            else if (text.Contains(attr.AttributeName.ToLower()))
                            {
                                FzAttributeEntity attrText = new FzAttributeEntity();
                                textTmp = text;
                                if (text.IndexOf("-distinct-") > -1)
                                    textTmp = textTmp.Substring(10);
                                if (textTmp.IndexOf("-as-") > 0)
                                {
                                    textAs = textTmp.Substring(textTmp.IndexOf("-as-") + 4, textTmp.Length - textTmp.IndexOf("-as-") - 4);
                                    attrText.AttributeName = textAs;
                                    attrText.DataType.DataType = attr.DataType.DataType;
                                    this._selectedAttributes.Add(attrText);
                                    textTmp = textTmp.Substring(0, textTmp.IndexOf("-as-"));
                                    if (textTmp.Equals(attr.AttributeName.ToLower()))
                                        count++;
                                }
                                else if (textTmp.Equals(attr.AttributeName.ToLower()))
                                {
                                    attrText.AttributeName = textTmp;
                                    attrText.DataType.DataType = attr.DataType.DataType;
                                    this._selectedAttributes.Add(attrText);
                                    count++;
                                }
                                _index.Add(i);
                            }
                            if(text == "*" || text == "-distinct-*")
                            {
                                for(int l = 0; l < this._selectedRelations[0].Scheme.Attributes.Count - 1 ; l++ )
                                {
                                    this._selectedAttributes.Add(this._selectedRelations[0].Scheme.Attributes[l]);
                                    _index.Add(l);
                                }
                                //flag = true;
                                count++;
                                break;
                            }
                            if (count > 0) break;
                            i++;

                        }
                        if (count == 0)
                        {
                            this._errorMessage = "Invalid selected object name of attribute: '" + text + "'.";
                            if (ErrorMessage != "") { this.Error = true; throw new Exception(_errorMessage); }
                        }     
                    }
                    // Add the membership attribute
                    this._selectedAttributes.Add(this._selectedRelations[0].Scheme.Attributes[this._selectedRelations[0].Scheme.Attributes.Count - 1]);
                }
                else
                {
                    // Mean select * from ...
                    this._selectedAttributes = this._selectedRelations[0].Scheme.Attributes;
                    for (int i = 0; i < this._selectedRelations[0].Scheme.Attributes.Count() - 1; i++)
                        _index.Add(i);
                }
                this._errorMessage = CheckAggreateFunctionAndAttribute();
            }
            catch (Exception ex)
            {
                this._error = true;
                this._errorMessage = ex.Message;
            }
        }

        private List<FzTupleEntity> GetSelectedAttributes(List<FzTupleEntity> resultTuple, FdbEntity _fdbEntity)
        {
            Boolean filter = false;
            List<FzTupleEntity> rs = new List<FzTupleEntity>();
            List<string> attributes = new List<string>();
            Boolean flagGetOneTuple = false;//use when have aggregate function & attributes
            if (_index.Count > 0)
            {
                int h = 0;
                foreach (var item0 in resultTuple) 
                {
                    h++;
                    FzTupleEntity r = new FzTupleEntity();
                    for (int i = 0; i < _index.Count; i++)
                    {
                        for (int j = 0; j < item0.ValuesOnPerRow.Count; j++)
                        {
                            if(flagGetOneTuple) goto End;
                            if (_index[i] == j)
                            {
                                r.Add(item0.ValuesOnPerRow[j]);
                                if (h == 1) attributes.Add(this._selectedRelations[0].Scheme.Attributes[j].AttributeName.ToString());
                                if (i == _index.Count - 1 && (itemSelects.Count > 0 || (itemSelects.Count == 0 && _queryText.Contains(" group by "))))
                                {
                                    flagGetOneTuple = true;
                                    goto End;
                                }
                                break;
                            }
                        }
                        
                    }
                    End:;
                if(r.ValuesOnPerRow.Count > 0)
                {
                        if (itemSelects.Count == 0)
                        {
                            if(_queryText.Contains(" group by "))
                            {
                                var arrMembership = resultTuple.Select(x => x.ValuesOnPerRow[this._selectedRelations[0].Scheme.Attributes.Count() - 1].ToString());
                                List<String> membershipList = arrMembership.ToList();
                                string FSName = "";
                                QueryConditionBLL condition = new QueryConditionBLL(_fdbEntity);
                                for (int i = 0; i < membershipList.Count(); i++)
                                {
                                    FSName = condition.FindAndMarkFuzzy(membershipList[i].ToString(), FSName);
                                }
                                foreach (var item in resultTuple.Where(x => x.ValuesOnPerRow[this._selectedRelations[0].Scheme.Attributes.Count() - 1].ToString() != FSName))
                                {
                                    item.ValuesOnPerRow[this._selectedRelations[0].Scheme.Attributes.Count() - 1] = FSName;
                                }
                            }
                            r.Add(item0.ValuesOnPerRow[item0.ValuesOnPerRow.Count - 1]);
                            rs.Add(r);
                            
                        }
                        else if (itemSelects.Count > 0)
                        {
                            QueryConditionBLL condtition = new QueryConditionBLL(_fdbEntity);
                            int i = 0;
                            string FSName = "";
                            
                            foreach (Item item in itemSelects)
                            {
                                i++;
                                string FSName1 = "", FSName2 = "";
                                double k = 0, tmp = 0; ;
                                int countMinMax = 0; 
                                foreach (var itemTuple in resultTuple)
                                {
                                    k++;
                                    for (int j = 0; j < itemTuple.ValuesOnPerRow.Count; j++)
                                    {
                                        if (int.Parse(item.elements[0]) == j && item.aggregateFunction != "min" && item.aggregateFunction != "max")
                                        {
                                            if (item.aggregateFunction == "sum")
                                                tmp = tmp + Convert.ToDouble(itemTuple.ValuesOnPerRow[j]);
                                            else if (item.aggregateFunction == "count")
                                            {
                                                //if(item.isDistinct && k== resultTuple.Count())
                                                //{
                                                //    var tuples = resultTuple.AsEnumerable().GroupBy(x => x.ValuesOnPerRow[Int32.Parse(item.elements[0].ToString())]).Select(grouping => grouping.Take(1)).ToList();
                                                //    tmp = tuples.Count();
                                                //}
                                                //else tmp += 1;
                                                tmp += 1;
                                            }   
                                            else if (item.aggregateFunction == "avg")
                                            {
                                                tmp = tmp + Convert.ToDouble(itemTuple.ValuesOnPerRow[j]);
                                                if (k == resultTuple.Count)
                                                    tmp = double.Parse((tmp / k).ToString());
                                            }
                                            FSName1 = condtition.FindAndMarkFuzzy(itemTuple.ValuesOnPerRow[itemTuple.ValuesOnPerRow.Count - 1].ToString(), FSName1);
                                            break;
                                        }
                                        else if (int.Parse(item.elements[0]) == j && (item.aggregateFunction == "min" || item.aggregateFunction == "max"))
                                        {
                                            if (k == 1) tmp = double.Parse(itemTuple.ValuesOnPerRow[j].ToString());
                                            if (item.aggregateFunction == "min")
                                            {
                                                if (tmp >= double.Parse(itemTuple.ValuesOnPerRow[j].ToString()))
                                                {
                                                    tmp = double.Parse(itemTuple.ValuesOnPerRow[j].ToString());
                                                    FSName2 = itemTuple.ValuesOnPerRow[itemTuple.ValuesOnPerRow.Count - 1].ToString();
                                                    countMinMax++;
                                                }

                                            }
                                            if (item.aggregateFunction == "max")
                                            {
                                                if (tmp <= double.Parse(itemTuple.ValuesOnPerRow[j].ToString()))
                                                {
                                                    tmp = double.Parse(itemTuple.ValuesOnPerRow[j].ToString());
                                                    FSName2 = itemTuple.ValuesOnPerRow[itemTuple.ValuesOnPerRow.Count - 1].ToString();
                                                    countMinMax++;
                                                }
                                            }

                                            if(k == resultTuple.Count  && countMinMax > 1)
                                            {
                                                for (int y = 0; y < resultTuple.Count; y++)
                                                { 
                                                    if (y == 0) FSName2 = "";
                                                    if(double.Parse(resultTuple[y].ValuesOnPerRow[j].ToString()) == tmp)
                                                    {
                                                        FSName2 = condtition.FindAndMarkFuzzy(resultTuple[y].ValuesOnPerRow[itemTuple.ValuesOnPerRow.Count - 1].ToString(), FSName2);
                                                    }
                                                }
                                            }

                                        }
                                    }
                                    if (k == 1) attributes.Add(item.attributeNameAs);
                                }
                                if (FSName1 != "")
                                    FSName = condtition.FindAndMarkFuzzy(FSName1, FSName);
                                if (FSName2 != "")
                                    FSName = condtition.FindAndMarkFuzzy(FSName2, FSName);

                                r.Add(tmp);
                                
                                if (i == itemSelects.Count)
                                {
                                    r.Add(FSName);
                                    rs.Add(r);
                                }


                            }
                        }
                        //rearrange select mix: min , max, filed1, count, field,...to return tuple and attributes respectedly
                        string tmpValue1 = "", tmpValue2 = "";
                        for (int i = 0; i < this._selectedAttributes.Count; i++)
                        {
                            for (int k = 0; k < attributes.Count; k++)
                            {
                                if (this._selectedAttributes[i].AttributeName.ToString() == attributes[k].ToLower())
                                {
                                    foreach (var tuple in rs)
                                    {
                                        tmpValue1 = tuple.ValuesOnPerRow[k].ToString();
                                        tmpValue2 = tuple.ValuesOnPerRow[i].ToString();
                                        tuple.ValuesOnPerRow[i] = tmpValue1;
                                        tuple.ValuesOnPerRow[k] = tmpValue2;

                                    }
                                    tmpValue1 = attributes[i];
                                    tmpValue2 = attributes[k]; ;
                                    attributes[k] = tmpValue1;
                                    attributes[i] = tmpValue2;
                                    break;
                                }

                            }
                        }//end rearrange
                    }     
                }
            }
            else if (itemSelects.Count > 0)
            {
                QueryConditionBLL condtition = new QueryConditionBLL(_fdbEntity);
                FzTupleEntity r = new FzTupleEntity();
                int i = 0;
                string FSName = "";
                foreach (Item item in itemSelects)     
                {
                    i++;
                    double k = 0;
                    double tmp = 0;
                    string FSName1 = "", FSName2 = "";
                    int countMinMax = 0;
                    foreach (var itemTuple in resultTuple)
                    {
                        k++;
                        for (int j = 0; j < itemTuple.ValuesOnPerRow.Count; j++)
                        {
                            if (int.Parse(item.elements[0]) == j && item.aggregateFunction != "min" && item.aggregateFunction != "max")
                            {
                                if(item.aggregateFunction == "sum")
                                    tmp = tmp + Convert.ToDouble(itemTuple.ValuesOnPerRow[j]);
                                else if(item.aggregateFunction == "count")
                                {
                                    //if (item.isDistinct && k == resultTuple.Count())
                                    //{
                                    //    var tuples = resultTuple.AsEnumerable().GroupBy(x => x.ValuesOnPerRow[Int32.Parse(item.elements[0].ToString())]).Select(grouping => grouping.Take(1)).ToList();
                                    //    tmp = tuples.Count();
                                    //}
                                    //else tmp += 1;
                                    tmp += 1;
                                }
                                else if (item.aggregateFunction == "avg")
                                {
                                    tmp = tmp + Convert.ToDouble(itemTuple.ValuesOnPerRow[j]);
                                    if (k == resultTuple.Count)
                                        tmp = double.Parse((tmp / k).ToString());
                                }
                                FSName1 = condtition.FindAndMarkFuzzy(itemTuple.ValuesOnPerRow[itemTuple.ValuesOnPerRow.Count - 1].ToString(), FSName1);
                                break;
                            }
                            else if (int.Parse(item.elements[0]) == j && (item.aggregateFunction == "min" || item.aggregateFunction == "max"))
                            {
                                if(k == 1) tmp = double.Parse(itemTuple.ValuesOnPerRow[j].ToString());
                                if (item.aggregateFunction == "min")
                                {
                                    if (tmp >= double.Parse(itemTuple.ValuesOnPerRow[j].ToString()))
                                    {
                                        tmp = double.Parse(itemTuple.ValuesOnPerRow[j].ToString());
                                        FSName2 = itemTuple.ValuesOnPerRow[itemTuple.ValuesOnPerRow.Count - 1].ToString();
                                        countMinMax++;
                                    }
                                       
                                }
                                if (item.aggregateFunction == "max")
                                {
                                    if (tmp <= double.Parse(itemTuple.ValuesOnPerRow[j].ToString()))
                                    {
                                        tmp = double.Parse(itemTuple.ValuesOnPerRow[j].ToString());
                                        FSName2 = itemTuple.ValuesOnPerRow[itemTuple.ValuesOnPerRow.Count - 1].ToString();
                                        countMinMax++;
                                    }    
                                }
                                if (k == resultTuple.Count && countMinMax > 1)
                                {
                                    for (int y = 0; y < resultTuple.Count; y++)
                                    {
                                        if (y == 0) FSName2 = "";
                                        if (double.Parse(resultTuple[y].ValuesOnPerRow[j].ToString()) == tmp)
                                        {
                                            FSName2 = condtition.FindAndMarkFuzzy(resultTuple[y].ValuesOnPerRow[itemTuple.ValuesOnPerRow.Count - 1].ToString(), FSName2);
                                        }
                                    }
                                }

                            }
                        } 
                    }  
                    r.Add(tmp);
                    if (FSName1 != "")
                        FSName = condtition.FindAndMarkFuzzy(FSName1, FSName);
                    if (FSName2 != "")
                        FSName = condtition.FindAndMarkFuzzy(FSName2, FSName);
                    if (i == itemSelects.Count)
                    {
                        r.Add(FSName);
                        rs.Add(r);
                    }
                        
                   
                }                
                //r.Add(resultTuple.ValuesOnPerRow[resultTuple.ValuesOnPerRow.Count - 1]);
            }
            return rs;
        }

        private String[] GetAttributeTexts(String s)
        {//the attributes which user input such: select attr1, att2... from
            String[] result = null;
            //String was standardzied and cut space,....
            int i = 7;//Attribute after "select"
            int j = s.IndexOf("from");
            String tmp = s.Substring(i, j - i);
            if (tmp != "* ")
            {
                tmp = tmp.Replace(" as ", "-as-");
                tmp = tmp.Replace("distinct ", "-distinct-");
                tmp = tmp.Replace(" ", "");
                //if (s.Contains("min")) //đừng xóa cmt này :)
                //{
                //    tmp = tmp.Replace("min", "");
                //    tmp = tmp.Replace("(", "");
                //    tmp = tmp.Replace(")", "");
                //}
                result = tmp.Split(',');
            }
            

            return result;
        }

        private String[] GetRelationTexts(String s)
        {//the relations which user input such: select attr1, att2... from
            String[] result = null;
            //String was standardzied and cut space,....
            int i = s.IndexOf(" from ") + 6;
            int j = s.Length;//query text doesn't contain any conditions
            if (s.Contains(" where "))//query text contains conditions
                j = s.IndexOf(" where ");
            else if (s.Contains(" group by "))
                j = s.IndexOf(" group by ");
            else if (s.Contains(" order by "))
                j = s.IndexOf(" order by ");
            String tmp = s.Substring(i, j - i);
            tmp = tmp.Replace(" ", "");
            result = tmp.Split(',');

            return result;
        }

        private String GetConditionText(String s)
        {//the relations which user input such: select attr1, att2... from
            String result = String.Empty;
            //String was standardzied and cut space,....
            int i = 0;
            if(s.Contains(" where "))//for get condition where only
            {
                i = s.IndexOf(" where ") + 7;
                if (!s.Contains(" group by ") && !s.Contains(" order by"))
                    result = s.Substring(i);
                else if (s.Contains(" group by "))
                    result = s.Substring(i, s.IndexOf(" group by ") - i);
                else if (s.Contains(" order by "))
                    result = s.Substring(i, s.IndexOf(" order by ") - i);
            }
            else if(s.Contains(" having ") && !s.Contains(" where "))// for get condition having only
            {
                i = s.IndexOf(" having ") + 8;
                if (s.Contains(" order by "))
                    result = s.Substring(i, s.IndexOf(" order by ") - i);
                else if (!s.Contains(" order by "))
                    result = s.Substring(i);
            }
            

                return result;
        }

        private string[] getFilterGroupBy()
        {
            int groupby = _queryText.IndexOf(" group by ");
            int having = _queryText.IndexOf(" having ");
            int orderby = _queryText.IndexOf(" order by ");
            string[] filterStr = null;
            string tmp = "";
            //this._selectedAttributeTexts = null;
            if (groupby > 0)
            {
                if (having < 0 && orderby < 0)
                {
                    tmp = _queryText.Substring(groupby + 10, _queryText.Length - groupby - 10);
                }
                else if (having > 0) // group by... + having ...
                {
                    tmp = _queryText.Substring(groupby + 10, having - groupby - 10);

                }//having
                else if (orderby > 0 && having < 0)// group by... + order by
                {
                    tmp = _queryText.Substring(groupby + 10, orderby - groupby - 10);
                }
                tmp = tmp.Replace(" ", "");
                filterStr = tmp.Split(',');

            }
            return filterStr;
        }


        private int IndexOfAttr(String s)
        {
            for (int i = 0; i < this._selectedRelations[0].Scheme.Attributes.Count; i++)
            {
                if (s.Equals(this._selectedRelations[0].Scheme.Attributes[i].AttributeName.ToLower()))
                {
                    return i;
                }
                else if (s == "*")
                    return -1;
            }
            return -2;
        }

        private List<String> SplitQuery(String mutipleQueryText, String combinationType)
        {
            int orderIndex = mutipleQueryText.IndexOf(" order by ");
            String tmpQueryText = mutipleQueryText;
            if (orderIndex >= 0)
            {
                tmpQueryText = mutipleQueryText.Substring(0, orderIndex);
            }
            List<String> singleQueries = new List<String>();
            String seperator = String.Empty;
            if (combinationType == Constants.COMBINATION_TYPE.UNION)
            {
                seperator = " union ";
            }
            else if (combinationType == Constants.COMBINATION_TYPE.INTERSECT)
            {
                seperator = " intersect ";
            }
            else if (combinationType == Constants.COMBINATION_TYPE.EXCEPT)
            {
                seperator = " except ";
            }
            singleQueries = tmpQueryText.Split(new String[] { seperator }, StringSplitOptions.None).ToList();
            return singleQueries;
        }

        private int IndexOfAttrGroupBy(string s)
        {
            int i = 0;
            string tmp = "";
            int flagCount = 0;
            if (IsNumber(s) && Int32.Parse(s) < this._selectedAttributes.Count - 1)//order by index of group by, must be in group by
            {
                this.dataTypeForOrderBy.Add(this._selectedAttributes[Convert.ToInt32(s)].DataType.DataType.ToString());
                return Int32.Parse(s);
            }
                
            As:
            for (i = 0; i < this._selectedAttributes.Count(); i++)
            {
                if (s.Equals(this._selectedAttributes[i].AttributeName.ToLower()))
                {
                    this.dataTypeForOrderBy.Add(this._selectedAttributes[i].DataType.DataType.ToString());
                    return i;
                }
                else if (s == "*")
                    return -1;
            }
            flagCount++;
            if (this._selectedAttributeTexts.Any(x => x.Contains("-as-")) && flagCount < this._selectedAttributeTexts.Count())
            {
                for (int y = 0; y < this._selectedAttributeTexts.Count(); y++)
                {
                    if (this._selectedAttributeTexts[y].Contains(s))
                    {
                        
                        tmp = this._selectedAttributeTexts[y];
                        if (this._selectedAttributeTexts[y].Contains("-distinct-"))
                            tmp = tmp.Substring(10);
                        if (this._selectedAttributeTexts[y].Contains("-as"))
                        {
                            tmp = tmp.Substring(0, this._selectedAttributeTexts[y].IndexOf("-as-"));
                            s = this._selectedAttributeTexts[y].Substring(this._selectedAttributeTexts[y].IndexOf("-as-") + 4);
                            goto As;
                        }
                            
                    }
                }
            }
            
            return -2;
        }

        private String ReverseOperator(String op)
        {
            String result = op;
            if (op == "<")
                result = ">";
            if (op == "<=")
                result = ">=";
            if (op == ">")
                result = "<";
            if (op == ">=")
                result = "<=";

            return result;
        }

        private bool ContainLogicality(String exp)
        {
            if (exp.Contains(" and ") || exp.Contains(" or ") /*|| exp.Contains(" not ") edit*/)
                return true;
            return false;
        }
        

        /// <summary>
        /// Split rank of expressions or one
        /// Rank contains logicality
        /// </summary>
        private List<String> SplitExpressions(String exps)
        {
            List<String> result = new List<string>();
            int i = 0;
            if (exps.Length < 5)
            {
                AddToList(result, exps, exps.Length, "");
                return result;
            }
            while(i <= exps.Length - 5)
            {
                if (!ContainLogicality(exps))
                {
                    // Add seperator; Split added and add to result; Also add the logicality
                    AddToList(result, exps, exps.Length, "");
                    //result.Add("");//For using in subQuery
                    return result;
                }

                String logic = exps.Substring(i, 5); // The logicality: " and ", " or ", " not "
                if (logic == " and ")
                {
                    // Add seperator; Split added and add to result; Also add the logicality
                    AddToList(result, exps, i, " and ");
                    exps = exps.Remove(0, i + 5);
                    i = -1;
                }
                else if(logic.Substring(0, logic.Length - 1) == " or ")
                {
                    // Add seperator; Split added and add to result; Also add the logicality
                    AddToList(result, exps, i, " or ");
                    exps = exps.Remove(0, i + 4);
                    i = -1;
                }
                //else if (logic == " not ")
                //{
                //    // Add seperator; Split added and add to result; Also add the logicality
                //    AddToList(result, exps, i, " not ");
                //    exps = exps.Remove(0, i + 5);
                //    i = -1;
                //}
                i++;
            }
            return result;

        }

        private void AddToList(List<String> result, String exps, int i, String logic)
        {
            String exp = exps.Substring(0, i);
            // Add seperator
            exp = AddSeperator(exp);
            //
            // Split added and add to result.
            List<String> splExps = Split(exp);
            foreach (string item in splExps)
            {
                result.Add(item.Trim());
            }
            // Also add the logicality
            if (logic != "") result.Add(logic);
        }
        /// <summary>
        /// Split the expressions (only one operator) to index of attribute and value
        /// The expression param does not contain logicality
        ///index0: attribute index (return index of attribute in this._selectedAttributes)
        ///index1: operator  
        ///index2: value (maybe fuzzy value)
        /// </summary>
        private List<String> Split(String expression)
        {
            List<String> result = new List<string>();
            String[] splited = expression.Split('|');
            String[] tmp = new String[splited.Length];
            for (int i = 0; i < splited.Length; i++)
            {
                tmp[i] = splited[i].Trim();// Prevent user query with spaces
            }
            splited = tmp;
            ///Get index of attribulte and add to first of result
            ///after that add operator and value next to it
            int flag = -1;
            //edit---
            int start = 0;
            if(splited.Length==4)
            {
                start = 1;
                result.Add(splited[0]);
            }
            //----
            for (int i = start; i < splited.Length; i++)
            {
                int index = IndexOfAttr(splited[i]);
                if (index >= -1)
                {
                    // Add the index of attribute
                    result.Add(index.ToString());
                    flag = i;
                    break;
                }
                else
                {
                    _errorMessage = "Invalid object name of attribute: '" + splited[i] + "'.";
                    _error = true;
                    throw new Exception(_errorMessage);
                }
            }
            
            // Add operator and value (maybe fuzzy value)
            if (flag == start)//age=young
            {
                for (int i =start+1; i < splited.Length - 1; i += 2)
                {
                    result.Add(splited[i]);//Add operator
                    result.Add(splited[i + 1]);//Add value (maybe fuzzy value)
                }
            }
            else if (flag == splited.Length - 1)//young=age
            {
                for (int i = splited.Length - 2; i > 0; i -= 2)
                {
                    result.Add(ReverseOperator(splited[i]));//Add operator
                    result.Add(splited[i - 1]);//Add value (maybe fuzzy value)
                }
            }
            else
                result = null;

            return result;

        }

        /// <summary>
        /// Add seperator | to split the operator, the logicality
        /// </summary>
        private String AddSeperator(String expression)
        {
            //expression = expression.Replace(" ", "");//here
            if (expression.Contains("not "))
            {
                if (expression.Contains("not like"))
                {
                    expression = expression.Insert(expression.IndexOf("not "), "|");
                    expression = expression.Insert(expression.IndexOf("like") + 4, "|");

                }
                if (expression.Contains("not in "))
                {
                    expression = expression.Insert(expression.IndexOf("not in "), "|");
                    expression = expression.Insert(expression.IndexOf("not in ") + 6, "|");

                }
                else if (!expression.Contains("not like") && !expression.Contains("not in "))
                {
                    expression = expression.Insert(expression.IndexOf("not ") + 3, "|");
                }   
            }
            if (expression.Contains("like ") && !expression.Contains("not like "))
            {
                expression = expression.Insert(expression.IndexOf("like") , "|");
                expression = expression.Insert(expression.IndexOf("like") + 4, "|");

            }

            if (expression.Contains("in "))
            {
                expression = expression.Insert(expression.IndexOf("in "), "|");
                expression = expression.Insert(expression.IndexOf("in ") + 2, "|");

            }


            for (int i = 1; i < expression.Length - 1; i++)
            {
                
                if (expression[i] == '<' || expression[i] == '>')
                {
                    expression = expression.Insert(i, "|"); i++;

                    if (expression[i + 1] == '=')
                    {
                        expression = expression.Insert(i + 2, "|"); i++;
                    }
                    else
                    {
                        expression = expression.Insert(i + 1, "|"); i++;
                    }
                }
                if (expression[i] == '=')
                {
                    if (expression[i - 1] == '!') 
                    {
                        expression = expression.Insert(i - 1, "|"); i++;
                        expression = expression.Insert(i + 1, "|"); i++;
                    }
                    else if (expression[i - 1] != '<' && expression[i - 1] != '>')
                    {
                        expression = expression.Insert(i, "|"); i++;
                        expression = expression.Insert(i + 1, "|"); i++;
                    }
                }

                if (expression[i] == '→')
                {
                    expression = expression.Insert(i, "|");
                    expression = expression.Insert(i + 2, "|"); i += 2;
                }

               

            }
            return expression;
        }

        /// <summary>
        /// Add quotes mark to each expression: Do not allow quotes nest((
        /// </summary>
        //private String AddParenthesis(String condition)
        //{
        //    for (int i = 0; i < condition.Length - 5; i++)
        //    {
        //        String logic = condition.Substring(i, 5);// " and ", " or ", " not ".
        //        if (condition[i] == '(')
        //        {
        //            int j = i + 1;
        //            while (condition[j] != ')') i = j++;
        //            i -= 5;// Prevent Index was outside the bounds of the array
        //        }
        //        else
        //        {
        //            if (logic == " and " || logic == " not ")
        //            {
        //                if (condition[i - 1] != ')' && condition[i + 5] != '(')
        //                {
        //                    condition = condition.Insert(i + 5, "(");
        //                    condition = condition.Insert(i, ")");
        //                    i += 2;
        //                }
        //                else if (condition[i - 1] == ')' && condition[i + 5] != '(')
        //                {
        //                    condition = condition.Insert(i + 5, "(");
        //                    i++;
        //                }
        //                else if (condition[i - 1] != ')' && condition[i + 5] == '(')
        //                    condition = condition.Insert(i++, ")");
        //                i += 4;// Jump to the '('
        //            }
        //            if (logic.Substring(0, logic.Length - 1) == " or ")
        //            {
        //                if (condition[i - 1] != ')' && condition[i + 4] != '(')
        //                {
        //                    condition = condition.Insert(i + 4, "(");
        //                    condition = condition.Insert(i, ")");
        //                    i += 2;
        //                }
        //                else if (condition[i - 1] == ')' && condition[i + 4] != '(')
        //                {
        //                    condition = condition.Insert(i + 4, "(");
        //                    i++;
        //                }
        //                else if (condition[i - 1] != ')' && condition[i + 4] == '(')
        //                    condition = condition.Insert(i++, ")");
        //                i += 3;// Jump to the '('
        //            }
        //        }
        //    }
        //    if (condition[0] != '(')
        //        condition = condition.Insert(0, "(");
        //    if (condition[condition.Length - 1] != ')')
        //        condition += ")";

        //    return condition;
        //}
        // edit-----
        private String AddParenthesis(String condition)
        {
            int closeFunction = 0; //pos for () of min, max, sum...
            for (int i = 0; i < condition.Length - 5; i++)
            {
                String logic = condition.Substring(i, 5);// " and ", " or "
                if (condition[i] == '(' && !condition.Contains(")"))
                {
                    int j = i + 1;
                    while (condition[j] != ')') i = j++;
                    i -= 5;// Prevent Index was outside the bounds of the array
                }
                else
                {
                    if (i < (condition.Length - 8) && condition.Substring(i, 8) == "between ")
                    {
                        int k = i;
                        bool isNotEle = false;
                        // Find the index of the first ')' before "between"
                        while (k > 0 && (condition.Substring(k, 5) != " and " || (condition.Substring(k, 4) != " or ")))
                            k--;
                        // Get the attribute name before "between"
                        if (condition.Substring(k, 5) == " and ")
                        {
                            k = k + 5;
                        }
                        if (condition.Substring(k, 4) == " or ")
                        {
                            k = k + 4;
                        }
                        if (condition.Substring(k, 4) == "not ")
                        {
                            k = k + 4;
                            isNotEle = true;
                        }
                        String attributeName = condition.Substring(k, i - k);
                        // Replace the text "between" with the comparison operator ">="
                        if (isNotEle)
                        {
                            condition = condition.Replace("between", "<");
                        }
                        else
                        {
                            condition = condition.Replace("between", ">=");
                        }
                        int j = i + 1;
                        // Find the index of the text " and "
                        while (j < (condition.Length - 5) && condition.Substring(j, 5) != " and ") j++;
                        // Insert the attribute name and the comparison operator "<=" for the second value
                        // If there is "not" statement, we add the comparison operator ">" for the second value and replace "and" to "or" and remove "not" 
                        if (isNotEle)
                        {
                            condition = condition.Insert(j + 5, attributeName + " > ");
                            condition = condition.Replace("and", "or");
                            condition = condition.Replace("not ", "");
                        }
                        else
                        {
                            condition = condition.Insert(j + 5, attributeName + " <= ");
                        }
                    }
                    if (logic == " and ")
                    {
                        if (condition[i - 1] != ')' && condition[i + 5] != '(')
                        {
                            if (condition.Substring(i + 5, 4) == "not " && condition[i + 9] != '(')
                            {
                                condition = condition.Insert(i + 9, "(");
                                condition = condition.Insert(i, ")");
                                i += 6;
                            }
                            else if (condition.Substring(i + 5, 4) == "not " && condition[i + 9] == '(')
                            {
                                condition = condition.Insert(i, ")");
                                i += 5;
                            }
                            else if ((condition.Substring(i + 5, 3) == "min" || condition.Substring(i + 5, 3) == "max" || condition.Substring(i + 5, 3) == "avg" || condition.Substring(i + 5, 3) == "sum") && condition[i + 8] == '(')
                            {
                                closeFunction = condition.IndexOf(")", i + 9);
                                condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min (Age > 10)
                                condition = condition.Insert(i, ")");
                                i += 5;
                            }
                            ////else if ((condition.Substring(i + 5, 3) == "min" || condition.Substring(i + 5, 3) == "max" || condition.Substring(i + 5, 3) == "avg" || condition.Substring(i + 5, 3) == "sum") && condition[i + 8] != '(')
                            ////{
                            ////    closeFunction = condition.IndexOf(")", i + 9);
                            ////    condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min (Age > 10)
                            ////    condition = condition.Insert(i + 8, "(");
                            ////    condition = condition.Insert(i, ")");
                            ////    i += 5;
                            ////}
                            //else if (condition.Substring(i + 5, 5) == "count" && condition[i + 9] != '(')
                            //{
                            //    closeFunction = condition.IndexOf(")", i + 10);
                            //    condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min (Age > 10)
                            //    condition = condition.Insert(i + 9, "(");
                            //    condition = condition.Insert(i, ")");
                            //    i += 6;
                            //}//cần check min(fdjf) ở hàm check syntax
                            else if (condition.Substring(i + 5, 5) == "count" && condition[i + 10] == '(')
                            {
                                closeFunction = condition.IndexOf(")", i + 10);
                                condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min (Age > 10)
                                condition = condition.Insert(i, ")");
                                i += 6;
                            }
                            else if (condition.Substring(i + 5, 4) != "not ")
                            {
                                condition = condition.Insert(i + 5, "(");
                                condition = condition.Insert(i, ")");
                                i += 2;
                            }
                        }
                        else if (condition[i - 1] != ')' && condition[i + 5] == '(')
                        {

                            if ((condition.Substring(i + 6, 3) == "min" || condition.Substring(i + 6, 3) == "max" || condition.Substring(i + 6, 3) == "avg" || condition.Substring(i + 6, 3) == "sum") && condition[i + 9] == '(')
                            {
                                closeFunction = condition.IndexOf(")", i + 10);
                                condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min (Age > 10)
                                condition = condition.Remove(i + 5, 1);
                            }
                            else if (condition.Substring(i + 6, 5) == "count" && condition[i + 11] == '(')
                            {
                                closeFunction = condition.IndexOf(")", i + 12);
                                condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min (Age > 10)
                            }
                            condition = condition.Insert(i, ")");
                        }


                        else if (condition[i - 1] == ')' && condition[i + 5] != '(')
                        {
                            if (condition.Substring(i + 5, 4) == "not " && condition[i + 9] != '(')
                            {
                                condition = condition.Insert(i + 9, "(");
                                i += 5;
                            }

                            //else if ((condition.Substring(i + 5, 3) == "min" || condition.Substring(i + 5, 3) == "max" || condition.Substring(i + 5, 3) == "avg" || condition.Substring(i + 5, 3) == "sum") && condition[i + 8] != '(')
                            //{
                            //    closeFunction = condition.IndexOf(")", i + 9);
                            //    condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min (Age > 10)
                            //    condition = condition.Insert(i + 8, "(");
                            //    i = i - 1;
                            //}
                            else if ((condition.Substring(i + 5, 3) == "min" || condition.Substring(i + 5, 3) == "max" || condition.Substring(i + 5, 3) == "avg" || condition.Substring(i + 5, 3) == "sum") && condition[i + 8] == '(')
                            {
                                closeFunction = condition.IndexOf(")", i + 9);
                                condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min (Age > 10)
                                i += 4;
                            }
                            else if (condition.Substring(i + 5, 5) == "count" && condition[i + 10] == '(')
                            {
                                closeFunction = condition.IndexOf(")", i + 11);
                                condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min (Age > 10)
                                i = i + 6;
                            }
                            else if (condition.Substring(i + 5, 4) != "not ")
                            {
                                condition = condition.Insert(i + 5, "(");
                                i += 5;
                            }


                        }

                        i += 4;// Jump to the '('
                    }
                    if (logic.Substring(0, logic.Length - 1) == " or ")
                    {
                        if (condition[i - 1] != ')' && condition[i + 4] != '(')
                        {
                            if (condition.Substring(i + 4, 4) == "not " && condition[i + 8] != '(')
                            {
                                condition = condition.Insert(i + 8, "(");
                                condition = condition.Insert(i, ")");
                                i += 6;
                            }
                            else if (condition.Substring(i + 4, 4) == "not " && condition[i + 8] == '(')
                            {
                                condition = condition.Insert(i, ")");
                                i += 5;
                            }
                            else if ((condition.Substring(i + 4, 3) == "min" || condition.Substring(i + 4, 3) == "max" || condition.Substring(i + 4, 3) == "avg" || condition.Substring(i + 4, 3) == "sum") && condition[i + 7] == '(')
                            {
                                closeFunction = condition.IndexOf(")", i + 8);
                                condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min(Age > 10)
                                condition = condition.Insert(i, ")");
                                i += 5;
                            }
                            //else if ((condition.Substring(i + 5, 3) == "min" || condition.Substring(i + 5, 3) == "max" || condition.Substring(i + 5, 3) == "avg" || condition.Substring(i + 5, 3) == "sum") && condition[i + 8] != '(')
                            //{
                            //    closeFunction = condition.IndexOf(")", i + 9);
                            //    condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min (Age > 10)
                            //    condition = condition.Insert(i + 8, "(");
                            //    condition = condition.Insert(i, ")");
                            //    i += 5;
                            //}
                            else if (condition.Substring(i + 4, 5) == "count" && condition[i + 9] == '(')
                            {
                                closeFunction = condition.IndexOf(")", i + 10);
                                condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min (Age > 10)
                                condition = condition.Insert(i, ")");
                                i += 7;
                            }//cần check min(fdjf) ở hàm check syntax
                            else if (condition.Substring(i + 4, 4) != "not ")
                            {
                                condition = condition.Insert(i + 4, "(");
                                condition = condition.Insert(i, ")");
                                i += 2;
                            }
                        }

                        else if (condition[i - 1] == ')' && condition[i + 4] != '(')
                        {
                            if (condition.Substring(i + 4, 4) == "not " && condition[i + 8] != '(')
                                condition = condition.Insert(i + 8, "(");
                            else if ((condition.Substring(i + 4, 3) == "min" || condition.Substring(i + 4, 3) == "max" || condition.Substring(i + 4, 3) == "avg" || condition.Substring(i + 4, 3) == "sum") && condition[i + 7] == '(')
                            {
                                closeFunction = condition.IndexOf(")", i + 8);
                                condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min(Age > 10)
                                i = i - 1;
                            }
                            else if (condition.Substring(i + 4, 5) == "count" && condition[i + 9] == '(')
                            {
                                closeFunction = condition.IndexOf(")", i + 10);
                                condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min (Age > 10)
                                i += 1;
                            }//cần check min(fdjf) ở hàm check syntax
                            else if (condition.Substring(i + 4, 4) != "not ")
                                condition = condition.Insert(i + 4, "(");
                            i += 5;
                        }
                        else if (condition[i - 1] != ')' && condition[i + 4] == '(')
                        {
                            if ((condition.Substring(i + 5, 3) == "min" || condition.Substring(i + 5, 3) == "max" || condition.Substring(i + 5, 3) == "avg" || condition.Substring(i + 5, 3) == "sum") && condition[i + 8] == '(')
                            {
                                closeFunction = condition.IndexOf(")", i + 10);
                                condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min (Age > 10)
                                condition = condition.Remove(i + 4, 1);
                            }
                            else if (condition.Substring(i + 5, 5) == "count" && condition[i + 10] == '(')
                            {
                                closeFunction = condition.IndexOf(")", i + 11);
                                condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min (Age > 10)
                            }
                            condition = condition.Insert(i, ")");
                            i += 2;
                        }

                        i += 3;// Jump to the '('
                    }
                }
            }
            if (condition[0] != '(')
            {
                if (condition.Substring(0, 4) == "not ")
                {
                    if ((condition.Substring(4, 3) == "min" || condition.Substring(4, 3) == "max" || condition.Substring(4, 3) == "avg" || condition.Substring(4, 3) == "sum") && condition[7] == '(')
                    {
                        closeFunction = condition.IndexOf(")");
                        condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min (Age > 10)
                    }
                    else if (condition.Substring(4, 5) == "count" && condition[9] == '(')
                    {
                        closeFunction = condition.IndexOf(")");
                        condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min (Age > 10)
                    }
                    else if (condition.Substring(4, 3) != "min") condition = condition.Insert(4, "(");
                }
                    
                else if ((condition.Substring(0, 3) == "min" || condition.Substring(0, 3) == "max" || condition.Substring(0, 3) == "avg" || condition.Substring(0, 3) == "sum") && condition[3] == '(')
                {
                    closeFunction = condition.IndexOf(")");
                    condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min (Age > 10)
                }
                else if (condition.Substring(0, 5) == "count" && condition[5] == '(')
                {
                    closeFunction = condition.IndexOf(")");
                    condition = condition.Remove(closeFunction, 1);// min(Age) > 10 => min (Age > 10)
                }
                else
                    condition = condition.Insert(0, "(");
            }
            if (condition[condition.Length - 1] != ')')
                condition += ")";

            if (condition.Contains(" in "))
            {
                int p1 = 0, p2 = 0, pos = 0;
                pos = condition.IndexOf(" in ", 0, condition.Length - 1);
                for (int i = pos; i < condition.Length - 1; i = pos)
                {
                    if (pos > 0)
                    {
                        p1 = condition.IndexOf(")", pos, condition.Length - pos);
                        if (p1 < condition.Length - 1)
                        {
                            p2 = condition.IndexOf(")", p1 + 1, 1);
                            if (p2 < 0)
                            {
                                condition = condition.Insert(p1 + 1, ")");
                            }
                        }
                        else if (p1 == condition.Length - 1)
                        {
                            condition += ")";
                            break;
                        }

                    }
                    else break;
                    pos = condition.IndexOf(" in ", i + 2, condition.Length - i - 2);
                }
            }

            return condition;
        }

        //private String AddParenthesis(String condition)
        //{
        //    for (int i = 0; i < condition.Length - 8; i++)
        //    {
        //        String logic = condition.Substring(i, 8);// "between"
        //        if (condition[i] == '(' && !condition.Contains(")"))
        //        {
        //            int j = i + 1;
        //            while (condition[j] != ')') i = j++;
        //            i -= 5;// Prevent Index was outside the bounds of the array
        //        }
        //        else
        //        {
        //            if (logic == "between ")
        //            {
        //                int k = i;
        //                bool isNotEle = false;
        //                // Find the index of the first ')' before "between"
        //                while (k > 0 && (condition.Substring(k, 5) != " and " || (condition.Substring(k, 4) != " or ")))
        //                    k--;
        //                // Get the attribute name before "between"
        //                if (condition.Substring(k, 5) == " and ")
        //                {
        //                    k = k + 5;
        //                }
        //                if (condition.Substring(k, 4) == " or ")
        //                {
        //                    k = k + 4;
        //                }
        //                if (condition.Substring(k, 4) == "not ")
        //                {
        //                    k = k + 4;
        //                    isNotEle = true;
        //                }
        //                String attributeName = condition.Substring(k, i - k);
        //                // Replace the text "between" with the comparison operator ">="
        //                if (isNotEle)
        //                {
        //                    condition = condition.Replace("between", "<");
        //                }
        //                else
        //                {
        //                    condition = condition.Replace("between", ">=");
        //                }
        //                int j = i + 1;
        //                // Find the index of the text " and "
        //                while (j < (condition.Length - 5) && condition.Substring(j, 5) != " and ") j++;
        //                // Insert the attribute name and the comparison operator "<=" for the second value
        //                // If there is "not" statement, we add the comparison operator ">" for the second value and replace "and" to "or" and remove "not" 
        //                if (isNotEle)
        //                {
        //                    condition = condition.Insert(j + 5, attributeName + " > ");
        //                    condition = condition.Replace("and", "or");
        //                    condition = condition.Replace("not ", "");
        //                }
        //                else
        //                {
        //                    condition = condition.Insert(j + 5, attributeName + " <= ");
        //                }
        //            }
        //            if (logic == " and ")
        //            {
        //                if (condition[i - 1] != ')' && condition[i + 5] != '(')
        //                {
        //                    if (condition.Substring(i + 5, 4) == "not " && condition[i + 9] != '(')
        //                    {
        //                        condition = condition.Insert(i + 9, "(");
        //                        condition = condition.Insert(i, ")");
        //                        i += 6;
        //                    }
        //                    else if (condition.Substring(i + 5, 4) == "not " && condition[i + 9] == '(')
        //                    {
        //                        condition = condition.Insert(i, ")");
        //                        i += 5;
        //                    }
        //                    else if (condition.Substring(i + 5, 4) != "not ")
        //                    {
        //                        condition = condition.Insert(i + 5, "(");
        //                        condition = condition.Insert(i, ")");
        //                        i += 2;
        //                    }
        //                }
        //                else if (condition[i - 1] != ')' && condition[i + 5] == '(')
        //                    condition = condition.Insert(i++, ")");

        //                else if (condition[i - 1] == ')' && condition[i + 5] != '(')
        //                {
        //                    if (condition.Substring(i + 5, 4) == "not " && condition[i + 9] != '(')
        //                        condition = condition.Insert(i + 9, "(");
        //                    else if (condition.Substring(i + 5, 4) != "not ")
        //                        condition = condition.Insert(i + 5, "(");
        //                    i +=5;
        //                }

        //                i += 4;// Jump to the '('
        //            }
        //            if (logic.Substring(0, logic.Length - 1) == " or ")
        //            {
        //                if (condition[i - 1] != ')' && condition[i + 4] != '(')
        //                {
        //                    if (condition.Substring(i + 4, 4) == "not " && condition[i + 8] != '(')
        //                    {
        //                        condition = condition.Insert(i + 8, "(");
        //                        condition = condition.Insert(i, ")");
        //                        i += 6;
        //                    }
        //                    else if (condition.Substring(i + 4, 4) == "not " && condition[i + 8] == '(')
        //                    {
        //                        condition = condition.Insert(i, ")");
        //                        i += 5;
        //                    }
        //                    else if (condition.Substring(i + 4, 4) != "not ")
        //                    {
        //                        condition = condition.Insert(i + 4, "(");
        //                        condition = condition.Insert(i, ")");
        //                        i += 2;
        //                    }
        //                }
        //                else if (condition[i - 1] == ')' && condition[i + 4] != '(')
        //                {
        //                    if (condition.Substring(i + 4, 4) == "not " && condition[i + 8] != '(')
        //                        condition = condition.Insert(i + 8, "(");
        //                    else if (condition.Substring(i + 4, 4) != "not ")
        //                        condition = condition.Insert(i + 4, "(");
        //                    i +=5;
        //                }
        //                else if (condition[i - 1] != ')' && condition[i + 4] == '(')
        //                    condition = condition.Insert(i++, ")");
        //                i += 3;// Jump to the '('
        //            }
        //        }
        //    }
        //    if (condition[0] != '(')
        //    {
        //        if (condition.Substring(0, 4) == "not ")
        //            condition = condition.Insert(4, "(");
        //        else
        //            condition = condition.Insert(0, "(");
        //    }
        //    if (condition[condition.Length - 1] != ')')
        //        condition += ")";

        //    if(condition.Contains(" in "))
        //    {
        //        int p1 = 0, p2 = 0, pos = 0;
        //        pos = condition.IndexOf(" in ", 0, condition.Length -1);
        //        for (int i = pos; i < condition.Length -1; i= pos)
        //        {
        //            if (pos > 0)
        //            {
        //                p1 = condition.IndexOf(")", pos, condition.Length - pos);
        //                if (p1 < condition.Length - 1)
        //                {
        //                    p2 = condition.IndexOf(")", p1 + 1, 1);
        //                    if (p2 < 0)
        //                    {
        //                        condition = condition.Insert(p1 + 1, ")");
        //                    }
        //                }
        //                else if (p1 == condition.Length - 1)
        //                {
        //                    condition += ")";
        //                    break;
        //                }

        //            }
        //            else break;
        //            pos = condition.IndexOf(" in ", i + 2, condition.Length - i - 2);
        //        }
        //    }
        //    //if (condition[0] != '(' && condition[condition.Length - 1] != ')')
        //    //{
        //    //    condition += ")";
        //    //    if (condition.Substring(0, 4) == "not ")
        //    //        condition = condition.Insert(4, "(");
        //    //    else
        //    //        condition = condition.Insert(0, "(");
        //    //}
        //    //else if (condition[0] != '(' && condition[condition.Length - 1] == ')')
        //    //{
        //    //    int pos = condition.LastIndexOf("(");
        //    //    if(condition.Substring(pos - 4, 4) == " in " || condition.Substring(pos - 8, 8) == " not in ")
        //    //    {
        //    //        condition += ")";
        //    //    }
        //    //    condition = condition.Insert(0, "(");
        //    //}
        //    //else if (condition[0] == '(' && condition[condition.Length - 1] != ')')
        //    //{
        //    //    condition += ")";
        //    //}
        //    return condition;
        //}

        #endregion

        #region 6. Check Objects
        private String ExistsFuzzySet(List<Item> items)
        {
            String message = "";
            FuzzyProcess fp = new FuzzyProcess();
            String path = Directory.GetCurrentDirectory() + @"\lib\";
            foreach (var item in items)
            {
                if (item.elements[1] == "->" || item.elements[1] == "→")
                {
                    if (!fp.Exists(path + item.elements[2] + ".conFS") &&
                        !fp.Exists(path + item.elements[2] + ".disFS"))
                    {
                        return message = "Incorrect fuzzy set: '" + item.elements[2] + "'.";
                    }
                }
            }

            return message;
        }

        private String ExistsRelation()
        {
            String message = "";
            int count = 0;
            foreach (var item in _relationSet)
            {
                if (_selectedRelationTexts[0].ToLower() == item.RelationName.ToLower())
                    count++;
            }
            if (count == 0)
                return message = "Invalid object name of relation: '" + _selectedRelationTexts[0] + "'.";

            return message;
        }

        private String ExistsAttribute()
        {
            String message = "";
            if (_selectedRelations.Count == 0 || _selectedAttributeTexts == null) return "";

            foreach (var item in _selectedAttributeTexts)
            {
                int count = 0;
                String attr = item.ToLower();
                foreach (var item1 in _selectedRelations[0].Scheme.Attributes)
                {
                    if (item1.AttributeName.ToLower() == attr)
                        count++;
                }

                if (count == 0)
                    return message = "Invalid selected object name of attribute: '" + attr + "'.";
            }
            return message;
        }

        private String CheckAggreateFunctionAndAttribute()
        {
            string message = "";
            if (!_queryText.Contains(" group by") && _index.Count() > 0 && itemSelects.Count() > 0)
                message = "Invalid select list because it is not contained in either an aggregate function or the GROUP BY clause.";
            return message;
        }

        private String CheckExistAttribute(string[] filterStr)
        {
            String message = "";
            if (filterStr.Length == 0) return "";

            for(int i = 0; i < filterStr.Length; i++)
            {
                int count = 0;
                //String attr = item.ToLower();
                foreach (var item in _selectedRelations[0].Scheme.Attributes)
                {
                    string s = item.AttributeName.ToLower();
                    string s1 = filterStr[i].ToLower();
                    int l1 = s.Length;
                    int l2 = s1.Length;
                    string h = "";
                    if (l1 == l2) h = "Equal";
                   // if (item.AttributeName.ToLower() == filterStr[i])
                        if (s1.Equals(s))
                            count++;
                }

                if (count == 0)
                    return message = "Invalid selected object name of attribute: '" + filterStr[i] + "'.";
            }
            return message;
        }
        
        private string[] CheckOrderByReturnList(string[] listOrder)
        {
            string orderByAttr = "";
            int indexAttr = 0;
            for (int p = 0; p < listOrder.Count(); p++)
            {
                if (listOrder[p].IndexOf(" ") == 0)
                    listOrder[p] = listOrder[p].Remove(0, 1);
                if (listOrder[p].IndexOf(" ") == listOrder[p].Count() - 1)
                    listOrder[p] = listOrder[p].Remove(listOrder[p].Count() - 1, 1);
                if (listOrder[p].IndexOf(" ") > 0 && listOrder[p].IndexOf(" desc") < 0 && listOrder[p].IndexOf(" asc") < 0)
                {
                    this._errorMessage = "Invalid attribute to order by";
                    throw new Exception(this._errorMessage);
                }
                orderByAttr = listOrder[p].Split(' ').First();
                Boolean flag = true;
                Start:
                if (_queryText.Contains(" group by "))
                    indexAttr = IndexOfAttrGroupBy(orderByAttr);
                else
                {
                    //indexAttr = IndexOfAttr(orderByAttr);
                    for (int i = 0; i < this._selectedRelations[0].Scheme.Attributes.Count; i++)
                    {
                        if (orderByAttr.Equals(this._selectedRelations[0].Scheme.Attributes[i].AttributeName.ToLower()))
                        {
                            indexAttr =  i;
                            this.dataTypeForOrderBy.Add(this._selectedRelations[0].Scheme.Attributes[i].DataType.DataType);
                            break;
                        }
                        else indexAttr = - 1;
                    }
                    if (IsNumber(orderByAttr) && Int32.Parse(orderByAttr) < this._selectedAttributes.Count - 1 && indexAttr < 0 && flag)//order by index 
                    {
                        indexAttr = Int32.Parse(orderByAttr);
                        if (this._selectedAttributeTexts != null)//!group by
                        {
                            orderByAttr = this._selectedAttributes[indexAttr].AttributeName.ToLower();
                            flag = false;
                            goto Start;
                        }
                        this.dataTypeForOrderBy.Add(this._selectedAttributes[indexAttr].DataType.DataType);
                    }
                        
                    if (indexAttr < 0 && (orderByAttr.Contains("count(") || orderByAttr.Contains("sum(") || orderByAttr.Contains("max(") || orderByAttr.Contains("min(") || orderByAttr.Contains("avg")))//order by when _selectedAttributeTexts != null, !group by & select count(*), sum....
                    {
                        for(int i = 0; i < this._selectedAttributes.Count - 1; i++)
                        {
                            if (orderByAttr.Equals(this._selectedAttributes[i].AttributeName.ToLower()))
                            {
                                indexAttr = i;
                                this.dataTypeForOrderBy.Add(this._selectedAttributes[indexAttr].DataType.DataType);
                            }
                                
                        }
                    }
                }
                    
               
                if (indexAttr < 0)
                {
                    if(_queryText.Contains(" as "))
                    {
                        for (int y = 0; y < this._selectedAttributeTexts.Count(); y++)
                        {
                            if(this._selectedAttributeTexts[y].IndexOf("-as-") > 0)
                            {
                                string asAttribute = this._selectedAttributeTexts[y].Substring(this._selectedAttributeTexts[y].IndexOf("-as-") + 4, this._selectedAttributeTexts[y].Length - this._selectedAttributeTexts[y].IndexOf("-as-") - 4);
                                if(asAttribute == orderByAttr)
                                {
                                    flag = false;
                                    orderByAttr = this._selectedAttributeTexts[y].Substring(0, this._selectedAttributeTexts[y].IndexOf("-as-"));
                                    if(orderByAttr.Contains("-distinct-"))
                                        orderByAttr = orderByAttr.Substring(orderByAttr.IndexOf("-distinct-") + 10);
                                    if ((orderByAttr.Contains("sum(") || orderByAttr.Contains("min(") || orderByAttr.Contains("max(") || orderByAttr.Contains("avg(") || orderByAttr.Contains("count(")) && !_queryText.Contains(" group by "))
                                    {
                                        indexAttr = 0;
                                        this.dataTypeForOrderBy.Add(this._selectedAttributes[indexAttr].DataType.DataType);
                                        break;
                                    }
                                    goto Start;
                                }
                                    
                            }
                        }
                        
                    }
                        
                    if (IsNumber(orderByAttr) && indexAttr < 0)
                    {
                        this._errorMessage = "The ORDER BY position number is out of range of the number of items in the select list.";
                        throw new Exception(this._errorMessage);
                    }  
                    else if (indexAttr < 0)
                    {
                        this._errorMessage = "Invalid attribute to order by";
                        throw new Exception(this._errorMessage);
                    } 
                    

                }
                if (listOrder[p].IndexOf(" ") < 0)
                    listOrder[p] = indexAttr.ToString();
                else
                {
                    string tmp = listOrder[p].Substring(listOrder[p].IndexOf(' ') + 1, listOrder[p].Count() - listOrder[p].IndexOf(' ') - 1);
                    listOrder[p] = indexAttr.ToString() + ' ' + tmp;
                }
                for (int u = 0; u < p; u++)
                {
                    int indexAttrTmp = Int32.Parse(new string(listOrder[u].TakeWhile(Char.IsDigit).ToArray()));
                    if (indexAttr == indexAttrTmp)
                    {
                        this._errorMessage = "A column has been specified more than once in the order by list. Columns in the order by list must be unique.";
                        throw new Exception(this._errorMessage);
                    }
                        

                }
            }
            return listOrder;
        }

        private string checkDistinctSelect()
        {
            string message = "";
            for(int i = 0; i < this._selectedAttributeTexts.Count(); i++)
            {
                if (i != 0 && this._selectedAttributeTexts[i].Contains("distinct") && !this._selectedAttributeTexts[i].Contains("count("))
                    message = "Incorrect syntax near 'distinct'";
                
            }
            return message;
        }

        private bool IsNumber(string pText)
        {
            Regex regex = new Regex(@"^[-+]?[0-9]*\.?[0-9]+$");
            return regex.IsMatch(pText);
        }





        //private String CheckDataType_AggregateFunction(int indexAttribute)
        //{
        //    String dataType = this._selectedRelations[0].Scheme.Attributes[indexAttribute].DataType.DataType;
        //}
        private String CheckGroupByExistInSelect(string[] filterStr)
        {
            Boolean flag = false;
            String message = "", tmp = "";
            string[] attributeTmp;
            List<string> Attributes = new List<string>();
            message = CheckExistAttribute(filterStr);// check exist attribute
            if (this._selectedAttributeTexts != null)
            {
                for (int k = 0; k < this._selectedAttributeTexts.Count(); k++)
                {
                    Attributes.Add(this._selectedAttributeTexts[k].ToString());
                }
            }
            if (this._selectedAttributeTexts == null || this._selectedAttributeTexts.Contains<String>("*") || this._selectedAttributeTexts.Contains<String>("-distinct-*")) //case select *,.. or select *
            {
                for (int k = 0; k < this._selectedRelations[0].Scheme.Attributes.Count() - 1; k++)
                {
                    //tmp += this._selectedRelations[0].Scheme.Attributes.ElementAt(k).AttributeName.ToString() + ",";
                    //Array.Resize(ref Attributes, Attributes.Count() + 1);
                    Attributes.Add(this._selectedRelations[0].Scheme.Attributes.ElementAt(k).AttributeName.ToString());
                }
                Attributes = Attributes.Where(s => s != "*").ToList();
                Attributes = Attributes.Where(s => s != "-distinct-*").ToList();
                //Attributes = Attributes.ToArray();
                //tmp = tmp.Substring(0, tmp.Length - 1);
                //tmpAttributes = tmp.Split(',');

                //Attributes = tmp.Split(',');
                //Attributes.Add

                //sortedTuple.Select((x, o) => h == o ? F1 : x).ToList();
                //tmp = "";
            }
            for (int i = 0; i < Attributes.Count; i++)
            {
                tmp = Attributes[i].ToLower();
                if (tmp.Contains("-distinct-"))
                    tmp = tmp.Substring(10);
                if (tmp.Contains("-as-"))
                    tmp = tmp.Substring(0, tmp.IndexOf("-as-"));
                if (!tmp.Contains("(") && !tmp.Contains(")"))
                {
                    flag = false;
                    for (int j = 0; j < filterStr.Length; j++)
                    {
                        if (filterStr[j].ToLower() == tmp.ToLower())
                        {
                            flag = true;
                            break;
                        }
                        //if (filterStr[j].ToLower() == tmp)
                        //{
                        //    flag = true;
                        //    break;
                        //}

                    }
                    if (!flag)
                        return message = "Invalid attribute in the select list because it is not contained in either an aggregate function or the GROUP BY clause.";
                }
            }
            return message;
        }

        private FzRelationEntity ProcessGroupBy(FzRelationEntity result)
        {
            List<Filter> filter = FormatFilter(result);
            Filter filterTmp = new Filter();
            FzRelationEntity filterResult = new FzRelationEntity();//result of group by
            List<FzRelationEntity> filterResultHavings = new List<FzRelationEntity>();
            FzRelationEntity filterResultHaving = new FzRelationEntity();
            FzRelationEntity resultTmp = new FzRelationEntity();
            List<List<Item>> listConditionForTuples = new List<List<Item>>();//having: condition1-true and condition2 false, to get priority tuple after process having condition.
            foreach (FzTupleEntity tuple in result.Tuples)
                resultTmp.Tuples.Add(tuple);
            foreach (FzAttributeEntity attr in result.Scheme.Attributes)
                resultTmp.Scheme.Attributes.Add(attr);
           
            List<int> indexGroupby = new List<int>();
            List<Item> itemConditionGroupBys = new List<Item>();
            filterResult.Scheme.Attributes = this._selectedAttributes;
            if (filter.Count > 0)// if format filter group by scuccess
            {
                filterTmp = filter[0];
                foreach (List<String> item in filterTmp.elementValue)//each different value in tuple
                {
                    int index = 0;
                    foreach (var itemAttr in resultTmp.Scheme.Attributes)// each attributes in result of selecting
                    {
                        //item[0] is name of attribute && index is not index of membership
                        if (item[0] == itemAttr.AttributeName.ToLower() && index != resultTmp.Scheme.Attributes.Count - 1)
                        {
                            for (int h = 0; h <= resultTmp.Tuples.Count - 1; h++)//each tuple
                            {
                                //index of group by Condition
                                if (!indexGroupby.Contains(index))
                                {
                                    indexGroupby.Add(index);
                                }
                                //if different value = value on tuple && still have value
                                if (item.Contains(resultTmp.Tuples[h].ValuesOnPerRow[index].ToString()) && item.Count > 1 && indexGroupby.Count >= 2)
                                {
                                    int countTupleFlagTrue = 0;
                                    //find below tuple have the same value with tuple in filterResult?
                                    for (int k = 0; k < filterResult.Tuples.Count; k++)
                                    {
                                        int CountSame = 0;
                                        for (int y = 0; y < indexGroupby.Count; y++)
                                        {
                                            //if they are the same=> count
                                            if (resultTmp.Tuples[h].ValuesOnPerRow[indexGroupby[y]].ToString() == filterResult.Tuples[k].ValuesOnPerRow[indexGroupby[y]].ToString())
                                            {
                                                CountSame++;
                                            }
                                        }
                                        //if same completely with all tuples in filterResult, break
                                        if (CountSame == indexGroupby.Count)
                                        {
                                            //result.Tuples.RemoveAt(h);
                                            break;
                                        }

                                        else if (CountSame < indexGroupby.Count && CountSame >= 0)
                                            countTupleFlagTrue++;
                                        //if not same slightly
                                        if (countTupleFlagTrue == filterResult.Tuples.Count)
                                        {
                                            filterResult.Tuples.Add(resultTmp.Tuples[h]);//add tuple to filterResult
                                            item.Remove(resultTmp.Tuples[h].ValuesOnPerRow[index].ToString());
                                            //remove condition group by
                                            break;
                                        }
                                    }
                                }
                                else if (item.Contains(resultTmp.Tuples[h].ValuesOnPerRow[index].ToString()) && item.Count > 1 && indexGroupby.Count < 2)
                                {
                                    filterResult.Tuples.Add(resultTmp.Tuples[h]);
                                    item.Remove(resultTmp.Tuples[h].ValuesOnPerRow[index].ToString());
                                }
                                else if (!item.Contains(resultTmp.Tuples[h].ValuesOnPerRow[index].ToString()) && item.Count <= 1) break;
                            }
                        }
                        index++;
                    }
                }
            }
            int having = _queryText.IndexOf(" having ");
            resultTmp.Tuples.Clear();
            //filterResult.Tuples = filterResult.Tuples.AsEnumerable().OrderBy(x => x.ValuesOnPerRow[indexGroupby[0]]).Select(x => x).ToList();
            //filterResult.Tuples.RemoveRange(0, filterResult.Tuples.Count() / 2);
            if (having < 0)//get select attribute with group by (without having condition)
            {
                QueryConditionBLL condition = new QueryConditionBLL(_fdbEntity);
                int countTuple = filterResult.Tuples.Count();
                int j = 0;
                while(j < countTuple)//format each tuple after group by
                {
                    for (int l = 0; l < indexGroupby.Count; l++)
                    {
                        if(l == 0)
                            filterResultHaving.Tuples = result.Tuples.Where(s => s.ValuesOnPerRow[indexGroupby[l]].ToString() == filterResult.Tuples[j].ValuesOnPerRow[indexGroupby[l]].ToString()).ToList();
                        else
                        {
                            filterResultHaving.Tuples.RemoveAll(x => x.ValuesOnPerRow[indexGroupby[l]].ToString() != filterResult.Tuples[j].ValuesOnPerRow[indexGroupby[l]].ToString());
                        }
                        if (filterResultHaving.Tuples.Count == 1 || l == indexGroupby.Count() - 1)
                        {
                            //if (itemSelects.Count() == 0 && filterResultHaving.Tuples.Count > 1)// unless select aggregate function
                            //{
                            //    var arrMembership = filterResultHaving.Tuples.Select(x => x.ValuesOnPerRow[this._selectedRelations[0].Scheme.Attributes.Count() - 1].ToString());
                            //    List<String> membershipList = arrMembership.ToList();
                            //    string FSName = "";
                            //    for (int i = 0; i < membershipList.Count(); i++)
                            //    {
                            //        FSName = condition.FindAndMarkFuzzy(membershipList[i].ToString(), FSName);
                            //    }
                            //    foreach (var item in filterResultHaving.Tuples.Where(x => x.ValuesOnPerRow[this._selectedRelations[0].Scheme.Attributes.Count() - 1].ToString() != FSName))
                            //    {
                            //        item.ValuesOnPerRow[this._selectedRelations[0].Scheme.Attributes.Count() - 1] = FSName;
                            //    }

                            //}
                            resultTmp.Tuples.AddRange(GetSelectedAttributes(filterResultHaving.Tuples, _fdbEntity));
                            filterResult.Tuples.RemoveAt(j);
                            countTuple = filterResult.Tuples.Count();
                            break;
                        }

                    }
                    filterResultHaving.Tuples.Clear();//renew filterResultHaving
                }
            }
            if (having > 0)
            {
                string tmp = _queryText.Substring(having);
                tmp = GetConditionText(tmp);//get condition Text 'having'(not where)
                if (tmp != String.Empty)
                    tmp = AddParenthesis(tmp);
                List<Item> itemTmp= FormatCondition(tmp);
                bool checkAttr = false;
                //Check fuzzy set and object here
                this._errorMessage = ExistsFuzzySet(itemTmp);
                for (int k = 0; k < itemTmp.Count(); k++)
                {
                    for(int l = 0; l < indexGroupby.Count(); l++)
                    {
                        if ((itemTmp[k].aggregateFunction == "" && itemTmp[k].elements[0].ToString() == indexGroupby[l].ToString()) || itemTmp[k].aggregateFunction != "")
                        {
                            checkAttr = true;
                            break;
                        }
                    }
                    if(!checkAttr)
                        this._errorMessage = "Invalid attribute in HAVING clause because it is not contained in either an aggregate function or the GROUP BY clause";
                    checkAttr = false;
                }
                if(ErrorMessage != "") { this.Error = true; throw new Exception(_errorMessage); }

                for (int j = 0; j < filterResult.Tuples.Count; j++)//format each tuple after group by
                {
                    //for (int l = 0; l < indexGroupby.Count; l++)
                    //{
                    //    Item itemConditionGroupBy = new Item();//set condition from group by
                    //    itemConditionGroupBy.elements.Add(indexGroupby[l].ToString());//index
                    //    itemConditionGroupBy.elements.Add("=");//operator
                    //    itemConditionGroupBy.elements.Add(filterResult.Tuples[j].ValuesOnPerRow[indexGroupby[l]].ToString());
                    //    //value
                    //    if (l < indexGroupby.Count - 1)
                    //        itemConditionGroupBy.nextLogic = " and ";// and, it must have 'and' if group by more than 2 attributes
                    //    itemConditionGroupBys.Add(itemConditionGroupBy);
                    //}
                    //QueryConditionBLL condition_GroupBy = new QueryConditionBLL(itemConditionGroupBys, this._selectedRelations, _fdbEntity);
                    //foreach (FzTupleEntity tuple in this._selectedRelations[0].Tuples)
                    //{
                    //    if (condition_GroupBy.Satisfy(itemConditionGroupBys, tuple) != "0")
                    //    {
                    //        filterResultHaving.Tuples.Add(tuple);// get tuples meet itemConditionGroupBys
                    //    }
                    //}
                    for (int l = 0; l < indexGroupby.Count; l++)
                    {
                        if (l == 0)
                            filterResultHaving.Tuples = result.Tuples.Where(s => s.ValuesOnPerRow[indexGroupby[l]].ToString() == filterResult.Tuples[j].ValuesOnPerRow[indexGroupby[l]].ToString()).ToList();
                        else
                        {
                            filterResultHaving.Tuples.RemoveAll(x => x.ValuesOnPerRow[indexGroupby[l]].ToString() != filterResult.Tuples[j].ValuesOnPerRow[indexGroupby[l]].ToString());
                        }
                    }
                    filterResultHaving.Scheme = this._selectedRelations[0].Scheme;
                    filterResultHavings.Add(filterResultHaving);
                    //itemConditionGroupBys = new List<Item>();
                    if (filterResultHaving.Tuples.Count > 0)
                    {
                        List<Item> itemConditionHavings = FormatCondition(tmp);// format condition having
                        QueryConditionBLL condition_Having = new QueryConditionBLL(itemConditionHavings, filterResultHavings, _fdbEntity);

                        for (int w = 0; w < itemConditionHavings.Count; w++)
                        {
                            if (itemConditionHavings[w].aggregateFunction != "") // if min, max, count,...
                            {
                                itemConditionHavings[w] = condition_Having.findAndMarkAggregatetion(itemConditionHavings[w], filterResultHaving.Tuples);//find min, max, .. and add value min, max ,.. to valueAggregate Item
                            }
                            itemConditionHavings[w].ItemName = "having";// use to distingue with another condition, apply for function 'Satisfy' in QueryConditionBLL
                        }
                        List<FzTupleEntity> tupleTmps = new List<FzTupleEntity>();
                        //resultTmp = new FzRelationEntity();
                        for (int q = 0; q < filterResultHaving.Tuples.Count; q++)// each tuple after filter with group by
                        {
                            List<Item> itemConditionHavings_Tmp = itemConditionHavings;
                            
                            if (condition_Having.Satisfy(itemConditionHavings_Tmp, filterResultHaving.Tuples[q]) != "0")
                            {

                                //FzTupleEntity tupleTmp = new FzTupleEntity();
                                //tupleTmps.Add(filterResultHaving.Tuples[q]);
                                tupleTmps.Add(condition_Having.ResultTuple);
                                //resultTmp.Add(condition.ResultTuple);
                                //if (itemSelects.Count == 0)
                                //    goto End;// if exist more than 1 tuple the same, break 2 loops
                                //for (int d = 0; d < condition_Having.ItemConditions.Count(); d++)
                                //{
                                //    if (condition_Having.ItemConditions[d].resultCondition)
                                //    {
                                //        resultTmp.Tuples.Add(filterResultHaving.Tuples[q]);
                                //        goto End;// if exist more than 1 tuple the same, break 2 loops
                                //    }
                                //}
                            }

                        }
                        //End:;//break 2 loops
                        if (tupleTmps.Count > 0)
                        {
                            resultTmp.Tuples.AddRange(GetSelectedAttributes(tupleTmps, _fdbEntity));
                        }
                        
                    }
                    else if (filterResultHaving.Tuples.Count > 0) return filterResult;
                    filterResultHaving = new FzRelationEntity();//renew filterResultHaving
                    
                }
                
            }
            
            resultTmp.Scheme.Attributes = this._selectedAttributes;
            //filterResult.Tuples.Clear();
            filterResult = resultTmp;
            //filterResult.Scheme.Attributes = this._selectedAttributes;
            return filterResult;
        }
        

        public List<Filter> FormatFilter(FzRelationEntity tupleRelation)
        {
            List<Filter> result = new List<Filter>();
            //try
            //{
                int groupby = 0, having, orderby, index = 0 ;
                groupby = _queryText.IndexOf(" group by ");
                //having = _queryText.IndexOf(" having ");
                //orderby = _queryText.IndexOf(" order by ");
                string[] filterStr = null;
                string tmp = "";
                //this._selectedAttributeTexts = null;
                if (groupby > 0)
                {
                    Filter filter = new Filter();
                    filter.filterName = "groupby";
                    //if (having < 0 && orderby < 0)
                    //{
                    //    tmp = _queryText.Substring(groupby + 10, _queryText.Length - groupby - 10);
                    //}
                    //else if (having > 0) // group by... + having ...
                    //{
                    //    tmp = _queryText.Substring(groupby + 10, having - groupby - 10);

                    //}//having
                    //else if (orderby > 0 && having < 0)// group by... + order by
                    //{
                    //    tmp = _queryText.Substring(groupby + 10, orderby - groupby - 10);
                    //}
                    //tmp = tmp.Replace(" ", "");
                    //filterStr = tmp.Split(',');
                    filterStr = getFilterGroupBy();
                    this._errorMessage = CheckGroupByExistInSelect(filterStr);// check whether valid select when having group by( select Age, min(Height) ..group by Age)
                    if (ErrorMessage != "") { this.Error = true; throw new Exception(_errorMessage); }

                    filter.elements = filterStr.ToList();
                    for (int h = 0; h < filter.elements.Count; h++)
                    {
                        filter.elementValue.Add(new List<string> { filter.elements[h].ToString() });
                        //example: age 30 10 32 and other data group by age
                    }
                    foreach (List<String> filterValue in filter.elementValue)
                    {
                        index = 0;
                        foreach (var itemAttr in tupleRelation.Scheme.Attributes)
                        {
                            if (filterValue[0] == itemAttr.AttributeName.ToLower() && index != tupleRelation.Scheme.Attributes.Count - 1)
                            {
                                foreach (var attrTuple in tupleRelation.Tuples)
                                {
                                    if (!filterValue.Contains(attrTuple.ValuesOnPerRow[index].ToString()))
                                    {

                                        filterValue.Add(attrTuple.ValuesOnPerRow[index].ToString());
                                    }
                                }
                            }

                            index++;
                        }
                    }

                    filter.elementValue = ArrangeFormatFilter(filter.elementValue);// add first attribute has maximum data to group by, and the second, third ,...
                    result.Add(filter);// arranged filter
                }// group by ...
                 
            //}
            //catch (Exception ex)
            //{
            //    this._error = true;
            //    this._errorMessage = ex.Message;
            //    //return result;
            //}


            return result;
            
        }

        public List<List<String>> ArrangeFormatFilter(List<List<String>> resultFormat)
        {
            List<List<String>> resultArrange = new List<List<String>>();
            int[] arr = new int[resultFormat.Count];
            int max = 0;
            for (int i = 0; i < resultFormat.Count; i++)
            {
                arr[i] = resultFormat[i].Count;
            }
            while(arr.Length > 0)
            {
                max = arr.Max();
                resultArrange.Add(resultFormat[Array.IndexOf(arr, max)]);
                resultFormat.RemoveAt(Array.IndexOf(arr, max));
                List<int> tmp = new List<int>(arr);
                tmp.RemoveAt(Array.IndexOf(arr, max));
                arr = tmp.ToArray();
            }
            return resultArrange;
        }
        public List<FzTupleEntity> ProcessDistinct(List<FzTupleEntity> tuples)
        {
            this._errorMessage = checkDistinctSelect();
            if(ErrorMessage != "") throw new Exception(this._errorMessage);
            List<FzTupleEntity> tupleTmp = new List<FzTupleEntity>();
            List<FzTupleEntity> tupleTmp2 = new List<FzTupleEntity>();
            List<FzTupleEntity> tupleResult = new List<FzTupleEntity>();
            for (int l = 0; l < tuples.Count(); l++)
                tupleTmp2.Add(tuples[l]);
            tupleResult = tuples.AsEnumerable().GroupBy(x => x.ValuesOnPerRow[0]).SelectMany(grouping => grouping.Take(1)).ToList();
            var attrDuplicate1 = from tuple in tuples
                                 group tuple by tuple.ValuesOnPerRow[0] into g
                                 where g.Count() > 1
                                 select g;
            if (attrDuplicate1.Count() > 0)//except memership => count() > 2
            {
                for(int i = 0; i < attrDuplicate1.Count(); i++)
                {
                    string s = attrDuplicate1.ElementAt(i).Key.ToString();
                    IEnumerable<FzTupleEntity> tupleDistinct = tupleTmp2.Where(x => x.ValuesOnPerRow[0].ToString() == attrDuplicate1.ElementAt(i).Key.ToString()).Select(x => x).ToList();
                    if (this._selectedAttributes.Count() > 2)
                        tupleTmp.AddRange(ProcessDistinct_Sub(tupleDistinct, attrDuplicate1, i));
                    else tupleTmp = tupleDistinct.Take(1).ToList();
                    int countTuples = tupleTmp2.Count();
                    int countTupleTmps = tupleTmp.Count();
                    for (int k = 0; k < countTuples; k++)
                    {
                        for (int j = 0; j < countTupleTmps; j++)
                        {
                            if (tupleTmp[j] == tupleTmp2[k])
                            {
                                //find and set min membership
                                String FsName = tupleTmp2[k].ValuesOnPerRow[this._selectedAttributes.Count() - 1].ToString();
                                QueryConditionBLL condition = new QueryConditionBLL(_fdbEntity);
                                for (int z = k + 1; z < countTuples; z++)
                                {
                                    int count = 0;
                                    for (int u = 0; u < this._selectedAttributes.Count() - 1; u++)
                                    {
                                        if (tupleTmp2[z].ValuesOnPerRow[u].ToString() == tupleTmp[j].ValuesOnPerRow[u].ToString())
                                            count++;
                                        if (count == this._selectedAttributes.Count() - 1)
                                        {
                                            FsName = condition.FindAndMarkFuzzy(FsName, tupleTmp2[z].ValuesOnPerRow[this._selectedAttributes.Count() - 1].ToString());
                                            tupleTmp2.RemoveAt(z);
                                            countTuples = tupleTmp2.Count();
                                            z--;
                                        }
                                            
                                    }
                                }
                                tupleTmp.RemoveAt(j);
                                tupleTmp2[k].ValuesOnPerRow[this._selectedAttributes.Count() - 1] = FsName;
                                countTupleTmps = tupleTmp.Count();
                                j--;
                                break;
                            }
                        }
                        
                    }
                    tupleTmp = new List<FzTupleEntity>();
                }
                tupleResult.Clear();
                tupleResult = tupleTmp2;


            }
            return tupleResult;
            
        }


        public List<FzTupleEntity> ProcessDistinct_Sub(IEnumerable<FzTupleEntity> listTuple, IEnumerable<IGrouping<object, FzTupleEntity>> attrDuplicate1, int k)
        {
            List<FzTupleEntity> tupleTmp = new List<FzTupleEntity>();
            List<FzTupleEntity> tupleResult = new List<FzTupleEntity>();
            int j = 0, countTuple = listTuple.Count();
            while (j < countTuple)
            {
                for (int i = 1; i < this._selectedAttributes.Count() - 1; i++)
                {
                    if (i == 1)
                    {
                        tupleTmp = listTuple.AsEnumerable().GroupBy(x => x.ValuesOnPerRow[i]).SelectMany(grouping => grouping).ToList();
                        //tupleTmp = listTuple.AsEnumerable().Where(x => x.ValuesOnPerRow[0].ToString() == attrDuplicate1.ElementAt(k).Key.ToString()).GroupBy(x => x.ValuesOnPerRow[i]).SelectMany(grouping => grouping).ToList();
                    }
                    else
                        tupleTmp = tupleTmp.AsEnumerable().GroupBy(x => x.ValuesOnPerRow[i]).SelectMany(grouping => grouping).ToList();
                   
                    if (tupleTmp.Count() == 1 || i == this._selectedAttributes.Count() - 2)//except membership
                    {
                        listTuple = listTuple.Except(tupleTmp);
                        tupleResult.AddRange(tupleTmp.AsEnumerable().GroupBy(x => x.ValuesOnPerRow[i]).SelectMany(grouping => grouping.Take(1)));
                        countTuple = listTuple.Count();
                        break;
                    }
                    else
                    {
                        string tmp = tupleTmp.AsEnumerable().Select(x => x.ValuesOnPerRow[i].ToString()).FirstOrDefault();
                        tupleTmp = tupleTmp.AsEnumerable().Where(item => item.ValuesOnPerRow[i].ToString() == tmp).Select(item => item).ToList();
                    }
                }
            }
            return tupleResult;
        }

        public FzRelationEntity ProcessNoOrderBy(FzRelationEntity OriginalRelation)
        {
            FzRelationEntity relation = OriginalRelation;
            string dataType = relation.Scheme.Attributes[0].DataType.DataType;
            if (dataType != "String")
            {
                for(int j = 0; j < relation.Tuples.Count(); j++)
                {
                    switch (dataType)
                    {
                        case "Int16":
                        case "Int64":
                        case "Int32":
                        case "Byte":
                        case "Currency": { relation.Tuples[j].ValuesOnPerRow[0] = Convert.ToInt32(relation.Tuples[j].ValuesOnPerRow[0]); break; }
                        case "Decimal":
                        case "Single":
                        case "Double":
                            { relation.Tuples[j].ValuesOnPerRow[0] = Convert.ToDouble(relation.Tuples[j].ValuesOnPerRow[0].ToString()); break; }
                        case "DateTime":
                            { relation.Tuples[j].ValuesOnPerRow[0] = DateTime.Parse(relation.Tuples[j].ValuesOnPerRow[0].ToString()); break; }

                    }
                }    
            }
            //FzRelationEntity result = new List<FzTupleEntity>();
            OriginalRelation.Tuples = relation.Tuples.OrderBy(x => x.ValuesOnPerRow[0]).Select(x=>x).ToList();
            OriginalRelation.Tuples.RemoveRange(0, OriginalRelation.Tuples.Count() / 2);
            return OriginalRelation;
       }

        public List<FzTupleEntity> ProcessOrderBy(List<FzTupleEntity> listOriginalTuple)
        {
            String[] listOrder = null;
            int orderBy = 0, indexAttr = 0;
            orderBy = this._queryText.IndexOf(" order by ");
            listOrder = this._queryText.Substring(orderBy + 10, this._queryText.Length - orderBy - 10).ToLower().Split(',');
            
            listOrder = CheckOrderByReturnList(listOrder);
            //convert value from string to exact data type
            if(_queryText.Contains(" group by ") || (!_queryText.Contains(" group by ") && itemSelects.Count() == 0))// !group by & no aggregate function
            {
                for (int i = 0; i < listOrder.Count(); i++)
                {
                    if (dataTypeForOrderBy[i] != "String")
                    {
                        int index = Int32.Parse(new string(listOrder[i].TakeWhile(Char.IsDigit).ToArray()));
                        for (int j = 0; j < listOriginalTuple.Count(); j++)
                        {
                            switch (dataTypeForOrderBy[i])
                            {
                                case "Int16":
                                case "Int64":
                                case "Int32":
                                case "Byte":
                                case "Currency": { listOriginalTuple[j].ValuesOnPerRow[index] = Convert.ToInt32(listOriginalTuple[j].ValuesOnPerRow[index]); break; }
                                case "Decimal":
                                case "Single":
                                case "Double":
                                    { listOriginalTuple[j].ValuesOnPerRow[index] = Convert.ToDouble(listOriginalTuple[j].ValuesOnPerRow[index].ToString()); break; }
                                case "DateTime":
                                    { listOriginalTuple[j].ValuesOnPerRow[index] = DateTime.Parse(listOriginalTuple[j].ValuesOnPerRow[index].ToString()); break; }

                            }

                        }
                    }
                }
            }
            

            List<FzTupleEntity> listTupleTmp = listOriginalTuple;
            IEnumerable<FzTupleEntity> sortedTuple = null;

            indexAttr = Int32.Parse(new string(listOrder[0].TakeWhile(Char.IsDigit).ToArray()));
            orderBy = listOrder[0].IndexOf(" desc");
         
            if (orderBy > 0)
            {
                sortedTuple = from tuple in listTupleTmp
                              orderby tuple.ValuesOnPerRow[indexAttr] descending
                              select tuple;
            }
            else
            {
                sortedTuple = from tuple in listTupleTmp
                              orderby tuple.ValuesOnPerRow[indexAttr] ascending
                              select tuple;
            }
            int countTuple0 = 0;
            countTuple0 = (from tuple in sortedTuple
                           group tuple by tuple.ValuesOnPerRow[indexAttr] into g
                           where g.Count() > 1
                           select g).Count();

            if (countTuple0 > 0 && listOrder.Length > 1)
            {
                var attrDuplicate1 = from tuple in sortedTuple
                                     group tuple by tuple.ValuesOnPerRow[indexAttr] into g
                                     where g.Count() > 1
                                     select g;
                List<FzTupleEntity> tupleResult = new List<FzTupleEntity>();
                for (int k = 0; k < attrDuplicate1.Count(); k++)
                {
                    string s = attrDuplicate1.ElementAt(k).Key.ToString();
                    IEnumerable<FzTupleEntity> sortedTuple1 = sortedTuple.Where(x => x.ValuesOnPerRow[indexAttr].ToString() == attrDuplicate1.ElementAt(k).Key.ToString()).Select(x => x).ToList();
                    tupleResult.AddRange(ProcessOrderBy_Sub(sortedTuple1, listOrder, attrDuplicate1, k, indexAttr));
                    int pos = sortedTuple.ToList().FindIndex(x => x.ValuesOnPerRow[indexAttr].ToString() == attrDuplicate1.ElementAt(k).Key.ToString());
                    for (int i = 0; i < tupleResult.Count(); i++)
                    {
                        sortedTuple = sortedTuple.Select((x, o) => pos == o ? tupleResult[i] : x).ToList();
                        pos++;
                    }
                    tupleResult = new List<FzTupleEntity>();
                }
            }
            return sortedTuple.ToList();
        }

        public List<FzTupleEntity> ProcessOrderBy_Sub(IEnumerable<FzTupleEntity> sortedTuple, string[] listOrder, IEnumerable<IGrouping<object, FzTupleEntity>> attrDuplicate1, int k, int indexAttr)
        {
            List<FzTupleEntity> sortedTuple1 = null;
            List<FzTupleEntity> tupleResult = new List<FzTupleEntity>();
            int countTuple = sortedTuple.Count();
            int j = 0;
            string[] filter = getFilterGroupBy();
            while (j < countTuple)
            {
                sortedTuple1 = null;
                for (int i = 1; i < listOrder.Length; i++)
                {
                    int indexAttr1 = Int32.Parse(new string(listOrder[i].TakeWhile(Char.IsDigit).ToArray()));
                    int orderBy1 = listOrder[i].IndexOf(" desc");
                    //string orderByAttr1 = listOrder[i].Split(' ').First();
                    //int indexAttr1 = 0;
                    //if (_queryText.Contains(" group by "))
                    //    indexAttr1 = IndexOfAttrGroupBy(orderByAttr1, filter);  
                    //else indexAttr1 = IndexOfAttr(orderByAttr1);
                    //if (IsNumber(orderByAttr1) && Int32.Parse(orderByAttr1) < this._selectedRelations[0].Scheme.Attributes.Count() - 1)//order by index
                    //    indexAttr1 = Int32.Parse(orderByAttr1);
                    //if (indexAttr1 < 0)
                    //{
                    //    if (IsNumber(orderByAttr1) && Int32.Parse(orderByAttr1) < this._selectedRelations[0].Scheme.Attributes.Count() - 1)//order by index
                    //        indexAttr1 = Int32.Parse(orderByAttr1);
                    //    else
                    //    {
                    //        if (IsNumber(orderByAttr1))
                    //            this._errorMessage = "The ORDER BY position number is out of range of the number of items in the select list.";
                    //        else this._errorMessage = "Invalid attribute to order by";
                    //        throw new Exception(this._errorMessage);
                    //    }
                    //}
                    if (i == 1)
                    {
                        sortedTuple1 = sortedTuple.AsEnumerable().Where(item => item.ValuesOnPerRow[indexAttr].ToString() == attrDuplicate1.ElementAt(k).Key.ToString())
                                            .GroupBy(item => item.ValuesOnPerRow[indexAttr1])
                                            .SelectMany(grouping => grouping).ToList();
                    }
                        
                    else
                    {
                        sortedTuple1 = sortedTuple1.AsEnumerable().Where(item => item.ValuesOnPerRow[indexAttr].ToString() == attrDuplicate1.ElementAt(k).Key.ToString())
                                            .GroupBy(item => item.ValuesOnPerRow[indexAttr1])
                                            .SelectMany(grouping => grouping).ToList();
                      
                    }
                    if (orderBy1 > 0)
                        sortedTuple1 = sortedTuple1.OrderByDescending(item => item.ValuesOnPerRow[indexAttr1]).ToList();
                    else
                        sortedTuple1 = sortedTuple1.OrderBy(item => item.ValuesOnPerRow[indexAttr1]).ToList();

                    

                    if (sortedTuple1.Count() == 1 || i == listOrder.Length - 1)
                    {
                        tupleResult.AddRange(sortedTuple1);
                        sortedTuple = sortedTuple.Except(sortedTuple1.ToList());
                        countTuple = sortedTuple.Count();
                        break;
                    }
                    else
                    {
                        string tmp = sortedTuple1.AsEnumerable().Select(x => x.ValuesOnPerRow[indexAttr1].ToString()).FirstOrDefault();
                        sortedTuple1 = sortedTuple1.AsEnumerable().Where(item => item.ValuesOnPerRow[indexAttr1].ToString() == tmp).Select(item => item).ToList();
                    }
                }
            }
            return tupleResult;
        }

        ////public FzRelationEntity ProcessOrderBy(FzRelationEntity relation)
        //{
        //    FzRelationEntity relationTmp = relation;
        //    String[] listOrder = null;
        //    int orderBy = 0, indexAttr = 0;
        //    string orderByAttr = "";
        //    orderBy = this._queryText.IndexOf(" order by ");
        //    listOrder = this._queryText.Substring(orderBy + 10, this._queryText.Length - orderBy - 10).ToLower().Split(',');
        //    IEnumerable<FzTupleEntity> sortedTuple = null;
        //    if (listOrder[0][0] == ' ')
        //        listOrder[0] = listOrder[0].Remove(0, 1);
        //    orderBy = listOrder[0].IndexOf(" desc");
        //    orderByAttr = listOrder[0].Split(' ').First();
        //    indexAttr = IndexOfAttr(orderByAttr);
        //    if (indexAttr < 0)
        //    {
        //        this._errorMessage = "Invalid attribute to order by";
        //        throw new Exception(this._errorMessage);
        //    }
        //    if (orderBy > 0)
        //    {
        //        sortedTuple = from tuple in relationTmp.Tuples
        //                      orderby tuple.ValuesOnPerRow[indexAttr] descending
        //                      select tuple;
        //    }
        //    else
        //    {
        //        sortedTuple = from tuple in relationTmp.Tuples
        //                      orderby tuple.ValuesOnPerRow[indexAttr] ascending
        //                      select tuple;
        //    }
        //    int countTuple0 = 0;
        //    countTuple0 = (from tuple in sortedTuple
        //                       group tuple by tuple.ValuesOnPerRow[indexAttr] into g
        //                       where g.Count() > 1
        //                       select g).Count();

        //    if(countTuple0 > 0)
        //    {
        //        var attrDuplicate1 = from tuple in sortedTuple
        //                             group tuple by tuple.ValuesOnPerRow[indexAttr] into g
        //                             where g.Count() > 1
        //                             select g;

        //        for (int k = 0; k < attrDuplicate1.Count(); k++)
        //        {
        //            for (int i = 1; i < listOrder.Length; i++)
        //            {
        //                IEnumerable<FzTupleEntity> sortedTuple1 = null;
        //                if (listOrder[i][0] == ' ')
        //                    listOrder[i] = listOrder[i].Remove(0, 1);
        //                int orderBy1 = listOrder[i].IndexOf(" desc");
        //                string orderByAttr1 = listOrder[i].Split(' ').First();
        //                int indexAttr1 = IndexOfAttr(orderByAttr1);
        //                if (indexAttr1 < 0)
        //                {
        //                    this._errorMessage = "Invalid attribute to order by";
        //                    throw new Exception(this._errorMessage);
        //                }
        //                for (int j = i - 1; j < i; j++)
        //                {
        //                    int orderBy2 = listOrder[j].IndexOf(" desc");
        //                    int indexAttr2 = IndexOfAttr(listOrder[j].Split(' ').First());
        //                    if (orderBy1 > 0)
        //                    {
        //                        sortedTuple1 = sortedTuple.Where(item => item.ValuesOnPerRow[indexAttr].ToString() == attrDuplicate1.ElementAt(k).Key.ToString())
        //                                        .GroupBy(item => item.ValuesOnPerRow[indexAttr2])
        //                                        .SelectMany(grouping => grouping)
        //                                        .OrderByDescending(item => item.ValuesOnPerRow[indexAttr1]);
        //                    }
        //                    else
        //                    {
        //                        string ss = sortedTuple.ElementAt(0).ValuesOnPerRow[indexAttr].ToString();
        //                        string gg = attrDuplicate1.ElementAt(k).ToString();
        //                        int ii2 = indexAttr2;
        //                        int ii1 = indexAttr1;
        //                        sortedTuple1 = sortedTuple.Where(item => item.ValuesOnPerRow[indexAttr].ToString() == attrDuplicate1.ElementAt(k).Key.ToString())
        //                                        .GroupBy(item => item.ValuesOnPerRow[indexAttr2])
        //                                        .SelectMany(grouping => grouping)
        //                                        .OrderBy(item => item.ValuesOnPerRow[indexAttr1]);
        //                    }
        //                    if (sortedTuple1.Count() > 0)
        //                    {
        //                        int f = 0;
        //                        int length = sortedTuple.Count();
        //                        for (int h = 0; h < sortedTuple.Count(); h++)
        //                        {
        //                            if(sortedTuple.ElementAt(h).ValuesOnPerRow[indexAttr2].ToString() == attrDuplicate1.ElementAt(k).Key.ToString())
        //                            {
        //                                FzTupleEntity F1 = new FzTupleEntity();
        //                                for(int p = f; p < sortedTuple1.Count(); p++)
        //                                {
        //                                    F1 = sortedTuple1.ElementAt(p);
        //                                    break;
        //                                }
        //                                sortedTuple = sortedTuple.Select((x, o) => h == o ? F1 : x).ToList();
        //                                f++;

        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //    }
        //    List<FzTupleEntity> relation2 = new List<FzTupleEntity>();
        //    foreach (var item in sortedTuple)
        //    {
        //        relation2.Add(item);
        //    }
        //    relationTmp.Tuples.Clear();
        //    foreach (var item in relation2)
        //    {
        //        relationTmp.Add(item);
        //    }
        //    return relationTmp;
        //}
        #endregion
    }
    
   


    public class Item
    {
        public List<String> elements = new List<string>();
        public String nextLogic = "";
        public bool notElement = false;
        public String aggregateFunction = "";
        public bool resultCondition = false;
        public double valueAggregate = 0;
        public string ItemName = ""; //where or having
        public string attributeNameAs = "";
        //public bool isDistinct = false;
    }

    


    public class Filter
    {
        public String filterName = "";
        public List<List<String>> elementValue = new List<List<string>>();
        public List<String> elements = new List<string>();
    }

}
