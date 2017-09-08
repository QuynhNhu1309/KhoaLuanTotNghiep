using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using FRDB_SQLite;
using System.Text.RegularExpressions;

namespace FRDB_SQLite.Class
{
    public class QueryPL
    {
        #region 1. Fields
        private String[] _compare = { ">", "=", "<", "!"};
        #endregion
        #region 2. Properties

        #endregion
        #region 3. Contructors

        #endregion
        #region 4. Methods
        public static void txtQuery_TextChanged(SyntaxRichTextBox txtQuery)
        {
            //textBox1.SelectionStart = 0;
            //textBox1.SelectionLength = textBox1.Text.Length;
            // Add the keywords to the list.
            txtQuery.Settings.Keywords.Add("select");
            txtQuery.Settings.Keywords.Add("from");
            txtQuery.Settings.Keywords.Add("where");
            // The operators and logicality
            //txtQuery.Settings.Keywords2.Add("*");
            txtQuery.Settings.Keywords2.Add("and");
            txtQuery.Settings.Keywords2.Add("or");
            txtQuery.Settings.Keywords2.Add("not");

            // Set the comment identifier. For Lua this is two minus-signs after each other (--). 
            // For C++ we would set this property to "//".
            txtQuery.Settings.Comment = "--";
            txtQuery.Settings.Between = "\"";

            // Set the colors that will be used.
            txtQuery.Settings.KeywordColor = Color.Blue;
            txtQuery.Settings.KeywordColor2 = Color.Gray;
            txtQuery.Settings.CommentColor = Color.Green;
            txtQuery.Settings.StringColor = Color.Black;
            txtQuery.Settings.IntegerColor = Color.DarkOrchid;//DarkLayGray

            // Let's not process strings and integers.
            txtQuery.Settings.EnableStrings = false;
            //txtQuery.Settings.EnableIntegers = false;

            // Let's make the settings we just set valid by compiling
            // the keywords to a regular expression.
            txtQuery.CompileKeywords();

            // LUpdate the syntax highlighting.
            txtQuery.ProcessAllLines();
        }

        public static String Standard(String query)
        {
            String s = query;
            String result = "";
            int i = 0;
            while (s.Length > 0)
            {
                
            }
            return result;
        }

        public static String StandardizeQuery(String query)
        {
            try
            {//The query text has been already cut spaces at the end and at the last string.
                String result = String.Empty;
                query = query.Replace("  ", " ");
                result = query;
                //min, max, sum, count, avg
                result = result.Replace("( min", "(min");
                result = result.Replace("min (", "min(");

                result = result.Replace("( max", "(max");
                result = result.Replace("max (", "max(");

                result = result.Replace("( sum", "(sum");
                result = result.Replace("sum (", "sum(");

                result = result.Replace("( avg", "(avg");
                result = result.Replace("avg (", "avg(");

                result = result.Replace("count (", "count(");
                result = result.Replace("( count", "(count");

                result = result.Replace("\n", "");
                result = result.Replace("<>", "!=");
                result = result.Replace("->", "→");

                return result.ToLower();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

            return "";
        }

        public static String ReplaceLetter(String query)
        {
            String result = String.Empty;
            result = query.Replace("'", "");
            result = query.Replace("\"", "");
            return result;
        }

        public static String CheckSyntax(String query)// Query must be standarded and remain single space between specify word
        {
            String message = "";
            if (!query.Contains("select"))// Need contains select
                return message = "Query is missing 'select' structure!";

            //if (!query.Contains("*"))// Need contains *
            //    return message = "Query is missing '*' structure!";

            //if (query.Substring(query.IndexOf("select") + 7, 1) != "*")// Next to after select must be * (select on one relation)
            //    return message = "Do not support select with attributes!";

            if (!query.Contains("from"))// Need to contains from
                return message = "Query is missing 'from' structure!";

            //var for select
            int select = query.IndexOf("select");
            int selectAttr = query.IndexOf("*");

            int from = query.IndexOf("from");
            
            //var for where
            int where = 0;
            string whereAttr = "";

            //var for group by
            int groupby = 0, startGroupbyAttr = 0, endGroupbyAttr = 0;
            string groupbyAttr = "", selectAttrStr = "";
            groupby = query.IndexOf(" group by ");

            //var for having
            int having = 0, startHavingAttr = 0, endHavingAttr = 0;
            string havingAttr = "";

            //var for order by
            int orderby = 0;
            string orderbyAttr = "";
            orderby = query.IndexOf(" order by ");

            string[] selectAttrArr = null;
            if (select != query.LastIndexOf("select"))// Select must be unique
                return message = "Not support multi 'select'!";

            //if (selectAttr != query.LastIndexOf("*"))// * must be unique
            //    return message = "Not support multi '*'!";
            // selected attributes
            if (selectAttr < 0)
            {
                selectAttrStr = query.Substring(select + 7, from - select - 8);
                selectAttrArr = selectAttrStr.Split(',');
                //MatchCollection attr = Regex.Matches(selectAttrStr, @"[\w]+");// count word in select clause
                //// ^ (\"|\\s\"|'|'\\s)([a-z0-9A-Z\\s/.])+(\"|\"\\s|'|'\\s)$|\\d
                //MatchCollection attrComma = Regex.Matches(selectAttrStr, @"[,]+");// count comma in select clause
                //int tmp4 = attr.Count;
                //if (attr.Count > 1 && attrComma.Count == attr.Count - 1)
                //    selectAttrArr = selectAttrStr.Split(',');
                //else if (attr.Count == 1)
                //{
                //    selectAttrStr += ",";
                //    selectAttrArr = selectAttrStr.Split(',');
                //}
                //else if (attrComma.Count != attr.Count - 1 && attr.Count > 1)
                //    return message = "Missing comma near 'select' clause";
            }

            if (from != query.LastIndexOf("from"))// From must be unique
                return message = "Not support multi 'from'!";

            if (!query.Contains("where"))
            {
                if (query.Substring(from + 4).Trim() == "")// Missing relation after 'from'
                    return message = "Incorrect syntax near 'from': missing relation.";
            }
            else// Check syntax of condition: where (age="young" not weight>=45 or height>150) and height>155
            {
                where = query.IndexOf("where");
                if (groupby > 0)
                    whereAttr = query.Substring(where + 5, groupby - where - 5);// condition of where clause
                else if (orderby > 0)
                    whereAttr = query.Substring(where + 5, orderby - where - 5);// condition of where clause
                else if (groupby < 0 && orderby < 0)
                    whereAttr = query.Substring(where + 5);

                if (where != query.LastIndexOf("where"))// Where must be unique
                    return message = "Not support multi condition with 'where'!";

                if (query.Substring(where + 5).Trim() == "")// Missing condition after 'where'
                    return message = "Incorrect syntax near 'where': missing condition.";

                if (query.Contains("and"))
                {
                    if (where + 6 == query.IndexOf("and"))
                        return message = "Incorrect syntax near 'where': 'and' does not at the begin of condition.";
                    int i = query.IndexOf("and");
                    if (query[i - 1] == '\"' || query[i + 3] == '\"')
                        return message = "Incorrect syntax near 'and': missing space with '\"'";
                }
                if (query.Contains("or"))
                {
                    if (where + 6 == query.IndexOf("or"))
                        return message = "Incorrect syntax near 'where': 'or' does not at the begin of condition.";
                    int i = query.IndexOf("or");
                    if (query[i - 1] == '\"' || query[i + 2] == '\"')
                        return message = "Incorrect syntax near 'or': missing space with '\"'";
                }
                if(whereAttr.Contains("min(") || whereAttr.Contains("max(") || whereAttr.Contains("count(") || whereAttr.Contains("avg(") || whereAttr.Contains("sum("))
                {
                    return message = "'where'condition must not contain aggregate function";
                }
                //edit--------
                //if (query.Contains(" not "))
                //{
                //    if (where + 6 != query.IndexOf("not"))
                //        return message = "Incorrect syntax near 'where': 'not' does not at the begin of condition.";
                //    int i = query.IndexOf("not");
                //    if (query[i - 1] == '\"' || query[i + 3] == '\"')
                //        return message = "Incorrect syntax near 'not': missing space with '\"'";
                //}

                //---------------
                String m = CheckNested(query);
                if (m != "")
                    return m;

                m = CheckLogic(query); // checking logical(and, or) between 2 or more than conditions
                if (m != "")
                    return m;
                m = CheckQuote(query);
                if (m != "")
                    return m;

                m = CheckFuzzySet(query);
                if (m != "")
                    return m;
                //m = CaseCheckFuzzySet(query, "->", 2);
                //if (m != "")
            }
            //group by
            if (query.Contains(" group by "))
            {
                groupby = query.IndexOf(" group by ");
                if (groupby < selectAttr || groupby < select || groupby < from || groupby < where)
                {
                    return message = "'Group by' clause is reuquired after 'select, from, where'";
                }
            }

            //having
            if (query.Contains(" having "))
            {
                having = query.IndexOf(" having ");
                if (having < groupby || groupby < 0)
                {
                    return message = "'Group by' clause is reuquired before 'having'";
                }
            }

            //order by
            if (query.Contains(" order by "))
            {
                orderby = query.IndexOf(" order by ");
                if (orderby < having || orderby < selectAttr || orderby < select || orderby < from || orderby < where)
                {
                    return message = "'Order by' clause is reuquired after 'select, from, where, group by, having'";
                }
            }

            //Check select in 'group by clause'
            if (groupby > 0)
            {
                startGroupbyAttr = groupby + 9;
                if (having > 0)
                    endGroupbyAttr = having;
                else if (having <= 0 && orderby > 0)
                    endGroupbyAttr = orderby;
                else if (having <= 0 && orderby <= 0)
                    endGroupbyAttr = query.Length;
                groupbyAttr = query.Substring(startGroupbyAttr + 1, endGroupbyAttr - startGroupbyAttr - 1).ToLower();
                MatchCollection attr = Regex.Matches(groupbyAttr, @"[\w]+");// count word in group by clause
                MatchCollection attrComma = Regex.Matches(groupbyAttr, @"[,]+");// count comma in group by clause
                if ((attr.Count > 1 || attr.Count == 1) && attrComma.Count == attr.Count - 1 && selectAttr < 0)
                {
                    for (int i = 0; i < selectAttrArr.Length; i++)
                    {
                        if (!groupbyAttr.Contains(selectAttrArr[i].Trim().ToLower()) && !selectAttrArr[i].Trim().ToLower().Contains("min") && !selectAttrArr[i].Trim().ToLower().Contains("max") && !selectAttrArr[i].Trim().ToLower().Contains("count") && !selectAttrArr[i].Trim().ToLower().Contains("avg") && !selectAttrArr[i].Trim().ToLower().Contains("sum"))
                            return message = "Attributes in 'select' must be included in 'group by'";
                    }
                }
                else
                    if (attrComma.Count != attr.Count - 1 && attr.Count > 1)
                    return message = "Missing comma in 'group by' clause";
                //if (selectAttr < 0) // not contain * in select
            }

            //Check 'having' clause
            //if (having > 0)
            //{
            //    startHavingAttr = having + 7;//resuse 'group by' var for 'having'
            //    if (orderby > 0)
            //        endHavingAttr = orderby;
            //    else if (orderby <= 0)
            //        endHavingAttr = query.Length;
            //    havingAttr = query.Substring(startHavingAttr + 1, endHavingAttr - startHavingAttr - 1).ToLower();
            //    //^\w+[\s](like|>|not like|<)[\s][[a-z]+|[a-z]+[\s]*]$
            //    MatchCollection attr = Regex.Matches(havingAttr, @"[\w]+");// count word in group by clause
            //    MatchCollection attrComma = Regex.Matches(havingAttr, @"[,]+");// count comma in group by clause
            //    int tmp1 = attr.Count;
            //    int tmp2 = attrComma.Count;
            //    //string minHaving
            //    if ((attr.Count > 1 || attr.Count == 1) && attrComma.Count == attr.Count - 1)
            //    {
            //        string[] havingAttrArr = null;
            //        havingAttrArr = groupbyAttr.Split(',');
            //        if (selectAttr < 0)
            //        {
            //            for (int i = 0; i < havingAttrArr.Length; i++)
            //            {
            //                if (!selectAttrStr.Contains(havingAttrArr[i].Trim().ToLower()) && !havingAttrArr[i].Trim().ToLower().Contains("min") && !havingAttrArr[i].Trim().ToLower().Contains("max") && !havingAttrArr[i].Trim().ToLower().Contains("avg") && !havingAttrArr[i].Trim().ToLower().Contains("count") && !havingAttrArr[i].Trim().ToLower().Contains("sum"))
            //                    return message = "Attributes in 'having' must be included in 'group by'";
            //            }
            //        }
            //    }
            //    else if (attrComma.Count != attr.Count - 1 && attr.Count > 1)
            //        return message = "Missing comma in 'having' clause";
            //}
            

            return message;
        }
        #endregion
        #region 5. Privates
        private static  String CheckLogic(String query)
        {
            String message = "";
            String select = query.Substring(0, query.IndexOf("where"));

            if (select.Contains(" and ") || select.Contains(" or ") || select.Contains(" not ") || select.Contains("\""))
                return message = "Select clause do not allow 'and', 'or', 'not', and '\"'.";
            int i = 0, j = 0;
            while (i < query.Length)
            {
                if (query[i] == ')' && i < query.Length - 1)
                {
                    j = i + 1;
                    if (query[j] != ')')
                    {
                        while (query[j] != '(' && j < query.Length - 1) j++;
                        String s = query.Substring(i + 1, j - i - 1);
                        int count = 0;
                        if (s.Length == 0) count++;
                        if (s.Length >= 5) { if (s.Substring(0, 5) == " and ") count++; }
                        if (s.Length >= 4) { if (s.Substring(0, 4) == " or ") count++; }
                        if (s.Length >= 9) { if (s.Substring(0, 9) == " and not ") count++; }
                        if (s.Length >= 8) { if (s.Substring(0, 8) == " or not ") count++; }
                        int tmp = 0;
                        tmp = query.LastIndexOf("(", i + 1);
                        //Regex regex = new Regex(@"^(sum|avg|min|max|count)[(]\w+[)]$");
                        Regex regex = new Regex(@"^(sum|avg|min|max|count)$");
                        if (regex.IsMatch(query.Substring(tmp - 3, 3)) || regex.IsMatch(query.Substring(tmp - 5, 5))) count++;

                        if (count == 0)
                        {
                            i = j + 1;
                            return message = "Missing logicality between two expression.";
                        }
                    }
                }
                i++;
            }

            //Check space surround logicality

            return message;
        }
        private static String CheckNested(String query)
        {
            String message = "";
            string tmp = query;
            int i = 0, countOpen = 0, countClose = 0,pos = 0, posOpen = 0, posClose = 0, k = 0;
            pos = query.IndexOf("(", 0, query.Length - 1);//pos for (
            if (pos > 0 && ((pos < query.IndexOf(")", 0, query.Length - 1) && query.IndexOf(")", 0, query.Length - 1) > 0)
                || (query.IndexOf(")", 0, query.Length - 1) < 0)))
                countOpen++;// count (
            else if(pos > 0 && pos > query.IndexOf(")", 0, query.Length - 1) && query.IndexOf(")", 0, query.Length - 1) > 0)
            {
                countClose++; //count )
                pos = query.IndexOf(")", 0, query.Length - 1);
            }   
            for (k = pos + 1; k < query.Length; k = pos)
            {
                string open = query.Substring(k, query.Length - k);
                string close = query.Substring(k, query.Length - k);
                posOpen = query.IndexOf("(", k, query.Length - k);
                posClose = query.IndexOf(")", k, query.Length - k);
                if (posClose > 0 && (posClose < posOpen || posOpen < 0))
                {
                    countClose++;
                    pos = posClose + 1;

                }
                else if (posOpen > 0 && (posClose > posOpen || posClose < 0))
                {
                    countOpen++;
                    pos = posOpen + 1;
                }
                else if (posOpen > 0 && posClose > 0 && 
                    !Regex.IsMatch(query.Substring(posOpen + 1, posClose - posOpen - 1), @"[a-zA-z1-9]")) // case ()
                    return message = "Incorrect syntax betwwen ()";
                if (pos == query.Length || (posOpen < 0 && posClose < 0)) break;
            }
            if (countOpen > countClose)
                return message = "Missing close parenthesis ')'. 0.1";
            else if (countOpen < countClose)
                return message = "Missing open parenthesis '('. 0.2";
            else if (countOpen == countClose)
            {
                while (i < query.Length - 1)
                {
                    posOpen = query.IndexOf("(", i);
                    posClose = query.IndexOf(")", i);
                    if (posOpen < posClose)
                    {
                        if (posOpen > 0)
                        {
                            if (query[posOpen - 1] != ' ' && query[posOpen - 1] != '(' && !query.Substring(posOpen - 5).Contains("min") && !query.Substring(posOpen - 5).Contains("max") && !query.Substring(posOpen - 5).Contains("not") && !query.Substring(posOpen - 5).Contains("avg") && !query.Substring(posOpen - 5).Contains("sum") && !query.Substring(posOpen - 5).Contains("count") && !query.Substring(posOpen - 5).Contains("in"))// must have space or '(' before (
                                return message = "Incorrect syntax near '(': missing space 1";
                        }
                        if (posClose < query.Length - 1)
                            if (query[posClose + 1] != ' ' && query[posClose + 1] != ')')// must have space or ')' after )
                                return message = "Incorrect syntax near ')': missing space 2";
                        query = query.Remove(posOpen, 1); //remove (
                        query = query.Remove(posClose - 1, 1);//remove )
                    }
                    else
                    if (posOpen > posClose)
                        return message = "Incorrect syntax near '"+query[posOpen]+"'";
                    else if (posOpen < 0 && posClose < 0)
                        break;
                    i = posOpen;
                }
                // case: Age in 19, 20 and Height > 100 => missing 
                //tmp = query, because query string is removed () after above while()
                posOpen = tmp.IndexOf(" in ", 0);
                while (posOpen >= 0 && posOpen < tmp.Length - 5)
                {
                    posClose = tmp.IndexOf(")", posOpen);
                    if (tmp[posOpen + 4] != '(')
                        return message = "Missing open parenthesis near 'in'";
                    else if (posClose < 0)
                        return message = "Missing close parenthesis near 'in'";
                    else if(posClose > posOpen && posOpen >= 0)
                    {
                        string s = tmp.Substring(posOpen + 5, posClose - posOpen - 5);// substring only between ()
                        string[] arrs = s.Split(',');
                        foreach (string arr in arrs)
                        {
                            //if in ("abc") must contain word, didit; in (20, 21) must contain only digit
                            if (!Regex.IsMatch(arr, "^(\"|\\s\"|'|'\\s)([a-z0-9A-Z\\s/.])+(\"|\"\\s|'|'\\s)$|\\d"))
                                return message = "Incorrect syntax between () near 'in'";
                        }
                    }
                    posOpen = tmp.IndexOf(" in ", posClose  + 1);
                }
            }
            
            
            return message;
        }

        private static String CheckQuote(String query)
        {
            string message = "";
            int index = query.IndexOf("where");
            int countQuoteSingle = 0,countQuoteDouble = 0, indexSecond = 0;
            //Remove space next to operator
            String str = query.Substring(index);
            for (int i = 0; i < str.Length; i=indexSecond)
            {
                if (str[i] == '\'')
                {
                    countQuoteSingle++;
                    if(i < str.Length - 1)
                    {
                        indexSecond = str.IndexOf("'", i + 1);
                        if (indexSecond > 0) //syntax betwwen two quote
                        {
                            countQuoteSingle++;
                            if (!CheckBetweenQuote(str.Substring(i + 1, indexSecond - i - 1)))
                                return message = "Incorrect syntax betwwen two quote";
                            if (indexSecond < str.Length - 1)
                            {
                                if (str[indexSecond + 1] != ' ' && str[indexSecond + 1] != ')')
                                {
                                    return message = "Missing space next to quote";
                                }
                            }

                        }
                    }
                    
                }
                else if (str[i] == '\"')
                {
                    countQuoteDouble++;
                    if (i < str.Length - 1)
                    {
                        indexSecond = str.IndexOf("\"", i + 1);
                        if (indexSecond > 0) //syntax betwwen two quote
                        {
                            countQuoteDouble++;
                            if (!CheckBetweenQuote(str.Substring(i + 1, indexSecond - i - 1)))
                                return message = "Incorrect syntax betwwen two quote";
                            if (indexSecond < str.Length - 1)
                            {
                                if (str[indexSecond + 1] != ' ' && str[indexSecond + 1] != ')' )
                                {
                                    return message = "Missing space next to quote";
                                }
                            }

                        }
                    }
                        
                        
                }
                if(indexSecond < 0)
                {
                    indexSecond = i;
                }
                
                indexSecond++;
            }
            if((str.Contains("\'") && countQuoteSingle%2 != 0) || (str.Contains("\"") && countQuoteDouble % 2 != 0))
            {
                return message = "Missing quote ";
            }
            

            return message;
        }

        private static Boolean CheckBetweenQuote(String c)
        {
            if (c.Contains(" and ") || c.Contains(" or ") || c.Contains(" like ") || c.Contains(" not "))
                return false;
            return Regex.IsMatch(c, @"[a-zA-z1-9]");
        }

       
        private static String CheckObject(String query, FdbEntity fdb)
        {
            String message = "";
            int i = query.IndexOf("from" + 5);
            int j = query.IndexOf("where");
            String relation = query.Substring(i, j);
            int c = 0;
            foreach (var item in fdb.Relations)
            {
                if (item.RelationName == relation)
                {
                    c++;
                    break;
                }
            }
            if (c == 0)
                return message = "Invalid object relation name '" + relation + "'";

            return message;
        }

        private static String CheckFuzzySet(String query)
        {
            String message = "";
            int index = -1;
            while (query.Contains("→") && index < query.Length - 1)
            {
                index = query.IndexOf("→", index + 1);
                if (index > 0 && index < query.Length - 2)
                {
                    if ((query[index + 2].ToString() == "\"" || query[index + 2].ToString() == "'") || 
                        (query[index + 1].ToString() == "\"" || query[index + 1].ToString() == "'"))
                        return message = "The name of fuzzy set must be outside of quotes ";
                }
                else if (index < 0) break;
                index++;
            }
            return message;
        }

       
        
        #endregion
    }
}
