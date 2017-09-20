using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FRDB_SQLite.Biz;

namespace FRDB_SQLite.Biz
{
    public class QueryExecutionBLLv2
    {
        private String _queryText;
        private String _queryType;
        private FzRelationEntity _queryResult;
        private FdbEntity _fdbEntity;
        private String _errorMessage = String.Empty;

        public QueryExecutionBLLv2(String queryText, FdbEntity fdbEntity)
        {
            this._queryText = queryText.ToLower();
            this._fdbEntity = fdbEntity;

            QueryAnalyze();
        }

        private void QueryAnalyze()
        {
            QueryConditionBLL condition = new QueryConditionBLL();
            if (this._queryText.Contains("union"))
            {
                this._queryType = Constants.QUERY_TYPE.MUTIPLE;
            }
            if (this._queryType == Constants.QUERY_TYPE.MUTIPLE)
            {
                List<String> singleQueries = SplitQuery(this._queryText);
                if (this._queryText.Contains("union"))
                {
                    String[] fstQueryAttributes = GetAttributeTexts(singleQueries[0]);
                    String[] sndQueryAttributes = GetAttributeTexts(singleQueries[1]);
                    bool isEqual = fstQueryAttributes.SequenceEqual(sndQueryAttributes);
                    if (!isEqual)
                    {
                        this._errorMessage = "The select statements do not have the same result columns";
                        throw new Exception(this._errorMessage);
                    }
                    String[] fstRelationTexts = GetRelationTexts(singleQueries[0]);
                    String[] sndRelationTexts = GetRelationTexts(singleQueries[0]);
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
                        if (fstAttr.DataType != sndAttr.DataType)
                        {
                            this._errorMessage = "The select statements do not have the same result columns";
                            throw new Exception(_errorMessage);
                        }
                    }
                    // The SQL statement with Union is now ready to process
                    QueryExcutetionBLL fstExecution = new QueryExcutetionBLL(singleQueries[0], this._fdbEntity);
                    QueryExcutetionBLL sndExecution = new QueryExcutetionBLL(singleQueries[1], this._fdbEntity);
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
                    List<FzTupleEntity> newTuples = new List<FzTupleEntity>(fstQueryResult.Tuples);
                    List<FzTupleEntity> tmpTuples = new List<FzTupleEntity>();
                    foreach (FzTupleEntity sndRelationTuple in sndQueryResult.Tuples)
                    {
                        bool isTupleEqual = false;
                        foreach (FzTupleEntity fstRelationTuple in newTuples)
                        {
                            if (fstRelationTuple.Equals(sndRelationTuple))
                            {
                                isTupleEqual = true;
                                String minFS = String.Empty;
                                String fstMemberShip = fstRelationTuple.MemberShip;
                                String sndMemberShip = sndRelationTuple.MemberShip;
                                DisFS fstDisFS = condition.getDisFS(fstMemberShip, _fdbEntity);
                                DisFS sndDisFS = condition.getDisFS(sndMemberShip, _fdbEntity);
                                if (fstDisFS == null && sndDisFS == null)
                                {
                                    minFS = Math.Min(Convert.ToDouble(fstMemberShip), Convert.ToDouble(sndMemberShip)).ToString();
                                }
                                else if (fstDisFS == null)
                                {
                                    fstDisFS = new DisFS(Convert.ToDouble(fstMemberShip));
                                    minFS = condition.Min_DisFS(fstDisFS, sndDisFS);
                                }
                                else
                                {
                                    sndDisFS = new DisFS(Convert.ToDouble(sndMemberShip));
                                    minFS = condition.Min_DisFS(fstDisFS, sndDisFS);
                                }
                                fstRelationTuple.MemberShip = minFS;
                            }
                        }
                        if (!isTupleEqual)
                        {
                            tmpTuples.Add(sndRelationTuple);
                        }
                    }
                    unionResult.Tuples = newTuples.Concat(tmpTuples).ToList();
                    this._queryResult = unionResult;
                }
            }
        }

        public FzRelationEntity ExecuteQuery()
        {
            return this._queryResult;
        }

        private List<String> SplitQuery(String mutipleQueryText)
        {
            List<String> singleQueries = new List<String>();
            singleQueries = mutipleQueryText.Split(new String[] { " union " }, StringSplitOptions.None).ToList();
            return singleQueries;
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
    }
}
