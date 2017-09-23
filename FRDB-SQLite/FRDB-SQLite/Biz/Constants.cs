using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FRDB_SQLite.Biz
{
    static public class Constants
    {
        static public class QUERY_TYPE
        {
            public const String SINGLE = "Constants/QUERY_TYPE/SINGLE";
            public const String MUTIPLE = "Constants/QUERY_TYPE/MUTIPLE";
        }
        static public class COMBINATION_TYPE
        {
            public const String UNION = "Constants/COMBINATION_TYPE/UNION";
            public const String INTERSECT = "Constants/COMBINATION_TYPE/INTERSECT";
        }
    }
}
