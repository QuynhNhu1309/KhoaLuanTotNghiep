using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FRDB_SQLite
{
    public class FzDataTypeEntity    //This is the data type of attributes
    {
        #region 1. Fields

        private String _typeName;// TypeName != DataType if DataType == "User-Defined"
        private String _dataType;
        private String _domainString;
        private List<String> _domainValues;

        #endregion

        #region 2. Properties

        public String TypeName 
        {
            get { return _typeName; }
            set { _typeName = value; }
        }

        public String DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }

        public String DomainString
        {
            get { return _domainString; }
            set { _domainString = value; }
        }

        public List<String> DomainValues
        {
            get { return _domainValues; }
            set { _domainValues = value; }
        }

        #endregion

        #region 3. Contructors

        public FzDataTypeEntity()
        {
            this._typeName = String.Empty;
            this._dataType = String.Empty;
            this._domainString = String.Empty;
            this._domainValues = new List<String>();
        }

        public FzDataTypeEntity(String typeName, String domain)
        {
            this._typeName = typeName;
            this._domainString = domain;
            GetDomain(domain);
            this._dataType = "UserDefined";
            GetDataType();
        }

        #endregion

        #region 4. Methods (None)

        #endregion

        #region 5. Privates

        private void GetDomain(String domain)
        {
            try
            {
               //if (this._typeName == "UserDefined")
                if(IsUserDefine(this._typeName))
                {
                    domain = domain.Replace("{", "");
                    domain = domain.Replace("}", "");
                    char[] seperator = { ',' };
                    String[] temp = domain.Split(seperator);
                    _domainValues = new List<String>();

                    foreach (String value in temp)
                    {
                        _domainValues.Add(value.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ERROR:\n" + ex.Message);
            }
        }

        private Boolean IsUserDefine(string typeName)
        {
            string[] types = new string[] {"Int16", "Int64", "Int32", "Byte", "Decimal", "Currency", "String", 
                                            "Binary" ,"DateTime", "Single" , "Boolean" , "Double"};
            foreach (var item in types)
            {
                if (typeName == item)
                    return false;
            }

            return true ;
        }

        private void GetDataType()
        {
            try
            {
                switch (this._typeName)
                {
                    case "Int16": this._dataType = "Int16"; break;
                    case "Int64": this._dataType = "Int64"; break;
                    case "Int32": this._dataType = "Int32"; break;
                    case "Byte": this._dataType = "Byte"; break;
                    case "Decimal": this._dataType = "Decimal"; break;
                    case "Currency": this._dataType = "Currency"; break;
                    case "String": this._dataType = "String"; break;
                    case "DateTime": this._dataType = "DateTime"; break;
                    case "Binary": this._dataType = "Binary"; break;
                    case "Single": this._dataType = "Single"; break;
                    case "Double": this._dataType = "Double"; break;
                    case "Boolean": this._dataType = "Boolean"; break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ERROR:\n" + ex.Message);
            }
        }

        #endregion
    }
}
