using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;


namespace FRDB_SQLite
{
    public class FdbEntity: ICloneable //This is a fuzzy relational database (FRDB)
    {
        #region 1. Fields

        private String _fdbName;
        private String _fdbPath;    
        private List<FzSchemeEntity> _schemes;
        private List<FzRelationEntity> _relations;
        private List<FzQueryEntity> _queries;
        private String _connString;
        private DataSet _dataSet;
        private List<FzDiscreteFuzzySetEntity> _discreteFuzzyNumbers;
        private List<FzContinuousFuzzySetEntity> _continuousFuzzyNumbers;
        #endregion

        #region 2. Properties

        public String FdbName
        {
            get { return _fdbName; }
            set { _fdbName = value; }
        }

        public String FdbPath
        {
            get { return _fdbPath; }
            set { _fdbPath = value; }
        }

        public List<FzSchemeEntity> Schemes
        {
            get { return _schemes; }
            set { _schemes = value; }
        }

        public List<FzRelationEntity> Relations
        {
            get 
            {
                //List<FzRelationEntity> result = new List<FzRelationEntity>();
                //foreach (FzRelationEntity item in this._relations)
                //{
                //    FzRelationEntity tmp = new FzRelationEntity(item);
                //    result.Add(tmp);
                //}
                //return result;
                return _relations;
            }
            set 
            {
                foreach (FzRelationEntity item in value)
                {
                    FzRelationEntity tmp = new FzRelationEntity(item);
                    _relations.Add(tmp);
                }
                //_relations = value;
            }
        }

        public List<FzQueryEntity> Queries
        {
            get { return _queries; }
            set { _queries = value; }
        }

        public String ConnString
        {
            get { return _connString; }
            set { _connString = value; }
        }

        public DataSet DataSet
        {
            get { return _dataSet; }
            set { _dataSet = value; }
        }

        public List<FzDiscreteFuzzySetEntity> DiscreteFuzzyNumbers
        {
            get { return _discreteFuzzyNumbers; }
            set { _discreteFuzzyNumbers = value; }
        }
        public List<FzContinuousFuzzySetEntity> ContinuousFuzzyNumbers
        {
            get { return _continuousFuzzyNumbers; }
            set { _continuousFuzzyNumbers = value; }
        }
        #endregion

        #region 3. Contructors

        public object Clone()//To copy to another area memory
        {
            return this.MemberwiseClone();
        }

        public FdbEntity()
        {
            this._fdbName = String.Empty;
            this._fdbPath = String.Empty;
            this._schemes = new List<FzSchemeEntity>();
            this._relations = new List<FzRelationEntity>();
            this._queries = new List<FzQueryEntity>();
            this._connString = String.Empty;
            this._dataSet = new DataSet();
            this._discreteFuzzyNumbers = new List<FzDiscreteFuzzySetEntity>();
            this._continuousFuzzyNumbers = new List<FzContinuousFuzzySetEntity>();
        }

        public FdbEntity(String fdbPath)
        {
            this._fdbName = String.Empty;//Get the path for fuzzy db
            this._fdbPath = fdbPath;
            this._fdbName = this.GetFdbName(fdbPath);
            this._connString = @"Data Source = " + fdbPath + "; Version = 3;";
            this._schemes = new List<FzSchemeEntity>();
            this._relations = new List<FzRelationEntity>();
            this._queries = new List<FzQueryEntity>();
            this._dataSet = new DataSet();
            this._discreteFuzzyNumbers = new List<FzDiscreteFuzzySetEntity>();
            this._continuousFuzzyNumbers = new List<FzContinuousFuzzySetEntity>();
        }

        #endregion

        #region 4. Methods (None)

        #endregion

        #region 5. Privates

        public String GetFdbName(String fdbPath)
        {
            String fdbName = String.Empty;

            //Get fdbName from the source path
            for (int i = fdbPath.Length - 1; i >= 0; i-- )
			{
                if (fdbPath[i] == '\\')
                    break;
                else
                    fdbName = fdbPath[i] + fdbName;
			}

            //Cut extention "."
            for (int i = fdbName.Length - 1; i >= 0; i--)
            {
                if (fdbName[i] == '.')
                {
                    fdbName = fdbName.Remove(i);
                    break;
                }
            }

            return fdbName;
        }



        #endregion
    }
}
