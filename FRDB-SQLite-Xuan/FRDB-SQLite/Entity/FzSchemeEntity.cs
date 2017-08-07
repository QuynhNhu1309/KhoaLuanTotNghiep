using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FRDB_SQLite
{
    public class FzSchemeEntity  //This content a schema of database
    {
        #region 1. Fields

        private String _schemeName;
        private List<FzAttributeEntity> _attributes; //List of attributes of scheme

        #endregion

        #region 2. Properties

        public String SchemeName
        {
            get { return _schemeName; }
            set { _schemeName = value; }
        }

        public List<FzAttributeEntity> Attributes
        {
            get { return _attributes; }
            set { _attributes = value; }
        }

        #endregion

        #region 3. Contructors

        public FzSchemeEntity()
        {
            this._schemeName = String.Empty;
            this._attributes = new List<FzAttributeEntity>();
        }

        public FzSchemeEntity(String schemeName, List<FzAttributeEntity> attributes)
        {
            this._schemeName = schemeName;
            this._attributes = attributes;
        }

        public FzSchemeEntity(String schemeName)
        {
            this._schemeName = schemeName;
            this._attributes = new List<FzAttributeEntity>();
        }

        #endregion

        #region 4. Methods


       
        #endregion

        #region 5. Privates



        #endregion
    }
}
