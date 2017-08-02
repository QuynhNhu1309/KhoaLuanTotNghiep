using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FRDB_SQLite;

namespace FRDB_SQLite
{
    /// <summary>
    /// The old algorithm
    /// Format of query with no quotes mark
    /// Exp: select * from patient where age=young and weight>=30 (Not suport (age=young) and (weight>=30)...)
    /// </summary>
    public class OldAlgorithm
    {
        #region 1. Fields

        private static String[] _operators = { "<", ">", "<=", ">=", "!=", "<>", "=" };
        private List<FzAttributeEntity> _attributes;
        
        private List<FzRelationEntity> _selectedRelations;
        private String _conditionText;
        private String _errorMessage;

        #endregion

        #region 2. Properties

        public List<FzRelationEntity> SelectedRelations
        {
            get { return _selectedRelations; }
            set { _selectedRelations = value; }
        }

        public String ConditionText
        {
            get { return _conditionText; }
            set { _conditionText = value; }
        }

        public String ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

        #endregion

        #region 3. Contructors

        public OldAlgorithm(String conditonText, List<FzRelationEntity> relations)
        {
            if (conditonText.Contains("<>"))
                conditonText = conditonText.Replace("<>", "!=");
            this._conditionText = conditonText;
            this._errorMessage = "";
            this._selectedRelations = relations;
            this._attributes = relations[0].Scheme.Attributes;
        }

        #endregion

        #region 4. Methods
        /// <summary>
        /// -Get expression (expression maybe content more than one operators): 20<age=young and height>150
        /// -Split expression to list string: first element is index of attribute, after that odd elements is operator and even element is value to compare(maybe fuzzy value)
        /// -Format condition pair of index attribute and value compare (operator is between) => 3> 20 and 3=young and 5>150
        /// -Split condition formated to list of string[] items: 1.index of attribute; 2.operator; 3.value compare(maybe fuzzy); 4.logicality
        /// -Loop to end list string[] splited: and check
        /// </summary>
        public FzTupleEntity IsSatisfy(FzTupleEntity tuple)
        {
            FzTupleEntity previous = null;// = new FzTupleEntity();
            Boolean flag = true;//flag for and logicality
            String and = "";

            String condition = FormatCondition(this._conditionText);
            List<String[]> itemsEx = GetItemExpression(condition);//condition must be formated, each itemsEx consists of 4 members

            for (int i = 0; i < itemsEx.Count; i++)
            {
                if (itemsEx[i][3] == " and ")
                {
                    FzTupleEntity current = IsSatisfyExpression(itemsEx[i], tuple);
                    if (current == null)
                    {
                        flag = false;
                        if (and == " and ")
                            previous = null;
                    }
                    else
                    {
                        if (and == " not ")
                        {
                            previous = null;
                        }
                        else if (previous != null)
                        {
                            previous = UpdateMembership(previous, current);
                        }
                        else
                        {
                            if (and == " and ")
                                previous = null;
                            else
                                previous = current;
                        }
                    }

                    and = " and ";
                }
                else if (itemsEx[i][3] == " or ")
                {
                    FzTupleEntity current = IsSatisfyExpression(itemsEx[i], tuple);
                    if (current != null)
                    {
                        if (and == " not ")
                        {
                            previous = null;
                        }
                        else if (previous != null)
                        {
                            previous = UpdateMembership(previous, current);
                        }
                        else
                            previous = current;
                    }
                    else
                    {
                        if (and == " and ")
                            previous = null;
                    }

                    flag = true;
                    and = " or ";
                }
                else if (itemsEx[i][3] == " not ")
                {
                    FzTupleEntity current = IsSatisfyExpression(itemsEx[i], tuple);
                    if (current != null)
                    {
                        if (and == " not " || and == " and ")//this not belong to this expression 
                        {
                            previous = null;
                        }
                        else if (previous != null)
                        {
                            previous = UpdateMembership(previous, current);
                        }
                        else//previous == null
                            previous = current;
                    }
                    else
                    {
                        if (previous == null)
                            previous = null;
                    }

                    flag = true;    
                    and = " not ";
                    
                }
                else//With no zlogicality, end of the while
                {
                    FzTupleEntity current = IsSatisfyExpression(itemsEx[i], tuple);
                    if (current != null)
                    {
                        if (and == " not ")
                        {
                            previous = null;
                        }
                        else if (and == " and ")
                        {
                            if (previous != null)
                                previous = UpdateMembership(previous, current);
                            else
                                previous = null;
                        }
                        else if (previous != null)
                        {
                            previous = UpdateMembership(previous, current);
                        }
                        else//mean previous =null
                        {
                            if (flag)
                            {
                                previous = current;
                            }
                            else//error heres
                                previous = null;
                        }
                    }
                    else
                    {
                        if (and == " and ")
                            previous = null;
                    }
                }
            }

            return previous;
        }
        #endregion

        #region 5. Privates
        private FzTupleEntity UpdateMembership(FzTupleEntity t1, FzTupleEntity t2)
        {
            Double u1 = Convert.ToDouble(t1.ValuesOnPerRow[t1.ValuesOnPerRow.Count - 1]);
            Double u2 = Convert.ToDouble(t2.ValuesOnPerRow[t2.ValuesOnPerRow.Count - 1]);
            t1.ValuesOnPerRow[t1.ValuesOnPerRow.Count - 1] = Math.Min(u1, u2);//Update the membership

            return t1;
        }

        private FzTupleEntity IsSatisfyExpression(String[] splitedList, FzTupleEntity tuple)
        {
            FzTupleEntity result = new FzTupleEntity() { ValuesOnPerRow = tuple.ValuesOnPerRow};
            
            int indexAttr = Convert.ToInt32(splitedList[0]);
            String dataType = this._attributes[indexAttr].DataType.DataType;
            Object value = tuple.ValuesOnPerRow[indexAttr];//we don't know the data type of value
            int count = 0;

            DiscreteFuzzySetBLL disFS = new DiscreteFuzzySetBLL().GetByName(splitedList[2]);//2 is value input
            ContinuousFuzzySetBLL conFS = new ContinuousFuzzySetBLL().GetByName(splitedList[2]);

            if (conFS != null)//continuous fuzzy set is priorer than discrete fuzzy set
            {
                Double uValue = FuzzyCompare(Convert.ToDouble(value), conFS, splitedList[1]);//1 is operator
                Double uRelation = Convert.ToDouble(tuple.ValuesOnPerRow[tuple.ValuesOnPerRow.Count - 1]);
                if (uValue != 0)
                {
                    result.ValuesOnPerRow[tuple.ValuesOnPerRow.Count - 1] = Math.Min(uValue, uRelation);
                    count++;
                }
            }

            if (disFS != null && conFS == null)
            {
                Double uValue = FuzzyCompare(Convert.ToDouble(value), disFS, splitedList[1]);
                Double uRelation = Convert.ToDouble(tuple.ValuesOnPerRow[tuple.ValuesOnPerRow.Count - 1]);
                if (uValue != 0)
                {
                    result.ValuesOnPerRow[tuple.ValuesOnPerRow.Count - 1] = Math.Min(uValue, uRelation);
                    count++;
                }
            }

            if (disFS == null && conFS == null)
            {
                if (ObjectCompare(value, splitedList[2], splitedList[1], dataType))
                {
                    count++;
                }
            }

            if (count == 1)//it mean the tuple is satisfied with all the compare operative
            {
                return result;
            }

            return null;
        }


        private int IndexOfAttr(String s)
        {
            for (int i = 0; i < this._attributes.Count; i++)
            {
                if (s.Equals(this._attributes[i].AttributeName.ToLower()))
                {
                    return i;
                }
            }

            return -1;
        }

        private String ReverseOperator(String op)
        {
            String result = op;
            if (op == "<")
                result = ">";
            if (op == "<=")
                result = ">=";
            if (op == ">")
                result = "<";
            if (op == ">=")
                result = "<=";

            return result;
        }

        private Boolean StringCompare(String a, String b, String opr)
        {
            switch (opr)
            {
                case "=": return (a.CompareTo(b) == 0);
                case "!=": return (a.CompareTo(b) != 0);
            }

            return false;
        }

        private Boolean BoolCompare(Boolean a, Boolean b, String opr)
        {
            switch (opr)
            {
                case "=": return (a == b);
                case "!=": return (a != b);
            }

            return false;
        }

        private Boolean DoubleCompare(Double a, Double b, String opr)
        {
            switch (opr)
            {
                case "<": return (a < b);
                case ">": return (a > b);
                case "<=": return (a <= b);
                case ">=": return (a >= b);
                case "=": return (Math.Abs(a - b) < 0.001);
                case "!=": return (Math.Abs(a - b) > 0.001);
            }

            return false;
        }

        private Boolean IntCompare(int a, int b, String opr)
        {
            switch (opr)
            {
                case "<": return (a < b);
                case ">": return (a > b);
                case "<=": return (a <= b);
                case ">=": return (a >= b);
                case "=": return (a == b);
                case "!=": return (a != b);
            }

            return false;
        }


        /// <summary>
        /// Get the dataType of attribute index
        /// After that convert value in tuple and value of user input to the same data type of attribute
        /// Comparing two values belongs to operator of data type
        /// </summary>
        private Boolean ObjectCompare(Object value, String input, String opr, String type)
        {
            switch (type)
            {
                case "Int16":
                case "Int64":
                case "Int32":
                case "Byte":
                case "Currency": return IntCompare(Convert.ToInt32(value), Convert.ToInt32(input), opr);
                case "String":
                case "DateTime":
                case "UserDefined":
                case "Binary": return StringCompare(value.ToString(), input, opr);
                case "Decimal":
                case "Single":
                case "Double": return DoubleCompare((double)value, Convert.ToDouble(input), opr);
                case "Boolean": return BoolCompare((bool)value, Convert.ToBoolean(input), opr);
            }

            return false;
        }

        private Double FuzzyCompare(Double value, ContinuousFuzzySetBLL set, String opr)
        {
            Double result = 0;
            
            switch (opr)
            {
                case "<"://
                    if (value < set.Bottom_Left)
                    {
                        result = 1; 
                    }
                    return result;

                case ">":
                    if (value > set.Bottom_Right)
                    {
                        result = 1;
                    }
                    return result;

                case "<=":
                    if (value <= set.Bottom_Right)
                    {
                        result = 1;//select 
                    }
                    return result;

                case ">=":
                    if (value >= set.Bottom_Left)
                    {
                        result = 1;//select 
                    }
                    return result;

                case "=":
                    if (value >= set.Bottom_Left && value <= set.Bottom_Right)
                    {
                        result = set.GetMembershipAt(value);
                    }
                    return result;

                case "!="://No need to get the membership
                    if (value < set.Bottom_Left && value > set.Bottom_Right)
                    {
                        result = 1;//selet the tuple
                    }
                    return result;
                }
            
            return result;
        }

        private Double FuzzyCompare(Double value, DiscreteFuzzySetBLL set, String opr)
        {
            Double result = 0;

            switch (opr)
            {
                case "<"://
                    if (set.GetMinValue() > value)
                    {
                        result = 1;
                    }
                    return result;

                case ">":
                   if (set.GetMaxValue() < value)
                    {
                        result = 1;
                    }
                    return result;

                case "<=":
                    if (value <= set.GetMaxValue())
                    {
                        result = 1;
                    }
                    return result;

                case ">=":
                    if (value >= set.GetMaxValue())
                    {
                        result = 1;//select 
                    }
                    return result;

                case "=":
                    if (set.IsMember(value))
                    {
                        result = set.GetMembershipAt(value);
                    }
                    return result;

                case "!="://No need to get the membership
                    if (!set.IsMember(value))
                    {
                        result = 1;
                    }
                    return result;
            }

            return result;
        }

       
        /// <summary>
        /// Return list of string[]: item in expression + logicality from the condition was formated 
        /// Each of string[] consists of four members: 1.index of attribute; 2.operator; 3.value; 4.logicality
        /// </summary>
        public List<String[]> GetItemExpression(String formated)
        {
            List<String[]> result = new List<string[]>();

            while (formated.Length > 0)
            {
                String[] expression = GetExpression(formated);
                String[] items = new String[4];
                int i = -1; int j = -1;

                if (expression[0].Contains("<=") || expression[0].Contains(">=") || expression[0].Contains("!="))
                {
                    if (expression[0].Contains("<="))
                        i = expression[0].IndexOf("<=");
                    else if (expression[0].Contains(">="))
                        i = expression[0].IndexOf(">=");
                    else if (expression[0].Contains("!="))
                        i = expression[0].IndexOf("!=");

                    j = i + 2;//age<=2
                    
                }
                else if ((expression[0].Contains("=") || expression[0].Contains("<") || expression[0].Contains(">")) &&
                    (!expression[0].Contains("<=") && !expression[0].Contains(">=") && !expression[0].Contains("!=")))
                {
                    if (expression[0].Contains("="))
                        i = expression[0].IndexOf("=");
                    else if (expression[0].Contains(">"))
                        i = expression[0].IndexOf(">");
                    else if (expression[0].Contains("<"))
                        i = expression[0].IndexOf("<");

                    j = i + 1;//age<=2
                }

                items[0] = expression[0].Substring(0, i);//index of attribute
                items[1] = expression[0].Substring(i, j - 1);//operator
                items[2] = expression[0].Substring(j);//value compare (maybe fuzzy)
                items[3] = expression[1];//logicality
                

                result.Add(items);
                formated = formated.Substring(Convert.ToInt32(expression[2]));
            }

            return result;
        }

        /// <summary>
        /// Return the condition contents single expressions, 
        /// The attribute in each expression would be converted into the position of attribute in selected relation
        /// Ex: 20<age<50 and weight>50 => 3>20 and 3<50 and 4>50
        /// </summary>
        private String FormatCondition(String condition)
        {
            String result = String.Empty;//20<age<30 and age>young and weight=50 and height>150=> age>20 and age<30 and age>young...

            while (condition.Length > 0)
            {
                String[] expression = GetExpression(condition);
                List<String> splited = SplitExpression(expression[0]);

                for (int i = 1; i < splited.Count; i += 2)//odd is operators, even is input value(maybe fuzzy value)
                {
                    //Attribute + operator + value compare + and
                    result += splited[0] + splited[i] + splited[i + 1] + " and ";
                }

                result = result.Remove(result.Length - 5);//remove logicality
                //result += expression[0];
                result += expression[1];//logicality
                condition = condition.Substring(Convert.ToInt32(expression[2])).Trim();
            }

            return result.Trim();
        }

        /// <summary>
        /// Split the expressions (maybe more operator) to index of attribute and value
        /// The expression param does not contain logicality
        ///index0: attribute 1 (return index of attribute in this._attributes)
        ///index1: operator 1 
        ///index2: value 1 (maybe fuzzy value)
        ///index3: operator 2
        ///index4: value 2 (maybe fuzzy value)
        ///...
        /// </summary>
        private List<String> SplitExpression(String expression)
        {
            List<String> result = new List<string>();

            expression = expression.Replace(" ", "");

            for (int i = 1; i < expression.Length - 1; i++)
            {
                if (expression[i] == '<' || expression[i] == '>')
                {
                    expression = expression.Insert(i, "|"); i++;

                    if (expression[i + 1] == '=')
                    {
                        expression = expression.Insert(i + 2, "|"); i++;
                    }
                    else
                    {
                        expression = expression.Insert(i + 1, "|"); i++;
                    }
                }
                if (expression[i] == '=')
                {
                    if (expression[i - 1] == '!')
                    {
                        expression = expression.Insert(i - 1, "|"); i++;
                        expression = expression.Insert(i + 1, "|"); i++;
                    }
                    else if (expression[i - 1] != '<' && expression[i - 1] != '>')
                    {
                        expression = expression.Insert(i, "|"); i++;
                        expression = expression.Insert(i + 1, "|"); i++;
                    }
                }
            }

            String[] splited = expression.Split('|');//Example: expression = {12, <, age, <, 30};
            ///Get index of attribulte and add to first of result
            ///after that add operator and value next to it
            int flag = -1;
            for (int i = 0; i < splited.Length; i++)
            {
                int index = IndexOfAttr(splited[i]);
                if (index >= 0)
                {
                    result.Add(index.ToString());
                    flag = i;
                    break;
                }
            }

            if (flag == 0)//age=young, age>30>21,...
            {
                for (int i = 1; i < splited.Length - 1; i += 2)
                {
                    result.Add(splited[i]);//Add operator
                    result.Add(splited[i + 1]);//Add value (maybe fuzzy value)
                }
            }
            else if (flag == splited.Length - 1)//young=age, ...<10<30<age
            {
                for (int i = splited.Length - 2; i > 0; i -= 2)
                {
                    result.Add(ReverseOperator(splited[i]));//Add operator
                    result.Add(splited[i - 1]);//Add value (maybe fuzzy value)
                }
            }
            else if (flag > 0 && flag < splited.Length - 1)//...<2<3<age<10<11<...
            {
                for(int i = flag -1; i > 0; i -= 2)
                {
                    result.Add(ReverseOperator(splited[i]));//Add operator
                    result.Add(splited[i - 1]);//Add value (maybe fuzzy value)
                }
                for (int i = flag + 1; i < splited.Length - 1; i += 2)
                {
                    result.Add(splited[i]);//Add operator
                    result.Add(splited[i + 1]);//Add value (maybe fuzzy value)
                }
            }
            else
                result = null;

            return result;
        }

        /// <summary>
        /// Return expression withne more operators
        /// Each expression consists of three members:1.expression (maybe more operators); 2.logicality; 3.the position end of expression + logicality
        /// Prepare for format condition
        /// </summary>
        private String[] GetExpression(String condition)
        {
            int i = int.MaxValue;//index of and
            int j = int.MaxValue;//index of or
            int k = int.MaxValue;//index of not
            condition = condition.ToLower();
            //index0: position to start the next expression
            //index1: the expression string
            //index2: the one of three logicalities (and or not)
            String[] result = new String[3];

            if (condition.Contains(" and "))
                i = condition.IndexOf(" and ");
            if (condition.Contains(" or "))
                j = condition.IndexOf(" or ");
            if (condition.Contains(" not "))
                k = condition.IndexOf(" not ");

            //Because three of logicality are not in the same index, so we have 4 situations:
            if (i < j && i < k)//logicality is " and "
            {
                result[0] = condition.Substring(0, i);
                result[1] = " and ";
                result[2] = (i + 5).ToString();
                return result;
            }
            else if (j < i && j < k)//mean logicality is " or "
            {
                result[0] = condition.Substring(0, j);
                result[1] = " or ";
                result[2] = (j + 4).ToString();//age = old or weight > 50
                return result;
            }
            else if (k < i && k < j)//mean logicality is " not "
            {
                result[0] = condition.Substring(0, k);
                result[1] = " not ";
                result[2] = (k + 5).ToString();

                return result;
            }
            else// it mean the string is not contain logicality
            {
                result[0] = condition;
                result[1] = "";
                result[2] = (condition.Length).ToString();
                return result;
            }
        }
        #endregion
    }
 }
