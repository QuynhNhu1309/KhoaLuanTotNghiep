using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FRDB_SQLite
{
    public class FzRelationEntity  //This contents an relation of db
    {
        #region 1. Fields

        private String _relationName;
        private FzSchemeEntity _scheme;//Each relation has a respective scheme
        private List<FzTupleEntity> _tuples;//List of tuples
        #endregion

        #region 2. Properties

        public String RelationName
        {
            get { return _relationName; }
            set { _relationName = value; }
        }

        public FzSchemeEntity Scheme
        {
            get { return _scheme; }
            set { _scheme = value; }
        }

        public List<FzTupleEntity> Tuples
        {
            get
            {
                //List<FzTupleEntity> result = new List<FzTupleEntity>();
                //foreach (FzTupleEntity item in this._tuples)
                //{
                //    FzTupleEntity tmp = new FzTupleEntity(item);
                //    result.Add(tmp);
                //}
                //return result;
                return _tuples;
            }
            set
            {
                foreach (FzTupleEntity item in value)
                {
                    FzTupleEntity tmp = new FzTupleEntity(item);
                    _tuples.Add(tmp);
                }
                //_tuples = value;
            }
        }

        #endregion

        #region 3. Contructors

        public FzRelationEntity()
        {
            this._relationName = String.Empty;
            this._scheme = new FzSchemeEntity();
            this._tuples = new List<FzTupleEntity>();
        }

        public FzRelationEntity(String relationName)
        {
            this._relationName = relationName;
            this._scheme = new FzSchemeEntity();
            this._tuples = new List<FzTupleEntity>();
        }

        public FzRelationEntity(FzRelationEntity old)
        {
            this._relationName = old._relationName;
            this._scheme = old._scheme;
            this._tuples = old._tuples;

        }
        #endregion

        #region 4. Methods (none)

        #endregion

        #region 5. Privates (none)
        #endregion
    }
}
