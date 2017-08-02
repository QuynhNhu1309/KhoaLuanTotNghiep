using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FRDB_SQLite;
using System.IO;

namespace FRDB_SQLite
{
    public class QueryExcutetionBLL
    {
        #region 1. Fields
        private static String[] _operators = { "<", ">", "<=", ">=", "!=", "<>", "=" };
        private String[] _selectedAttributeTexts = null;
        private String[] _selectedRelationTexts = null;
        public String _conditionText = String.Empty;

        private List<FzAttributeEntity> _selectedAttributes = new List<FzAttributeEntity>();
        private List<FzRelationEntity> _selectedRelations = new List<FzRelationEntity>();
        List<int> _index = new List<int>();

        private String _queryText;
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
            FzRelationEntity result = new FzRelationEntity();
            try
            {
                this.GetSectedRelation(); if (this._error) throw new Exception(this._errorMessage);
                this.GetSelectedAttr(); if (this._error) throw new Exception(this._errorMessage);

                _errorMessage = ExistsAttribute();
                if (ErrorMessage != "") { this.Error = true; throw new Exception(_errorMessage); }


                if (this._queryText.Contains("where"))
                {
                    List<Item> items = FormatCondition(this._conditionText);
                    //Check fuzzy set and object here
                    this.ErrorMessage = ExistsFuzzySet(items);
                    if (ErrorMessage != "") { this.Error = true; return result; }
                    
                    QueryConditionBLL condition = new QueryConditionBLL(items, this._selectedRelations,_fdbEntity);
                    result.Scheme.Attributes = this._selectedAttributes;
                    
                    foreach (FzTupleEntity tuple in this._selectedRelations[0].Tuples)
                    {
                        if (condition.Satisfy(items, tuple)!="0")
                        {
                            if (this._selectedAttributeTexts != null)
                                result.Tuples.Add(GetSelectedAttributes(condition.ResultTuple));
                            else
                                result.Tuples.Add(condition.ResultTuple);
                        }
                    }
                }
                else// Select all tuples
                {
                    result.Scheme.Attributes = this._selectedAttributes;
                    result.RelationName = this._selectedRelations[0].RelationName;

                    if (this._selectedAttributeTexts != null)
                    {
                        foreach (var item in this._selectedRelations[0].Tuples)
                            result.Tuples.Add(GetSelectedAttributes(item));
                    }
                    else
                    {
                        foreach (var item in this._selectedRelations[0].Tuples)
                            result.Tuples.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                this._error = true;
                this._errorMessage = ex.Message;
                return result;
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
                if (condition[i] == '(')//(young=age) and (weight>=20 or height<=60)
                {
                    j = i + 1;
                    while (condition[j] != ')') j++;// Get index of ')'

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
                        }
                        else
                            item.nextLogic = condition.Substring(j + 1, k - j - 1);
                    }

                    result.Add(item);
                    if (j != condition.Length - 1)
                    {
                        if (flag)
                            i = k - 5;
                        else
                            i = k - 1;
                    }
                    else i = j - 1;// end of the condition
                }
                i++;
            }

            return result;
        }
        #endregion

        #region 5. Privates
        private void QueryAnalyze()
        {
            try
            {
                ///Get selected attribute which user input
                this._selectedAttributeTexts = GetAttributeTexts(this._queryText);
                
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
                if (this._selectedAttributeTexts != null)
                {
                    foreach (String text in this._selectedAttributeTexts)
                    {
                        int i = 0;
                        foreach (FzAttributeEntity attr in this._selectedRelations[0].Scheme.Attributes)
                        {
                            if (text.Equals(attr.AttributeName.ToLower()))
                            {
                                this._selectedAttributes.Add(attr);
                                _index.Add(i);
                            }
                            i++;
                        }
                    }
                    // Add the membership attribute
                    this._selectedAttributes.Add(this._selectedRelations[0].Scheme.Attributes[this._selectedRelations[0].Scheme.Attributes.Count - 1]);
                    //_index.Add(this._selectedRelations[0].Scheme.Attributes.Count - 1);// Add
                }
                else
                {
                    // Mean select * from ...
                    this._selectedAttributes = this._selectedRelations[0].Scheme.Attributes;
                }
            }
            catch (Exception ex)
            {
                this._error = true;
                this._errorMessage = ex.Message;
            }
        }

        private FzTupleEntity GetSelectedAttributes(FzTupleEntity resultTuple)
        {
            FzTupleEntity r = new FzTupleEntity();
            for (int i = 0; i < _index.Count; i++)
            {
                for (int j = 0; j < resultTuple.ValuesOnPerRow.Count; j++)
                {
                    if (_index[i] == j)
                    {
                        r.ValuesOnPerRow.Add(resultTuple.ValuesOnPerRow[j]);
                        break;
                    }
                }
            }
            r.ValuesOnPerRow.Add(resultTuple.ValuesOnPerRow[resultTuple.ValuesOnPerRow.Count - 1]);
            return r;
        }

        private String[] GetAttributeTexts(String s)
        {//the attributes which user input such: select attr1, att2... from
            String[] result = null;
            //String was standardzied and cut space,....
            if (!s.Contains("*"))
            {
                int i = 7;//Attribute after "select"
                int j = s.IndexOf("from");
                String tmp = s.Substring(i, j - i);
                tmp = tmp.Replace(" ", "");
                result = tmp.Split(',');
            }

            return result;
        }

        private String[] GetRelationTexts(String s)
        {//the relations which user input such: select attr1, att2... from
            String[] result = null;
            //String was standardzied and cut space,....
            int i = s.IndexOf("from") + 5;
            int j = s.Length;//query text doesn't contain any conditions
            if (s.Contains("where"))//query text contains conditions
            {
                j = s.IndexOf("where");
            }
            String tmp = s.Substring(i, j - i);
            tmp = tmp.Replace(" ", "");
            result = tmp.Split(',');

            return result;
        }

        private String GetConditionText(String s)
        {//the relations which user input such: select attr1, att2... from
            String result = String.Empty;
            //String was standardzied and cut space,....
            if (s.Contains("where"))
            {
                int i = s.IndexOf("where") + 6;// form where to the end of the string s (i is the first index to cut )
                result = s.Substring(i);
            }

            return result;
        }

        private int IndexOfAttr(String s)
        {
            for (int i = 0; i < this._selectedRelations[0].Scheme.Attributes.Count; i++)
            {
                if (s.Equals(this._selectedRelations[0].Scheme.Attributes[i].AttributeName.ToLower()))
                {
                    return i;
                }
            }
            return -1;
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
                if (index >= 0)
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
            if (expression.Contains("not"))
            {
                if (expression.Contains("not like"))
                {
                    expression = expression.Insert(expression.IndexOf("not"), "|");
                    expression = expression.Insert(expression.IndexOf("like") + 4, "|");

                }
                else
                {
                    expression = expression.Insert(expression.IndexOf("not") + 3, "|");
                }   
            }
            if (expression.Contains("like") && !expression.Contains("not like"))
            {
                expression = expression.Insert(expression.IndexOf("like") , "|");
                expression = expression.Insert(expression.IndexOf("like") + 4, "|");

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
            for (int i = 0; i < condition.Length - 5; i++)
            {
                String logic = condition.Substring(i, 5);// " and ", " or "
                if (condition[i] == '(')
                {
                    int j = i + 1;
                    while (condition[j] != ')') i = j++;
                    i -= 5;// Prevent Index was outside the bounds of the array
                }
                else
                {
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
                            else if (condition.Substring(i + 5, 4) != "not ")
                            {
                                condition = condition.Insert(i + 5, "(");
                                condition = condition.Insert(i, ")");
                                i += 2;
                            }
                        }
                        else if (condition[i - 1] == ')' && condition[i + 5] != '(')
                        {
                            if (condition.Substring(i + 5, 4) == "not " && condition[i + 9] != '(')
                                condition = condition.Insert(i + 9, "(");
                            else if (condition.Substring(i + 5, 4) != "not ")
                                condition = condition.Insert(i + 5, "(");
                            i +=5;
                        }
                        else if (condition[i - 1] != ')' && condition[i + 5] == '(')
                            condition = condition.Insert(i++, ")");
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
                            else if (condition.Substring(i + 4, 4) != "not ")
                                condition = condition.Insert(i + 4, "(");
                            i +=5;
                        }
                        else if (condition[i - 1] != ')' && condition[i + 4] == '(')
                            condition = condition.Insert(i++, ")");
                        i += 3;// Jump to the '('
                    }
                }
            }
            if (condition[0] != '(')
            {
                if (condition.Substring(0, 4) == "not ")
                    condition = condition.Insert(4, "(");
                else
                    condition = condition.Insert(0, "(");
            }
            if (condition[condition.Length - 1] != ')')
                condition += ")";

            return condition;
        }

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
        #endregion
    }

    public class Item
    {
        public List<String> elements = new List<string>();
        public String nextLogic = "";
        public bool notElement = false;
    }
}
