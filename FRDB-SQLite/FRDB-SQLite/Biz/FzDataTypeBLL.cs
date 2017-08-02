using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FRDB_SQLite;
using System.IO;

namespace FRDB_SQLite
{
    public class FzDataTypeBLL
    {
        #region 1. Fields (none)

        #endregion

        #region 2. Properties (None)
        #endregion

        #region 3. Contructors (None)
        #endregion

        #region 4. Methods

        public static Boolean CheckDataType(Object value, FzDataTypeEntity dataType)
        {
            return FzDataTypeDAL.CheckDataType(value, dataType);
        }
        //edit
        public static Boolean CheckExistsFuzzy(string FuzzySetName, string path)
        {
            bool result = true;
            FuzzyProcess fp = new FuzzyProcess();
            if (!fp.Exists(path + FuzzySetName + ".conFS") &&
                        !fp.Exists(path + FuzzySetName + ".disFS"))
            {
                return result=false;
            }
            return result;
        }
        //end
        #endregion

        #region 5. Privates (none)
        #endregion
    }
}
