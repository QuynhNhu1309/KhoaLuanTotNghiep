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
        private FdbEntity _fdbEntity;
        public QueryExecutionBLLv2(String queryText, FdbEntity fdbEntity)
        {
            this._queryText = queryText.ToLower();
            this._fdbEntity = fdbEntity;

            QueryAnalyze();
        }

        private void QueryAnalyze()
        {
            if (this._queryText.Contains("union"))
            {
                this._queryType = Constants.QUERY_TYPE.MUTIPLE;
            }
            if (this._queryType == Constants.QUERY_TYPE.MUTIPLE)
            {
                List<String> singleQueries = SplitQuery(this._queryType);
            }
        }

        private List<String> SplitQuery(String mutipleQueryText)
        {
            List<String> singleQueries = new List<String>();
            singleQueries = mutipleQueryText.Split(new String[] { "contains" }, StringSplitOptions.None).ToList();
            return singleQueries;
        }
    }
}
