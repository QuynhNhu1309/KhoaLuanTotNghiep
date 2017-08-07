using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FRDB_SQLite
{
    public class FzQueryEntity   //This contents string to query from db
    {
        #region 1. Fields

        private String _queryName;//Each query has a name
        private String _queryString;// And each queryName contents a string query

        #endregion

        #region 2. Properties

        public String QueryName
        {
            get { return _queryName; }
            set { _queryName = value; }
        }

        public String QueryString
        {
            get { return _queryString; }
            set { _queryString = value; }
        }

        #endregion

        #region 3. Contructors

        public FzQueryEntity()
        {
            this._queryName = String.Empty;
            this._queryString = String.Empty;
        }

        public FzQueryEntity(String queryString)
        {
            this._queryName = String.Empty;
            this._queryString = queryString;
        }

        public FzQueryEntity(String queryName, String queryString)
        {
            this._queryName = queryName;
            this._queryString = queryString;
        }

        #endregion

        #region 4. Methods


       
        #endregion

        #region 5. Privates



        #endregion
    }
}
