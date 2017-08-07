using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FRDB_SQLite
{
    public class FzAttributeEntity   //List attributes of a table, schema.....
    {
        #region 1. Fields

        private bool _primaryKey;
        private String _attributeName;
        private FzDataTypeEntity _dataType;
        private String _description;

        #endregion

        #region 2. Properties

        public bool PrimaryKey
        {
            get { return _primaryKey; }
            set { _primaryKey = value; }
        }

        public String AttributeName
        {
            get { return _attributeName; }
            set { _attributeName = value; }
        }

        public FzDataTypeEntity DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }

        public String Description
        {
            get { return _description; }
            set { _description = value; }
        }

        #endregion

        #region 3. Contructors

        public FzAttributeEntity()
        {
            this._primaryKey = false;
            this._attributeName = String.Empty;
            this._dataType = new FzDataTypeEntity();
            this._description = String.Empty;
        }
        
        public FzAttributeEntity(Boolean primaryKey, String attributeName, FzDataTypeEntity dataType, String description)
        {
            this._primaryKey = primaryKey;
            this._attributeName = attributeName;
            this._dataType = dataType;
            this._description = description;
        }

        //Copy Attribute
        public FzAttributeEntity(FzAttributeEntity newAttribute)
        {
            this._primaryKey = newAttribute.PrimaryKey;
            this._attributeName = newAttribute.AttributeName;
            this._dataType = newAttribute.DataType;
            this._description = newAttribute.Description;
        }

        #endregion

        #region 4. Methods (none)

        #endregion

        #region 5. Privates (none)

        #endregion
    }
}
