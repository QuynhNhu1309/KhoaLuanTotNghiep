using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FRDB_SQLite;

namespace FRDB_SQLite
{
    public class FzRelationDAL
    {
        #region 1. Fields (none)

        #endregion

        #region 2. Properties (None)
        #endregion

        #region 3. Contructors (None)
        #endregion

        #region 4. Methods

        public static List<String> GetListRelationName(FdbEntity fdb)
        {
            List<String> relationNames = new List<String>();

            foreach (FzRelationEntity relation in fdb.Relations)
            {
                relationNames.Add(relation.RelationName);
            }

            return relationNames;
        }

        public static FzRelationEntity GetRelationByName(String relationName, FdbEntity fdb)
        {
            foreach (FzRelationEntity r in fdb.Relations)
            {
                if (r.RelationName == relationName)
                {
                    return r;
                }
            }

            return null;
        }

        public static List<int> GetArrPrimaryKey(FzRelationEntity rel)
        {
            List<int> result = new List<int>();
            int i = 0;
            foreach (FzAttributeEntity item in rel.Scheme.Attributes)
            {
                if (item.PrimaryKey == true)
                {
                    result.Add(i);
                }
                i++;
            }

            return result;
        }

        #endregion

        #region 5. Privates (none)
        #endregion
    }
}
