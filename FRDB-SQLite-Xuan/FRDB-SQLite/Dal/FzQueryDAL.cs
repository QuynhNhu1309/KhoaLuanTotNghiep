using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FRDB_SQLite;

namespace FRDB_SQLite
{
    public class FzQueryDAL
    {
        #region 1. Fields

        #endregion

        #region 2. Properties (None)
        #endregion

        #region 3. Contructors (None)
        #endregion

        #region 4. Methods

        public static List<String> ListOfQueryName(FdbEntity fdb)
        {
            List<String> queryNames = new List<String>();
            foreach (FzQueryEntity query in fdb.Queries)
            {
                queryNames.Add(query.QueryName);
            }

            return queryNames;
        }

        public static FzQueryEntity GetQueryByName(String queryName, FdbEntity fdb)
        {
            foreach (var item in fdb.Queries)
            {
                if (queryName.Equals(item.QueryName))
                {
                    return item;
                }
            }
            return null;
        }

        #endregion

        #region 5. Privates
        #endregion
    }
}
