using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FRDB_SQLite;
using FRDB_SQLite;

namespace FRDB_SQLite
{
    public class FzSchemeBLL
    {
        #region 1. Fields

        FzSchemeDAL dal = new FzSchemeDAL();

        #endregion

        #region 2. Properties (none)

        #endregion

        #region 3. Contructors (none)

        #endregion

        #region 4. Methods

        public static Boolean IsInherited(FzSchemeEntity scheme, List<FzRelationEntity> relations)
        {
            if (scheme.Attributes.Count > 0)
            {
                return FzSchemeDAL.IsInherited(scheme, relations);
            }
            return false;
        }

        public static Boolean IsInherited(FdbEntity fdb)
        {
            return FzSchemeDAL.IsInherited(fdb);
        }

        public static FzSchemeEntity GetSchemeByName(String name, FdbEntity fdb)
        {
            return FzSchemeDAL.GetSchemeByName(name, fdb);
        }

        public static List<String> GetListSchemeName(FdbEntity fdb)
        {
            return FzSchemeDAL.GetListSchemeName(fdb);
        }

        public static void RenameScheme(String oldName, String newName, FdbEntity fdb)
        {
            if (fdb.Schemes.Count > 0)
            {
                FzSchemeDAL.RenameScheme(oldName, newName, fdb);
            }
        }

        public static Boolean DeleteAllSchems(FdbEntity fdb)
        {
            return FzSchemeDAL.DeleteAllSchemes(fdb);
        }

        #endregion

        #region 5. Privates (None)

        #endregion
    }
}
