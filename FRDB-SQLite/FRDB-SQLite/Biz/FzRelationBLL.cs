using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FRDB_SQLite;
using FRDB_SQLite;

namespace FRDB_SQLite
{
    public class FzRelationBLL
    {
        #region 1. Fields

        FzRelationDAL dal = new FzRelationDAL();

        #endregion

        #region 2. Properties (none)

        #endregion

        #region 3. Contructors (none)

        #endregion

        #region 4. Methods

        public static List<String> GetListRelationName(FdbEntity fdb)
        {
            return FzRelationDAL.GetListRelationName(fdb);
        }

        public static FzRelationEntity GetRelationByName(String relationName, FdbEntity fdb)
        {
            return FzRelationDAL.GetRelationByName(relationName, fdb);
        }

        public static List<int> GetArrPrimaryKey(FzRelationEntity rel)
        {
            return FzRelationDAL.GetArrPrimaryKey(rel);
        }
        #endregion

        #region 5. Privates (None)

        #endregion
    }
}
