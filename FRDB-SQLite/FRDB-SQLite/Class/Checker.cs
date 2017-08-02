using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FRDB_SQLite.Class
{
    public class Checker
    {
        private static char[] specialCharacters = new char[] { '~', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '+',
                                            '`', ';', ',', '<', '>', '?', '/', ':', '\"', '\'', '=', '{', '}', '[', ']', '\\', '|', '.',' ' };

        public static String GetSpecialCharaters()
        {
            String result = "";
            foreach (var item in specialCharacters)
            {
                result += item.ToString() + ", ";
            }

            result = result.Remove(result.Length - 1);

            return result;
        }

        public static Boolean NameChecking(String name)
        {
            foreach (char item in specialCharacters)
	        {
                if (name.Contains(item.ToString()))
                    return false;
	        }

            return true;
        }
    }
}
