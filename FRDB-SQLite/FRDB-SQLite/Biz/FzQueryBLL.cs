using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FRDB_SQLite;

namespace FRDB_SQLite
{
    public class FzQueryBLL
    {
        #region 1. Fields
        FzQueryDAL queryDAL = new FzQueryDAL();
        #endregion

        #region 2. Properties (None)
        #endregion

        #region 3. Contructors (None)
        #endregion

        #region 4. Methods 
        public static List<String> ListOfQueryName(FdbEntity fdb)
        {
            return FzQueryDAL.ListOfQueryName(fdb);
        }

        public static FzQueryEntity GetQueryByName(String queryName, FdbEntity fdb)
        {
            return FzQueryDAL.GetQueryByName(queryName, fdb);
        }
        #endregion

        #region 5. Privates (none)
        #endregion
    }
}
