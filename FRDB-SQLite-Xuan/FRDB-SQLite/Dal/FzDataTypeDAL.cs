using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FRDB_SQLite;
using System.Text.RegularExpressions;

namespace FRDB_SQLite
{
    public class FzDataTypeDAL
    {
        #region 1. Fields (none)

        #endregion

        #region 2. Properties (None)
        #endregion

        #region 3. Contructors (None)
        #endregion

        #region 4. Publics

        public static Boolean CheckDataType(Object value, FzDataTypeEntity dataType)
        {
            try
            {
                switch (dataType.DataType)
                {
                    case "Int16": Int16 a = 0; return (Int16.TryParse(value.ToString(), out a));
                    case "Int32": Int32 b = 0; return (Int32.TryParse(value.ToString(), out b));
                    case "Int64": Int64 c = 0; return (Int64.TryParse(value.ToString(), out c));
                    case "Byte":  Byte  d = 0; return (Byte.TryParse(value.ToString(), out d));
                    case "String": return true;
                    case "DateTime": return true;//DateTime e = DateTime.Today; return (DateTime.TryParse(value.ToString(), out e));
                    case "Decimal": Decimal f = 0; return (Decimal.TryParse(value.ToString(), out f));
                    case "Single": Single g = 0; return (Single.TryParse(value.ToString(), out g));
                    case "Double": Double h = 0; return (Double.TryParse(value.ToString(), out h));
                    case "Boolean":
                        if (value.ToString().ToLower() == "true" || value.ToString().ToLower() == "false") return true;
                        else return false;//Boolean k = true; return (Boolean.TryParse(value.ToString(), out k));
                    case "Binary": return (IsBinaryType(value));
                    case "Currency": return (IsCurrencyType(value));
                    case "UserDefined": return (dataType.DomainValues.Contains(value.ToString()));
                }
            }
            catch(Exception ex)
            {
                return false;
            }

            return false;
        }

        #endregion

        #region 5. Privates

        private static bool IsBinaryType(object v)
        {
            try
            {
                foreach (char i in v.ToString())
                {
                    if (i != '0' && i != '1')
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private static bool IsCurrencyType(object v)
        {
            try
            {
                double MINCURRENCY = 1.0842021724855044340074528008699e-19;
                double MAXCURRENCY = 9223372036854775807.0;
                double temp = Convert.ToDouble(v);

                if (temp - MINCURRENCY >= 0)
                {
                    if (temp - MAXCURRENCY <= 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }

        private static bool IsNumber(String pText)
        {
            Regex regex = new Regex(@"^[-+]?[0-9]*\.?[0-9]+$");
            return regex.IsMatch(pText);
        }

        #endregion
    }
}
