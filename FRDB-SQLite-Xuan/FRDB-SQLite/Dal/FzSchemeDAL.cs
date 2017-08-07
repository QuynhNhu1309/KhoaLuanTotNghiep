using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FRDB_SQLite;

namespace FRDB_SQLite
{
    public class FzSchemeDAL
    {
        #region 1. Fields

        #endregion

        #region 2. Properties (None)
        #endregion

        #region 3. Contructors (None)
        #endregion

        #region 4. Methods

        public static Boolean IsInherited(FzSchemeEntity scheme, List<FzRelationEntity> relations)
        {
            try
            {
                foreach (FzRelationEntity relation in relations)
                {
                    if (scheme.Equals(relation.Scheme))
                    {
                        return true;
                        //break;
                    }
                }
            }
            catch { }

            return false;
        }

        public static Boolean IsInherited(FdbEntity fdb)
        { 
            try
            {
                foreach (var item in fdb.Schemes)
                    if (IsInherited(item, fdb.Relations))
                        return false;
            }
            catch { }

            return true;
        }

        public static FzSchemeEntity GetSchemeByName(String schemeName, FdbEntity fdb)
        {
            foreach (FzSchemeEntity s in fdb.Schemes)
            {
                if (s.SchemeName == schemeName)
                {
                    return s;
                }
            }

            return null;
        }

        public static List<String> GetListSchemeName(FdbEntity fdb)
        {
            List<String> schemeNames = new List<String>();

            foreach (FzSchemeEntity scheme in fdb.Schemes)
            {
                schemeNames.Add(scheme.SchemeName);
            }

            return schemeNames;
        }

        public static void RenameScheme(String oldName, String newName, FdbEntity fdb)
        {
            foreach (var item in fdb.Schemes)
            {
                if (item.SchemeName == oldName)
                {
                    item.SchemeName = newName;
                    break;
                }
            }
        }

        public static Boolean DeleteAllSchemes(FdbEntity fdb)
        {
            foreach (var item in fdb.Schemes)
            {
                fdb.Schemes.Remove(item);
            }
            return true;
        }

        #endregion

        #region 5. Privates
        #endregion
    }
}
