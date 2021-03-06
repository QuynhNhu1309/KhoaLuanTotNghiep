﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FRDB_SQLite.Biz;
using System.IO;
using System.Text.RegularExpressions;

namespace FRDB_SQLite
{
    public class QueryConditionBLL
    {
        #region 1. Fields
        
        private List<FzAttributeEntity> _attributes = new List<FzAttributeEntity>();
        //edit---
        private string _uRelation;
        private Logger logger = new Logger(true);
    //--
        //private Double _uRelation;
        //private List<String> _memberships;

        private FzTupleEntity _resultTuple;
        private List<FzRelationEntity> _selectedRelations;
        private List<Item> _itemConditions;
        private String _errorMessage;
        private FdbEntity _fdbEntity;
        private List<int> random_array = new List<int>();
        #endregion

        #region 2. Properties
        public FdbEntity FdbEntity
        {
            get { return _fdbEntity; }
            set { _fdbEntity = value; }
        }
        public FzTupleEntity ResultTuple
        {
            get { return _resultTuple; }
            set {
                FzTupleEntity newTuple = new FzTupleEntity(value);
                this._resultTuple = newTuple;
            }
        }

        public List<FzRelationEntity> SelectedRelations
        {
            get { return _selectedRelations; }
            set { _selectedRelations = value; }
        }

        public List<Item> ItemConditions
        {
            get { return _itemConditions; }
            set {
                foreach (var items in value)
                {
                    Item item = new Item()
                    { elements = items.elements, nextLogic = items.nextLogic,
                        valueAggregate = items.valueAggregate, 
                        notElement = items.notElement, resultCondition = items.resultCondition, ItemName = items.ItemName
                      
                    };
                    this._itemConditions.Add(item);
                } 
            }
        }

        public String ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }


        #endregion

        #region 3. Contructors
        public QueryConditionBLL() {
        }

        public QueryConditionBLL(FdbEntity fdbEntity)
        {
            this._fdbEntity = fdbEntity;

        }

        public QueryConditionBLL(List<Item> items, List<FzRelationEntity> sets, FdbEntity fdbEntity)
        {
            //this._resultTuple = new FzTupleEntity();
            this._selectedRelations = sets;
            this._itemConditions = items;
            this._errorMessage = "";
            this._fdbEntity = fdbEntity;

            //this._memberships = new List<string>();
            //this._uRelation = Double.MaxValue;
            //edit--
            this._uRelation = "";
            //---
            this._attributes = sets[0].Scheme.Attributes;
            //this._fdbEntity = new FdbEntity();
        }
        #endregion

        #region 4. Publics
        /// <summary>
        /// 
        /// </summary>
        //edit---
        public DisFS GetDisFS(string path, string name)
        {
            DisFS result = null;
            result = new FuzzyProcess().ReadEachDisFS(path + name + ".disFS");//2 is value of user input
            return result;
        }
        public ConFS GetConFS(string path, string name)
        {
            ConFS result = null;
            result = new FuzzyProcess().ReadEachConFS(path + name + ".conFS");//2 is value of user input
            return result;
        }

        //----
        //public Boolean Satisfy(List<Item> list, FzTupleEntity tuple)
        //{
        //    this._resultTuple = new FzTupleEntity() { ValuesOnPerRow = tuple.ValuesOnPerRow };//select * from rpatient where age <> "old"
        //    //_uRelation = Convert.ToDouble(tuple.ValuesOnPerRow[tuple.ValuesOnPerRow.Count - 1]);
        //    _uRelation = (tuple.ValuesOnPerRow[tuple.ValuesOnPerRow.Count - 1]).ToString();
        //    this._memberships = new List<string>();

        //    String logicText = String.Empty;

        //    for (int i = 0; i < list.Count; i++)
        //    {
               
        //        if (list[i].elements.Count==4)// contains not : not age = 23
        //        {
        //            if (SatisfyItem(list[i].elements.GetRange(1,3), tuple, i - 1))
        //                logicText += "0" + list[i].nextLogic;
        //            else
        //                logicText += "1" + list[i].nextLogic;
        //        }
        //        if (list[i].elements.Count == 3)// single expression: age='young'.
        //        {
        //            if (SatisfyItem(list[i].elements, tuple, i - 1))
        //                logicText += "1" + list[i].nextLogic;
        //            else
        //                logicText += "0" + list[i].nextLogic;
        //        }
        //        else// Multiple expression: weight='heavy' and height>165
        //        {
        //            List<Item> subItems = CreateSubItems(list[i].elements);
        //            Boolean end = Satisfy(subItems, tuple);
        //            if (end)
        //                logicText += "1" + list[i].nextLogic;
        //            else
        //                logicText += "0" + list[i].nextLogic;
        //        }
        //    }

        //    logicText = ReplaceLogicality(logicText);
        //    _resultTuple.ValuesOnPerRow[_resultTuple.ValuesOnPerRow.Count - 1] = UpdateMembership(_memberships);
        //    return CalulateLogic(logicText);
        //}
        public string Satisfy(List<Item> list, FzTupleEntity tuple)
        {
            
            this._resultTuple = new FzTupleEntity() { ValuesOnPerRow = tuple.ValuesOnPerRow };//select * from rpatient where age <> "old"
            _uRelation = (tuple.ValuesOnPerRow[tuple.ValuesOnPerRow.Count - 1]).ToString();
            List<string> _memberships = new List<string>();
            for (int i = 0; i < list.Count; i++)
            {
                String membership = string.Empty;
                if (list[i].elements.Count == 3 && list[i].aggregateFunction == "")// single expression: age='young'.
                {
                   membership= SatisfyItem(list[i].elements, tuple, i - 1);
                }
                else if (list[i].elements.Count != 3 && list[i].aggregateFunction == "")// Multiple expression: weight='heavy' and height>165
                {
                   List<Item> subItems = CreateSubItems(list[i].elements);
                    membership= Satisfy(subItems, tuple);
                }
                else if (list[i].aggregateFunction != "")
                {
                    membership = SatisfyAggregatetionFunction(list[i], tuple, i - 1);// use for min, max,..
                }
                if (list[i].notElement)
                {
                    string path = Directory.GetCurrentDirectory() + @"\lib\temp\";
                    DisFS FSMembership = GetDisFS(path, membership);
                    if (FSMembership != null)
                    {
                        DisFS dis = new DisFS();
                        dis.ValueSet.Add(1);
                        dis.MembershipSet.Add(1);
                        membership = Diff_DisFS(dis, FSMembership);
                    }
                    else
                        membership = (1 - Convert.ToDouble(membership)).ToString();
                }
                if (membership != "0" && list[i].ItemName == "having")
                {
                    if(list[i].aggregateFunction !="")
                        ItemConditions[i].valueAggregate = list[i].valueAggregate;
                    ItemConditions[i].resultCondition = true;
                }
                if (i != 0 && _memberships.Count > 0)// Getting previous logicality
                    _memberships.Add(list[i-1].nextLogic);
                _memberships.Add(membership.ToString());
            }
            string NewMembership = UpdateMembership(_memberships);
            DisFS FSMembership_New = GetDisFS(Directory.GetCurrentDirectory() + @"\lib\temp\", NewMembership);
            if (FSMembership_New != null && FSMembership_New.ValueSet.Count == 1 && FSMembership_New.MembershipSet[0] == 0)
                NewMembership = "0";
            _resultTuple.ValuesOnPerRow[_resultTuple.ValuesOnPerRow.Count - 1] = NewMembership;
            return NewMembership;
        }

        public Item findAndMarkAggregatetion (Item condition, List<FzTupleEntity> filterResultHaving)
        {
            condition.resultCondition = false;
            string operation = condition.elements[1];// 3 = 40
            string aggregatetionFunction = condition.aggregateFunction;// min, max, avg,...
            double a = 0, b = 0;
            b = double.Parse(condition.elements[2]);
            double[] arr = new double[filterResultHaving.Count];
            if (aggregatetionFunction != "count")
            {
                for (int q = 0; q < filterResultHaving.Count; q++)
                {
                    arr[q] = double.Parse(filterResultHaving[q].ValuesOnPerRow[int.Parse(condition.elements[0])].ToString());
                }
            }
            else if (aggregatetionFunction == "count")
            {
                for (int q = 0; q < filterResultHaving.Count; q++)
                {
                    a++;
                }
            }


            switch (aggregatetionFunction)
            {
                case "min": a = arr.Min();break;
                case "max": a = arr.Max();break;
                case "avg": a = arr.Average();break;
                case "sum": a = arr.Sum();break;
                //case "count": a++ below
            }

            switch (operation)
            {
                case "=":
                        if (a == b)
                        {
                            condition.resultCondition = true;
                            condition.valueAggregate = a;
                        }
                        break;
                    
                case ">":
                        if (a > b)
                        {
                            condition.resultCondition = true;
                            condition.valueAggregate = a;
                        }
                        break;
                case "<":
                        if (a < b)
                        {
                            condition.resultCondition = true;
                            condition.valueAggregate = a;
                        }
                        break;
                case ">=":
                    if (a >= b)
                    {
                        condition.resultCondition = true;
                        condition.valueAggregate = a;
                    }
                    break;
                case "<=":
                    if (a <= b)
                    {
                        condition.resultCondition = true;
                        condition.valueAggregate = a;
                    }
                    break;
            }
            return condition;     
        }

        #endregion

        #region 5. Privates
        /// <summary>
        /// 
        /// </summary>
        //private Double UpdateMembership(List<String> memberships)
        //{
        //    if (memberships.Count == 0) return Convert.ToDouble(_resultTuple.ValuesOnPerRow[_resultTuple.ValuesOnPerRow.Count - 1]);

        //    Double result = Convert.ToDouble(memberships[0]);
        //    while (memberships.Count > 1)//0.9 and 0.8 or 0.5
        //    {
        //        Double v1 = Convert.ToDouble(memberships[0]);
        //        Double v2 = Convert.ToDouble(memberships[2]);
        //        switch (memberships[1])
        //        {
        //            case " and ": result = Math.Min(v1, v2); break;
        //            case " or ": result = Math.Max(v1, v2); break;
        //            case " not ": result = Math.Min(v1, 1 - v2); break;
        //        }
        //        memberships.RemoveRange(0, 2);
        //        memberships[0] = result.ToString();
        //    }
        //    return result;
        //}
        //edit-----
        private string UpdateMembership(List<String> memberships)
        {
            if (memberships.Count == 0) return (_resultTuple.ValuesOnPerRow[_resultTuple.ValuesOnPerRow.Count - 1]).ToString();
            string result = (memberships[0]).ToString();
            while (memberships.Count > 1)//0.9 and 0.8 or 0.5
            {
                //edit----
                string path = Directory.GetCurrentDirectory() + @"\lib\temp\"; //edit
                DisFS dis1 = GetDisFS(path, memberships[0].ToString());
                DisFS dis2 = GetDisFS(path, memberships[2].ToString());
                Double v1 = 0;
                Double v2 = 0;
                if (dis1 == null && dis2 == null)
                {
                    v1 = Convert.ToDouble(memberships[0]);
                    v2 = Convert.ToDouble(memberships[2]);
                    switch (memberships[1])
                    {
                        case " and ": result = Math.Min(v1, v2).ToString(); break;
                        case " or ": result = Math.Max(v1, v2).ToString(); break;
                        case " not ": result = Math.Min(v1, 1 - v2).ToString(); break;
                    }
                }
                if (dis1 != null || dis2 != null)
                {
                    if (dis1 == null && dis2 != null)
                    {
                        v1 = Convert.ToDouble(memberships[0]);
                        dis1 = new DisFS();
                        dis1.ValueSet.Add(v1);
                        dis1.MembershipSet.Add(1);
                        dis1.V = v1.ToString();
                        dis1.M = "1";
                    }
                    if (dis1 != null && dis2 == null)
                    {
                        v2 = Convert.ToDouble(memberships[2]);
                        dis2 = new DisFS();
                        dis2.ValueSet.Add(v2);
                        dis2.MembershipSet.Add(1);
                        dis2.V = v2.ToString();
                        dis2.M = "1";
                    }
                    switch (memberships[1])
                    {
                        case " and ": result = Min_DisFS(dis1, dis2).ToString(); break;
                        case " or ": result = Max_DisFS(dis1, dis2).ToString(); break;
                            // case " not ": result = Math.Min(v1, 1 - v2).ToString(); break;
                    }
                }
                memberships.RemoveRange(0, 2);
                memberships[0] = result.ToString();
            }
            return result;
        }
        //---
        private List<Item> CreateSubItems(List<String> itemCondition)
        {
            List<String> items = new List<string>();
            foreach (var item in itemCondition)
            {
                items.Add(item);
            }
            List<Item> subItems = new List<Item>();
            while (items.Count > 0)
            {
                Item item = new Item();
                if (items[0] == "not")
                {
                    item.notElement = true;
                    item.elements.Add(items[1]);
                    item.elements.Add(items[2]);
                    item.elements.Add(items[3]);
                    if (items.Count >= 5)
                    {
                        item.nextLogic = items[4];
                        subItems.Add(item);
                        items.RemoveRange(0, 5);
                    }
                    else// No need to add because the nextLogic default is empty
                    {
                        subItems.Add(item);
                        items.RemoveRange(0, 4);
                    }
                }
                else
                {
                    item.elements.Add(items[0]);
                    item.elements.Add(items[1]);
                    item.elements.Add(items[2]);
                    if (items.Count >= 4)
                    {
                        item.nextLogic = items[3];
                        subItems.Add(item);
                        items.RemoveRange(0, 4);
                    }
                    else// No need to add because the nextLogic default is empty
                    {
                        subItems.Add(item);
                        items.RemoveRange(0, 3);
                    }
                }
            }

            return subItems;
        }

        private String ReplaceLogicality(String logicText)
        {
            logicText = logicText.Replace(" and ", "&");
            logicText = logicText.Replace(" or ", "|");
            logicText = logicText.Replace(" not ", "!");
            return logicText;
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean CalulateLogic(String l)
        {
            try
            {
                string bit = "";
                while (l.Length > 1)
                {
                    bool v1 = (l[0].CompareTo('1') == 0) ? true : false;
                    bool v2 = (l[2].CompareTo('1') == 0) ? true : false;

                    switch (l[1])
                    {
                        case '&': bit = ((v1 && v2) ? "1" : "0");
                            break;
                        case '|': bit = ((v1 || v2) ? "1" : "0");
                            break;
                        case '!': bit = ((v1 != v2) ? "1" : "0");
                            break;
                    }

                    l = l.Remove(0, 3);//false && false != false && true || true;
                    l = bit + l;
                }
            }
            catch (Exception ex)
            {
                this._errorMessage = ex.Message;
            }
            return (l.CompareTo("1") == 0) ? true : false;
        }

        /// <summary>
        ///  The variable i for getting the previous logicality
        /// </summary>
        //private Boolean SatisfyItem(List<String> itemCondition, FzTupleEntity tuple, int i)
        //{
        //    int indexAttr = Convert.ToInt32(itemCondition[0]);
        //    String dataType = this._attributes[indexAttr].DataType.DataType;
        //    Object value = tuple.ValuesOnPerRow[indexAttr];//we don't know the data type of value
        //    int count = 0;
        //    String fs = itemCondition[2];
        //        //fs = itemCondition[2].Substring(1, itemCondition[2].Length - 2);;
        //    ContinuousFuzzySetBLL conFS = null;
        //    DiscreteFuzzySetBLL disFS = null;

        //    if (itemCondition[1] == "->" || itemCondition[1] == "→")
        //    {
        //        //fs = fs.Substring(1, fs.Length - 2);
        //        conFS = new ContinuousFuzzySetBLL().GetByName(fs);
        //        disFS = new DiscreteFuzzySetBLL().GetByName(fs);//2 is value of user input
        //    }
        //    if (conFS != null)//continuous fuzzy set is priorer than discrete fuzzy set
        //    {
        //        //itemCondition[1] is operator, uValue is the membership of the value on current cell for the selected fuzzy set
        //        Double uValue = FuzzyCompare(Convert.ToDouble(value), conFS, itemCondition[1]);
        //        uValue = Math.Min(uValue, _uRelation);//Update the min value
        //        if (uValue != 0)
        //        {
        //            if (i != -1 && _memberships.Count > 0)// Getting previous logicality
        //                _memberships.Add(_itemConditions[i].nextLogic);
        //            _memberships.Add(uValue.ToString());
        //            count++;
        //        }
        //    }
        //    if (disFS != null && conFS == null)
        //    {
        //        Double uValue = FuzzyCompare(Convert.ToDouble(value), disFS, itemCondition[1]);
        //        uValue = Math.Min(uValue, _uRelation);//Update the min value
        //        if (uValue != 0)
        //        {
        //            if (i != -1 && _memberships.Count > 0)// Getting previous logicality
        //                _memberships.Add(_itemConditions[i].nextLogic);
        //            _memberships.Add(uValue.ToString());
        //            count++;
        //        }
        //    }
        //    if (disFS == null && conFS == null)
        //    {
        //        //if (fs.Contains("\""))
        //        //    fs = fs.Substring(1, fs.Length - 2);
        //        if (ObjectCompare(value, fs, itemCondition[1], dataType))
        //        {
        //            count++;
        //        }
        //    }

        //    if (count == 1)//it mean the tuple is satisfied with all the compare operative
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        public string FindAndMarkFuzzy(string dis1, string dis2)
        {
            string FSName = "";

            if (dis1 == "")
            FSName = dis2;
            else if (dis2 == "") FSName = dis1;
            if (dis1 == "" || dis2 == "")
                return FSName;

            DisFS disFS1 = null;
            DisFS disFS2 = null;
            string path = "";
            List<String> arr = new List<string>();
            arr.Add(dis1);
            arr.Add(dis2);
            for(int o = 0; o < arr.Count(); o++)
            {
                FzContinuousFuzzySetEntity get_conFS = ContinuousFuzzySetBLL.GetConFNByName(arr[o].ToString(), _fdbEntity);
                ConFS conFS1 = null;
                if (get_conFS != null)
                {
                    conFS1 = new ConFS(get_conFS.Name, get_conFS.Bottom_Left, get_conFS.Top_Left, get_conFS.Top_Right, get_conFS.Bottom_Right);
                    disFS2 = transContoDis(conFS1); //trans CF to DF
                }
                else
                {
                    FzDiscreteFuzzySetEntity get_disFS = DiscreteFuzzySetBLL.GetDisFNByName(arr[o].ToString(), _fdbEntity);
                    if (get_disFS != null)
                        disFS2 = new DisFS(get_disFS.Name, get_disFS.V, get_disFS.M, get_disFS.ValueSet, get_disFS.MembershipSet);
                    else if (!IsNumber(arr[o]) && !arr[o].Contains("appox_"))
                        return "FN not exists";
                }
                if (o == 0)
                {
                    disFS1 = disFS2;
                    disFS2 = null;
                }
            }
            path = Directory.GetCurrentDirectory() + @"\lib\temp\";
            if(disFS1 == null)
                disFS1 = GetDisFS(path, dis1);
            if (disFS2 == null) disFS2 = GetDisFS(path, dis2);
            if (disFS1 != null && disFS2 != null)
                FSName = Min_DisFS(disFS1, disFS2);
            else if (disFS1 == null && disFS2 != null)
            {
                DisFS dis = new DisFS();
                dis.ValueSet.Add(double.Parse(dis1));
                dis.MembershipSet.Add(1);
                FSName = Min_DisFS(dis, disFS2);
            }
            else if (disFS1 != null && disFS2 == null)
            {
                DisFS dis = new DisFS();
                dis.ValueSet.Add(double.Parse(dis2));
                dis.MembershipSet.Add(1);
                FSName = Min_DisFS(disFS1, dis);
            }
            else if (disFS1 == null && disFS2 == null)
            {
                FSName = Math.Min(double.Parse(dis1), double.Parse(dis2)).ToString();
            }
            return FSName;
        }


        public string FindAndMarkFuzzy_Max(string dis1, string dis2)
        {
            string FSName = "";

            if (dis1 == "")
                FSName = dis2;
            else if (dis2 == "") FSName = dis1;
            if (dis1 == "" || dis2 == "")
                return FSName;

            DisFS disFS1 = null;
            DisFS disFS2 = null;
            string path = "";
            List<String> arr = new List<string>();
            arr.Add(dis1);
            arr.Add(dis2);
            for (int o = 0; o < arr.Count(); o++)
            {
                FzContinuousFuzzySetEntity get_conFS = ContinuousFuzzySetBLL.GetConFNByName(arr[o].ToString(), _fdbEntity);
                ConFS conFS1 = null;
                if (get_conFS != null)
                {
                    conFS1 = new ConFS(get_conFS.Name, get_conFS.Bottom_Left, get_conFS.Top_Left, get_conFS.Top_Right, get_conFS.Bottom_Right);
                    disFS2 = transContoDis(conFS1); //trans CF to DF
                }
                else
                {
                    FzDiscreteFuzzySetEntity get_disFS = DiscreteFuzzySetBLL.GetDisFNByName(arr[o].ToString(), _fdbEntity);
                    if (get_disFS != null)
                        disFS2 = new DisFS(get_disFS.Name, get_disFS.V, get_disFS.M, get_disFS.ValueSet, get_disFS.MembershipSet);
                    else if (!IsNumber(arr[o]) && !arr[o].Contains("appox_"))
                        return "FN not exists";
                }
                if (o == 0)
                {
                    disFS1 = disFS2;
                    disFS2 = null;
                }
            }
            path = Directory.GetCurrentDirectory() + @"\lib\temp\";
            if (disFS1 == null)
                disFS1 = GetDisFS(path, dis1);
            if (disFS2 == null) disFS2 = GetDisFS(path, dis2);
            if (disFS1 != null && disFS2 != null)
                FSName = Max_DisFS(disFS1, disFS2);
            else if (disFS1 == null && disFS2 != null)
            {
                DisFS dis = new DisFS();
                dis.ValueSet.Add(double.Parse(dis1));
                dis.MembershipSet.Add(1);
                FSName = Max_DisFS(dis, disFS2);
            }
            else if (disFS1 != null && disFS2 == null)
            {
                DisFS dis = new DisFS();
                dis.ValueSet.Add(double.Parse(dis2));
                dis.MembershipSet.Add(1);
                FSName = Max_DisFS(disFS1, dis);
            }
            else if (disFS1 == null && disFS2 == null)
            {
                FSName = Math.Max(double.Parse(dis1), double.Parse(dis2)).ToString();
            }
            return FSName;
        }
        //public string FindAndMarkFuzzy(string dis1, string dis2, Boolean filter)
        //{
        //    DisFS disFS1 = null;
        //    DisFS disFS2 = null;
        //    string path = "";
        //    if (!filter)
        //    {
        //        FzContinuousFuzzySetEntity get_conFS = ContinuousFuzzySetBLL.GetConFNByName(dis1, _fdbEntity);
        //        ConFS conFS1 = null;
        //        if (get_conFS != null)
        //        {
        //            conFS1 = new ConFS(get_conFS.Name, get_conFS.Bottom_Left, get_conFS.Top_Left, get_conFS.Top_Right, get_conFS.Bottom_Right);
        //            disFS1 = transContoDis(conFS1); //trans CF to DF
        //        }
        //        else
        //        {
        //            FzDiscreteFuzzySetEntity get_disFS = DiscreteFuzzySetBLL.GetDisFNByName(dis1, _fdbEntity);
        //            if (get_disFS != null)
        //                disFS1 = new DisFS(get_disFS.Name, get_disFS.V, get_disFS.M, get_disFS.ValueSet, get_disFS.MembershipSet);
        //            else if (!IsNumber(dis1))
        //                return "FN not exists";
        //        }

        //    }
        //    path = Directory.GetCurrentDirectory() + @"\lib\temp\";
        //    string FSName = "";
        //    if (dis2 == "")
        //    {
        //       FSName = dis1;
        //       //if(disFS1 != null)
        //       // {
        //       //     String Name = "appox_" + Random();
        //       //     List<String> resultFz = new List<String>();
        //       //     resultFz.Add(disFS1.V);
        //       //     resultFz.Add(disFS1.M);
        //       //     System.Diagnostics.Debug.WriteLine("Fz: " + disFS1.Name + ", " + disFS1.Name);
        //       //     System.Diagnostics.Debug.WriteLine("Value: " + "" + ", Member: " + "");
        //       //     if (new FuzzyProcess().UpdateFS(path, resultFz, Name + ".disFS") == 1)
        //       //     {
        //       //         System.Diagnostics.Debug.WriteLine(Name);
        //       //         FSName = Name;
        //       //     }
        //       // }

        //    }
        //    else if (dis2 != "")
        //    {
        //        if(filter)
        //            disFS1 = GetDisFS(path, dis1);
        //        disFS2 = GetDisFS(path, dis2);
        //        if (disFS1 != null && disFS2 != null)
        //            FSName = Min_DisFS(disFS1, disFS2);
        //        else if (disFS1 == null && disFS2 != null)
        //        {
        //            DisFS dis = new DisFS();
        //            dis.ValueSet.Add(double.Parse(dis1));
        //            dis.MembershipSet.Add(1);
        //            FSName = Min_DisFS(dis, disFS2);
        //        }
        //        else if (disFS1 != null && disFS2 == null)
        //        {
        //            DisFS dis = new DisFS();
        //            dis.ValueSet.Add(double.Parse(dis2));
        //            dis.MembershipSet.Add(1);
        //            FSName = Min_DisFS(disFS1, dis);
        //        }
        //        else if (disFS1 == null && disFS2 == null)
        //        {
        //            FSName = Math.Min(double.Parse(dis1), double.Parse(dis2)).ToString();
        //        }
        //    }
        //    return FSName;
        //}

        public DisFS getDisFS(String memberShip, FdbEntity fdbEntity)
        {
            FzContinuousFuzzySetEntity get_conFS = ContinuousFuzzySetBLL.GetConFNByName(memberShip, fdbEntity);
            ConFS conFS_uRelation = null;
            DisFS disFS_uRelation = null;
            if (get_conFS != null)
            {
                conFS_uRelation = new ConFS(get_conFS.Name, get_conFS.Bottom_Left, get_conFS.Top_Left, get_conFS.Top_Right, get_conFS.Bottom_Right);
                disFS_uRelation = transContoDis(conFS_uRelation); //trans CF to DF
            }
            else
            {
                FzDiscreteFuzzySetEntity get_disFS = DiscreteFuzzySetBLL.GetDisFNByName(memberShip, fdbEntity);
                if (get_disFS != null)
                    disFS_uRelation = new DisFS(get_disFS.Name, get_disFS.V, get_disFS.M, get_disFS.ValueSet, get_disFS.MembershipSet);
                else
                {
                    string tempPath = Directory.GetCurrentDirectory() + @"\lib\temp\" + memberShip + ".disFS";
                    disFS_uRelation = new FuzzyProcess().ReadEachDisFS(tempPath);
                }
                if (disFS_uRelation == null && !IsNumber(memberShip))
                    return null;

            }
            return disFS_uRelation;
        }

        private string SatisfyItem(List<String> itemCondition, FzTupleEntity tuple, int i)
        {
            int indexAttr = Convert.ToInt32(itemCondition[0]);
            String dataType = this._attributes[indexAttr].DataType.DataType;
            Object value = tuple.ValuesOnPerRow[indexAttr];//we don't know the data type of value
            String fs = itemCondition[2];
            String result = "0";
            //fs = itemCondition[2].Substring(1, itemCondition[2].Length - 2);;
            ConFS conFS = null;
            DisFS disFS = null;
            string path = Directory.GetCurrentDirectory() +  @"\lib\";
            //----edit check _uRelation is fuzzyset or number
            int check_uRelation = 0;
            FzContinuousFuzzySetEntity get_conFS = ContinuousFuzzySetBLL.GetConFNByName(_uRelation, _fdbEntity);
            ConFS conFS_uRelation = null;
            DisFS disFS_uRelation = null;
            if (get_conFS != null)
            {
                conFS_uRelation = new ConFS(get_conFS.Name, get_conFS.Bottom_Left, get_conFS.Top_Left, get_conFS.Top_Right, get_conFS.Bottom_Right);
                disFS_uRelation = transContoDis(conFS_uRelation); //trans CF to DF
            }
            else
            {
                FzDiscreteFuzzySetEntity get_disFS = DiscreteFuzzySetBLL.GetDisFNByName(_uRelation, _fdbEntity);
                if (get_disFS != null)
                    disFS_uRelation = new DisFS(get_disFS.Name, get_disFS.V, get_disFS.M, get_disFS.ValueSet, get_disFS.MembershipSet);
                else
                {
                    string tempPath = Directory.GetCurrentDirectory() + @"\lib\temp\" + _uRelation + ".disFS";
                    disFS_uRelation = new FuzzyProcess().ReadEachDisFS(tempPath);
                }
                if (disFS_uRelation == null && !IsNumber(_uRelation))
                    return "FN not exists";

            }
            if (conFS_uRelation!= null || disFS_uRelation != null)
                check_uRelation = 1; //_uRelation is a fuzzyset
            if (itemCondition[1] == "->" || itemCondition[1] == "→")
            {
                conFS = GetConFS(path, fs);
                disFS = GetDisFS(path, fs);
            }
            if (conFS != null)//continuous fuzzy set is priorer than discrete fuzzy set
            {
                Double uValue = FuzzyCompare(Convert.ToDouble(value), conFS, itemCondition[1]);
                if (check_uRelation == 0) //if _uRelation is a number
                {
                    uValue = Math.Min(uValue, Convert.ToDouble(_uRelation));//Update the min value
                    result= uValue.ToString();
                }
                if (check_uRelation == 1) //if _uRelation is fuzzyset 
                {
                    DisFS uDisFS = new DisFS();
                    uDisFS.ValueSet.Add(uValue);
                    uDisFS.MembershipSet.Add(1);
                    string FSName = Min_DisFS(disFS_uRelation, uDisFS);
                    result= FSName;
                }
            }
            if (disFS != null && conFS == null)
            {
                Double uValue = FuzzyCompare(Convert.ToDouble(value), disFS, itemCondition[1]);
                if (check_uRelation == 0) //if _uRelation is a number
                {
                    uValue = Math.Min(uValue, Convert.ToDouble(_uRelation));//Update the min value
                    result= uValue.ToString();
                }
                if (check_uRelation == 1) //if _uRelation is fuzzyset //hỏi lại
                {
                    DisFS uDisFS = new DisFS();
                    uDisFS.ValueSet.Add(uValue);
                    uDisFS.MembershipSet.Add(1);
                    string FSName = Min_DisFS(disFS_uRelation, uDisFS);
                    result= FSName;
                }
            }
            if (disFS == null && conFS == null)
            {
                if (ObjectCompare(value, fs, itemCondition[1], dataType))
                {
                    if (check_uRelation==0)
                        result= tuple.ValuesOnPerRow[tuple.ValuesOnPerRow.Count - 1].ToString();
                    if (check_uRelation==1)
                    {
                        DisFS uDisFS = new DisFS();
                        uDisFS.ValueSet.Add(1);
                        uDisFS.MembershipSet.Add(1);
                        string FSName = Min_DisFS(disFS_uRelation, uDisFS);
                        result= FSName;
                    }
                }
            }
            return result;
        }

        private string SatisfyAggregatetionFunction(Item itemCondition, FzTupleEntity tuple, int i)
        {
            String result = "0";
            string path = Directory.GetCurrentDirectory() + @"\lib\";
            //----edit check _uRelation is fuzzyset or number
            int check_uRelation = 0;
            FzContinuousFuzzySetEntity get_conFS = ContinuousFuzzySetBLL.GetConFNByName(_uRelation, _fdbEntity);
            ConFS conFS_uRelation = null;
            DisFS disFS_uRelation = null;
            if (get_conFS != null)
            {
                conFS_uRelation = new ConFS(get_conFS.Name, get_conFS.Bottom_Left, get_conFS.Top_Left, get_conFS.Top_Right, get_conFS.Bottom_Right);
                disFS_uRelation = transContoDis(conFS_uRelation); //trans CF to DF
            }
            else
            {
                FzDiscreteFuzzySetEntity get_disFS = DiscreteFuzzySetBLL.GetDisFNByName(_uRelation, _fdbEntity);
                if (get_disFS != null)
                    disFS_uRelation = new DisFS(get_disFS.Name, get_disFS.V, get_disFS.M, get_disFS.ValueSet, get_disFS.MembershipSet);
                else if (!IsNumber(_uRelation) && !_uRelation.Contains("appox_"))
                    return "FN not exists";

            }
            if (conFS_uRelation != null || disFS_uRelation != null)
                check_uRelation = 1; //_uRelation is a fuzzyset

            if (itemCondition.resultCondition)//if aggregatetion function is true else membership = 0
            {
                if (check_uRelation == 0)
                {
                    result = tuple.ValuesOnPerRow[tuple.ValuesOnPerRow.Count - 1].ToString();
                       
                }
                if (check_uRelation == 1)
                {
                    DisFS uDisFS = new DisFS();
                    uDisFS.ValueSet.Add(1);
                    uDisFS.MembershipSet.Add(1);
                    string FSName = Min_DisFS(disFS_uRelation, uDisFS);
                    result = FSName;
                       
                }
            }
            
            return result;
        }
        //private string SatisfyItem(List<String> itemCondition, FzTupleEntity tuple, int i)
        //{
        //    int indexAttr = Convert.ToInt32(itemCondition[0]);
        //    String dataType = this._attributes[indexAttr].DataType.DataType;
        //    Object value = tuple.ValuesOnPerRow[indexAttr];//we don't know the data type of value
        //    String fs = itemCondition[2];
        //    String result = "0";
        //    //fs = itemCondition[2].Substring(1, itemCondition[2].Length - 2);;
        //    ConFS conFS = null;
        //    DisFS disFS = null;
        //    string path = Directory.GetCurrentDirectory() + @"\lib\";
        //    //----edit check _uRelation is fuzzyset or number
        //    int check_uRelation = 0;
        //    ConFS conFS_uRelation = GetConFS(path + @"\membershipFS\", _uRelation);
        //    DisFS disFS_uRelation = GetDisFS(path + @"\membershipFS\", _uRelation);
        //    if (conFS_uRelation != null)
        //        disFS_uRelation = transContoDis(conFS_uRelation); //trans CF to DF
        //    if (conFS_uRelation != null || disFS_uRelation != null)
        //        check_uRelation = 1; //_uRelation is a fuzzyset
        //    if (itemCondition[1] == "->" || itemCondition[1] == "→")
        //    {
        //        //fs = fs.Substring(1, fs.Length - 2);
        //        //conFS = new FuzzyProcess().ReadEachConFS(path + fs + ".conFS");
        //        //disFS = new FuzzyProcess().ReadEachDisFS(path + fs + ".disFS");//2 is value of user input
        //        conFS = GetConFS(path, fs);
        //        disFS = GetDisFS(path, fs);
        //    }
        //    if (conFS != null)//continuous fuzzy set is priorer than discrete fuzzy set
        //    {
        //        //itemCondition[1] is operator, uValue is the membership of the value on current cell for the selected fuzzy set
        //        Double uValue = FuzzyCompare(Convert.ToDouble(value), conFS, itemCondition[1]);
        //        if (check_uRelation == 0) //if _uRelation is a number
        //        {
        //            uValue = Math.Min(uValue, Convert.ToDouble(_uRelation));//Update the min value
        //            result = uValue.ToString();
        //            //if (i != -1 && _memberships.Count > 0)// Getting previous logicality
        //            //    _memberships.Add(_itemConditions[i].nextLogic);
        //            //_memberships.Add(uValue.ToString());
        //        }
        //        if (check_uRelation == 1) //if _uRelation is fuzzyset 
        //        {
        //            DisFS uDisFS = new DisFS();
        //            uDisFS.ValueSet.Add(uValue);
        //            uDisFS.MembershipSet.Add(1);
        //            string FSName = Min_DisFS(disFS_uRelation, uDisFS);
        //            result = FSName;
        //            //if (i != -1 && _memberships.Count > 0)// Getting previous logicality
        //            //    _memberships.Add(_itemConditions[i].nextLogic);
        //            //_memberships.Add(FSName);
        //        }
        //    }
        //    if (disFS != null && conFS == null)
        //    {
        //        Double uValue = FuzzyCompare(Convert.ToDouble(value), disFS, itemCondition[1]);
        //        if (check_uRelation == 0) //if _uRelation is a number
        //        {
        //            uValue = Math.Min(uValue, Convert.ToDouble(_uRelation));//Update the min value
        //            result = uValue.ToString();
        //            //if (i != -1 && _memberships.Count > 0)// Getting previous logicality
        //            //    _memberships.Add(_itemConditions[i].nextLogic);
        //            //_memberships.Add(uValue.ToString());
        //        }
        //        if (check_uRelation == 1) //if _uRelation is fuzzyset //hỏi lại
        //        {
        //            DisFS uDisFS = new DisFS();
        //            uDisFS.ValueSet.Add(uValue);
        //            uDisFS.MembershipSet.Add(1);
        //            string FSName = Min_DisFS(disFS_uRelation, uDisFS);
        //            result = FSName;
        //            //if (i != -1 && _memberships.Count > 0)// Getting previous logicality
        //            //    _memberships.Add(_itemConditions[i].nextLogic);
        //            //_memberships.Add(FSName);
        //        }
        //    }
        //    if (disFS == null && conFS == null)
        //    {
        //        //if (fs.Contains("\""))
        //        //    fs = fs.Substring(1, fs.Length - 2);
        //        if (ObjectCompare(value, fs, itemCondition[1], dataType))
        //        {
        //            if (check_uRelation == 0)
        //            {
        //                result = tuple.ValuesOnPerRow[tuple.ValuesOnPerRow.Count - 1].ToString();
        //                //if (i != -1 && _memberships.Count > 0)// Getting previous logicality
        //                //    _memberships.Add(_itemConditions[i].nextLogic);
        //                //_memberships.Add(tuple.ValuesOnPerRow[tuple.ValuesOnPerRow.Count - 1].ToString());
        //            }
        //            if (check_uRelation == 1)
        //            {
        //                DisFS uDisFS = new DisFS();
        //                uDisFS.ValueSet.Add(1);
        //                uDisFS.MembershipSet.Add(1);
        //                string FSName = Min_DisFS(disFS_uRelation, uDisFS);
        //                result = FSName;
        //                //if (i != -1 && _memberships.Count > 0)// Getting previous logicality
        //                //    _memberships.Add(_itemConditions[i].nextLogic);
        //                //_memberships.Add(FSName);

        //            }
        //        }
        //    }
        //    return result;
        //}

        //private Boolean SatisfyItem(List<String> itemCondition, FzTupleEntity tuple, int i)
        //{
        //    int indexAttr = Convert.ToInt32(itemCondition[0]);
        //    String dataType = this._attributes[indexAttr].DataType.DataType;
        //    Object value = tuple.ValuesOnPerRow[indexAttr];//we don't know the data type of value
        //    int count = 0;
        //    String fs = itemCondition[2];
        //    //fs = itemCondition[2].Substring(1, itemCondition[2].Length - 2);;
        //    ConFS conFS = null;
        //    DisFS disFS = null;
        //    string path = Directory.GetCurrentDirectory() + @"\lib\";
        //    //----edit check _uRelation is fuzzyset or not
        //    int check_uRelation = 0;
        //    ConFS conFS_uRelation = GetConFS(path, _uRelation);
        //    DisFS disFS_uRelation = GetDisFS(path, _uRelation);
        //    if (conFS_uRelation != null)
        //    {
        //        disFS_uRelation = transContoDis(conFS_uRelation);
        //    }
        //    if (conFS_uRelation != null || disFS_uRelation != null)
        //    {
        //        check_uRelation = 1;
        //    }
        //    if (itemCondition[1] == "->" || itemCondition[1] == "→")
        //    {
        //        //fs = fs.Substring(1, fs.Length - 2);
        //        //conFS = new FuzzyProcess().ReadEachConFS(path + fs + ".conFS");
        //        //disFS = new FuzzyProcess().ReadEachDisFS(path + fs + ".disFS");//2 is value of user input
        //        conFS = GetConFS(path, fs);
        //        disFS = GetDisFS(path, fs);
        //    }
        //    if (conFS != null)//continuous fuzzy set is priorer than discrete fuzzy set
        //    {
        //        //itemCondition[1] is operator, uValue is the membership of the value on current cell for the selected fuzzy set
        //        Double uValue = FuzzyCompare(Convert.ToDouble(value), conFS, itemCondition[1]);
        //        if (check_uRelation == 0) //if _uRelation is a number
        //        {
        //            uValue = Math.Min(uValue, Convert.ToDouble(_uRelation));//Update the min value
        //            if (uValue != 0)
        //            {
        //                if (i != -1 && _memberships.Count > 0)// Getting previous logicality
        //                    _memberships.Add(_itemConditions[i].nextLogic);
        //                _memberships.Add(uValue.ToString());
        //                count++;
        //            }
        //        }
        //        if (check_uRelation == 1 && uValue != 0) //if _uRelation is fuzzyset //hỏi lại
        //        {
        //            DisFS uDisFS = new DisFS();
        //            uDisFS.ValueSet.Add(uValue);
        //            uDisFS.MembershipSet.Add(1);
        //            string FSName = Min_DisFS(disFS_uRelation, uDisFS);
        //            if (FSName != "-1")
        //            {
        //                if (i != -1 && _memberships.Count > 0)// Getting previous logicality
        //                    _memberships.Add(_itemConditions[i].nextLogic);
        //                _memberships.Add(FSName);
        //                count++;
        //            }
        //        }
        //    }
        //    if (disFS != null && conFS == null)
        //    {
        //        Double uValue = FuzzyCompare(Convert.ToDouble(value), disFS, itemCondition[1]);
        //        if (check_uRelation == 0) //if _uRelation is a number
        //        {
        //            uValue = Math.Min(uValue, Convert.ToDouble(_uRelation));//Update the min value
        //            if (uValue != 0)
        //            {
        //                if (i != -1 && _memberships.Count > 0)// Getting previous logicality
        //                    _memberships.Add(_itemConditions[i].nextLogic);
        //                _memberships.Add(uValue.ToString());
        //                count++;
        //            }
        //        }
        //        if (check_uRelation == 1 && uValue != 0) //if _uRelation is fuzzyset //hỏi lại
        //        {
        //            DisFS uDisFS = new DisFS();
        //            uDisFS.ValueSet.Add(uValue);
        //            uDisFS.MembershipSet.Add(1);
        //            string FSName = Min_DisFS(disFS_uRelation, uDisFS);
        //            if (FSName != "-1")
        //            {
        //                if (i != -1 && _memberships.Count > 0)// Getting previous logicality
        //                    _memberships.Add(_itemConditions[i].nextLogic);
        //                _memberships.Add(FSName);
        //                count++;
        //            }
        //        }
        //    }
        //    if (disFS == null && conFS == null)
        //    {
        //        //if (fs.Contains("\""))
        //        //    fs = fs.Substring(1, fs.Length - 2);
        //        if (ObjectCompare(value, fs, itemCondition[1], dataType))
        //        {
        //            count++;
        //        }
        //    }

        //    if (count == 1)//it mean the tuple is satisfied with all the compare operative
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        public static DisFS transContoDis (ConFS con)
        {
            DisFS result = new DisFS();
            Double value = con.Bottom_Left;
            Double membership;
            String values = "";
            String memberships = "";
            while (value <= con.Bottom_Right)
            {
                membership = con.GetMembershipAt(value);
                result.ValueSet.Add(value);
                result.MembershipSet.Add(membership);
                values += value.ToString() + ",";
                memberships += membership.ToString() + ",";
                value = Math.Round (value + 0.01,2);
            }
            values = values.Remove(values.LastIndexOf(","));
            memberships = memberships.Remove(memberships.LastIndexOf(","));
            result.V = values;
            result.M = memberships;
            return result;
        }
        //private ConFS transDistoCon (DisFS dis)
        //{
        //    ConFS result = new ConFS();
        //    int count = dis.ValueSet.Count;
        //    result.Bottom_Left = dis.ValueSet[0];
        //    result.Bottom_Right = dis.ValueSet[count- 1];
        //    for(int i=0;i<count-1;i++)
        //    {
        //        if (dis.MembershipSet[i] != 1 && dis.MembershipSet[i + 1] == 1)
        //            result.Top_Left = dis.ValueSet[i + 1];
        //        if (dis.MembershipSet[i] == 1 && dis.MembershipSet[i + 1] != 1)
        //            result.Top_Right = dis.ValueSet[i];
        //    }
        //    return result;
        //}
        public string Diff_DisFS (DisFS FuzzySet1, DisFS FuzzySet2) //FuzzySet1 - FuzzySet2
        {
            try
            {
                String Name = "appox_" + Random();
                List<String> result = new List<String>();
                String path = Directory.GetCurrentDirectory() + @"\lib\temp\";
                String values = "";
                String memberships = "";
                bool flag = false;
                for (int k1=0; k1< FuzzySet1.ValueSet.Count();k1++)
                {
                    flag = false;
                    for(int k2=0;k2< FuzzySet2.ValueSet.Count();k2++)
                    {
                        if (FuzzySet1.ValueSet[k1]==FuzzySet2.ValueSet[k2])
                        {
                            values += FuzzySet1.ValueSet[k1].ToString() + ",";
                            memberships += Math.Min(FuzzySet1.MembershipSet[k1], 1 - FuzzySet2.MembershipSet[k2]).ToString() + ",";
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        values += FuzzySet1.ValueSet[k1].ToString() + ",";
                        memberships += FuzzySet1.MembershipSet[k1].ToString() + ",";

                    }
                }
                    values = values.Remove(values.LastIndexOf(","));
                    memberships = memberships.Remove(memberships.LastIndexOf(","));
                    result.Add(values);
                    result.Add(memberships);
                if (new FuzzyProcess().UpdateFS(path, result, Name + ".disFS") == 1)
                {
                    return Name;
                }
                return "0";
        }
            catch  
            {
                return "0";
            }
        }
        public string Min_DisFS(DisFS FuzzySet1, DisFS FuzzySet2)
        {
            try
            {
                String Name = "appox_" + Random();
                List<String> result = new List<String>();
                String path = Directory.GetCurrentDirectory() + @"\lib\temp\";
                String values = "";
                String memberships = "";
                DisFS dis = new DisFS();
                if ((FuzzySet1.ValueSet.Count == 1 && FuzzySet1.ValueSet[0] == 0)|| (FuzzySet2.ValueSet.Count == 1 && FuzzySet2.ValueSet[0] == 0))
                {
                    return "0";
                }
                if (FuzzySet1.ValueSet.Count == 1 && FuzzySet1.ValueSet[0] == 1)
                {
                    result.Add(FuzzySet2.V);
                    result.Add(FuzzySet2.M);
                }
                if (FuzzySet2.ValueSet.Count == 1 && FuzzySet2.ValueSet[0] == 1 && result.Count() < 1)
                {
                    result.Add(FuzzySet1.V);
                    result.Add(FuzzySet1.M);
                }
                else
                {
                    for (int k1 = 0; k1 < FuzzySet1.ValueSet.Count(); k1++)
                    {
                        double sup = 0;
                        for (int k2 = 0; k2 < FuzzySet2.ValueSet.Count(); k2++)
                        {
                            if (FuzzySet1.ValueSet[k1] <= FuzzySet2.ValueSet[k2])
                            {
                                sup = Math.Max(Math.Min(FuzzySet1.MembershipSet[k1], FuzzySet2.MembershipSet[k2]), sup);
                            }
                        }
                        if (sup != 0)
                        {
                            dis.ValueSet.Add(FuzzySet1.ValueSet[k1]);
                            dis.MembershipSet.Add(sup);
                        }
                    }
                    for (int k2 = 0; k2 < FuzzySet2.ValueSet.Count(); k2++)
                    {
                        double sup1 = 0;
                        for (int k1 = 0; k1 < FuzzySet1.ValueSet.Count(); k1++)
                        {
                            if (FuzzySet2.ValueSet[k2] <= FuzzySet1.ValueSet[k1])
                            {
                                sup1 = Math.Max(Math.Min(FuzzySet1.MembershipSet[k1], FuzzySet2.MembershipSet[k2]), sup1);
                            }

                        }
                        if (sup1 != 0)
                        {
                            dis.ValueSet.Add(FuzzySet2.ValueSet[k2]);
                            dis.MembershipSet.Add(sup1);
                        }
                    }
                    Quicksort(dis.ValueSet, dis.MembershipSet, 0, dis.ValueSet.Count - 1);
                    for (int i = 0; i < dis.ValueSet.Count - 1; i++)
                    {
                        if (dis.ValueSet[i] == dis.ValueSet[i + 1])
                        {
                            if (dis.MembershipSet[i] >= dis.MembershipSet[i + 1])
                            {
                                dis.ValueSet.RemoveAt(i + 1);
                                dis.MembershipSet.RemoveAt(i + 1);
                            }
                            else
                            {
                                dis.ValueSet.RemoveAt(i);
                                dis.MembershipSet.RemoveAt(i);
                            }
                        }
                    }
                    for (int i = 0; i < dis.ValueSet.Count; i++)
                    {
                        values += dis.ValueSet[i].ToString() + ",";
                        memberships += dis.MembershipSet[i].ToString() + ",";
                    }

                    values = values.Remove(values.LastIndexOf(","));
                    memberships = memberships.Remove(memberships.LastIndexOf(","));
                    result.Add(values);
                    result.Add(memberships);
                }
                System.Diagnostics.Debug.WriteLine("Fz: " + FuzzySet1.Name + ", " + FuzzySet2.Name);
                System.Diagnostics.Debug.WriteLine("Value: " + values + ", Member: " + memberships);
                if (new FuzzyProcess().UpdateFS(path, result, Name + ".disFS") == 1)
                {
                    System.Diagnostics.Debug.WriteLine(Name);
                    return Name;
                }
                return "0";
            }
            catch  
            {
                return "0";
            }
        }
        public string Max_DisFS(DisFS FuzzySet1, DisFS FuzzySet2)
        {
                try
                {
                    String Name = "appox_" + Random();
                    List<String> result = new List<String>();
                    String path = Directory.GetCurrentDirectory() + @"\lib\temp\";
                    String values = "";
                    String memberships = "";
                    DisFS dis = new DisFS();
                    if ((FuzzySet1.ValueSet.Count == 1 && FuzzySet1.ValueSet[0] == 1 && FuzzySet1.MembershipSet[0] == 1) || (FuzzySet2.ValueSet.Count == 1 && FuzzySet2.ValueSet[0] == 1 && FuzzySet2.MembershipSet[0] == 1))
                    {
                        return "1";
                    }
                    if ((FuzzySet1.ValueSet.Count == 1 && FuzzySet1.ValueSet[0] == 0))
                    {
                        result.Add(FuzzySet2.V);
                        result.Add(FuzzySet2.M);
                    }
                    if ((FuzzySet2.ValueSet.Count == 1 && FuzzySet2.ValueSet[0] == 0) && result.Count() < 1)
                    {
                        result.Add(FuzzySet1.V);
                        result.Add(FuzzySet1.M);
                    }
                    else
                    {
                        for (int k1 = 0; k1 < FuzzySet1.ValueSet.Count(); k1++)
                        {
                            double sup = 0;
                            for (int k2 = 0; k2 < FuzzySet2.ValueSet.Count(); k2++)
                            {
                                if (FuzzySet1.ValueSet[k1] >= FuzzySet2.ValueSet[k2])
                                {
                                    sup = Math.Max(Math.Min(FuzzySet1.MembershipSet[k1], FuzzySet2.MembershipSet[k2]), sup);
                                }
                            }
                            if (sup != 0)
                            {
                                dis.ValueSet.Add(FuzzySet1.ValueSet[k1]);
                                dis.MembershipSet.Add(sup);
                            }
                        }
                        for (int k2 = 0; k2 < FuzzySet2.ValueSet.Count(); k2++)
                        {
                            double sup1 = 0;
                            for (int k1 = 0; k1 < FuzzySet1.ValueSet.Count(); k1++)
                            {
                                if (FuzzySet2.ValueSet[k2] >= FuzzySet1.ValueSet[k1])
                                {
                                    sup1 = Math.Max(Math.Min(FuzzySet1.MembershipSet[k1], FuzzySet2.MembershipSet[k2]), sup1);
                                }

                            }
                            if (sup1 != 0)
                            {
                                dis.ValueSet.Add(FuzzySet2.ValueSet[k2]);
                                dis.MembershipSet.Add(sup1);
                            }
                        }
                        Quicksort(dis.ValueSet, dis.MembershipSet, 0, dis.ValueSet.Count - 1);
                        for (int i = 0; i < dis.ValueSet.Count - 1; i++)
                        {
                            if (dis.ValueSet[i] == dis.ValueSet[i + 1])
                            {
                                if (dis.MembershipSet[i] >= dis.MembershipSet[i + 1])
                                {
                                    dis.ValueSet.RemoveAt(i + 1);
                                    dis.MembershipSet.RemoveAt(i + 1);
                                }
                                else
                                {
                                    dis.ValueSet.RemoveAt(i);
                                    dis.MembershipSet.RemoveAt(i);
                                }
                            }
                        }
                        for (int i = 0; i < dis.ValueSet.Count; i++)
                        {
                            values += dis.ValueSet[i].ToString() + ",";
                            memberships += dis.MembershipSet[i].ToString() + ",";
                        }

                        values = values.Remove(values.LastIndexOf(","));
                        memberships = memberships.Remove(memberships.LastIndexOf(","));
                        result.Add(values);
                        result.Add(memberships);
                    }
                    if (new FuzzyProcess().UpdateFS(path, result, Name + ".disFS") == 1)
                    {
                        return Name;
                    }
                    return "0";
                }
                catch
                {
                    return "0";
                }
            }
            
        //private string Max_DisFS(DisFS FuzzySet1, DisFS FuzzySet2)
        //{
        //    try
        //    {
        //        //DisFS result = new DisFS();
        //        String Name = "appox_"+ Random(); // chưa code hàm tính tên Fuzzyset
        //        List<String> result = new List<String>();
        //        String values = "";
        //        String memberships = "";

        //        for (int k1 = 0; k1 < FuzzySet1.ValueSet.Count(); k1++)
        //        {
        //            double sup = 0;
        //            for (int k2 = 0; k2 < FuzzySet2.ValueSet.Count(); k2++)
        //            {
        //                if (FuzzySet1.ValueSet[k1] >= FuzzySet2.ValueSet[k2])// hỏi lại
        //                {
        //                    sup = Math.Max(Math.Min(FuzzySet1.MembershipSet[k1], FuzzySet2.MembershipSet[k2]), sup);
        //                }
        //            }
        //            if (sup != 0)
        //            {
        //                values += FuzzySet1.ValueSet[k1].ToString() + ",";
        //                memberships += sup.ToString() + ",";
        //                // result.ValueSet.Add(FuzzySet.ValueSet[k]);
        //                //result.MembershipSet.Add(sup);
        //            }
        //        }
        //        for (int k2 = 0; k2 < FuzzySet2.ValueSet.Count(); k2++)
        //        {
        //            double sup1 = 0;
        //            for (int k1 = 0; k1 < FuzzySet1.ValueSet.Count(); k1++)
        //            {
        //                if (FuzzySet2.ValueSet[k2] >= FuzzySet1.ValueSet[k1]) //hỏi lại
        //                {
        //                    sup1 = Math.Max(Math.Min(FuzzySet1.MembershipSet[k1], FuzzySet2.MembershipSet[k2]), sup1);
        //                    //result.ValueSet.Add(Convert.ToDouble(FuzzyNum));
        //                    //result.MembershipSet.Add(sup1);
        //                }

        //            }
        //            if (sup1 != 0)
        //            {
        //                values += FuzzySet2.ValueSet[k2].ToString() + ",";
        //                memberships += sup1.ToString() + ",";
        //            }
        //        }
        //        String path = Directory.GetCurrentDirectory() + @"\lib\temp\";
        //        values = values.Remove(values.LastIndexOf(","));
        //        memberships = memberships.Remove(memberships.LastIndexOf(","));
        //        result.Add(values);
        //        result.Add(memberships);
        //        if (new FuzzyProcess().UpdateFS(path, result, Name + ".disFS") == 1)
        //        {
        //            return Name;
        //        }
        //        return "-1";
        //    }
        //    catch (Exception ex)
        //    {
        //        return "-1";
        //    }
        //}

        public static void Quicksort(List<Double> values, List<Double> memberships, int left, int right)
        {
            int i = left, j = right;
            Double pivot = values[(left+right) / 2];

            while (i <= j)
            {
                while (values[i].CompareTo(pivot) < 0)
                {
                    i++;
                }

                while (values[j].CompareTo(pivot) > 0)
                {
                    j--;
                }

                if (i <= j)
                {
                    // Swap
                    Double temp = values[j];
                    values[j] = values[i];
                    values[i] = temp;
                    temp = memberships[j];
                    memberships[j] = memberships[i];
                    memberships[i] = temp;
                    i++;
                    j--;
                }
            }

            // Recursive calls
            if (left < j)
            {
                Quicksort(values,memberships, left, j);
            }

            if (i < right)
            {
                Quicksort(values,memberships, i, right);
            }
        }

        private int GiveMeANumber()
        {
            var exclude = new HashSet<int>(random_array);
            var range = Enumerable.Range(1, 100).Where(i => !exclude.Contains(i));

            var rand = new Random();
            int index = rand.Next(1, 99 - exclude.Count);
            return range.ElementAt(index);
        }

        private String Random()
        {
            String result ="";
            int randomNumber = GiveMeANumber();
            Double randomDouble = (Double.Parse(randomNumber.ToString()) / 100);
            result = randomDouble.ToString();
            random_array.Add(randomNumber);
            logger.Debug($"{String.Join(" | ", random_array.Select((item) => item.ToString()).ToArray())} {result}");
            System.Diagnostics.Debug.WriteLine("Random array add: " + String.Join(",", random_array.Select(item => item.ToString()).ToArray()));
            
            return result;
        }
        private Boolean StringCompare(String a, String b, String opr)
        {
            //a = "\"" + a + "\"";
            //int indexOpen = 0, indexClose = 0;
            if ((b[0] != '\'' && b[b.Length - 1] != '\'') && (b[0] != '\"' && b[b.Length - 1] != '\"'))
            {
                this._errorMessage = "Missing quote near string";
                if (this._errorMessage != "") throw new Exception(_errorMessage);
            }
            b = b.Remove(b.Length - 1);
            b = b.Remove(0, 1);

            int maxLength = a.Length;
            if (maxLength > b.Length)
            {
                maxLength = b.Length;
            }
            //if (b.Contains("\""))
            //{
            //    //indexOpen = b.IndexOf("\"");
            //    //indexClose = b.IndexOf("\"");
            //    a = "\"" + a + "\"";
            //    maxLength--;
            //}
            //else if (b.Contains("\'"))
            //{
            //    //indexOpen = b.IndexOf("\'");
            //    //indexClose = b.IndexOf("\'");
            //    a = "\'" + a + "\'";
            //    maxLength--;
            //}
            //if(indexOpen < 0 || indexClose < 0)
            //{
            //    return false;
            //}

            string STRING_BIGGER = "string_bigger";
            string STRING_SMALLER = "string_smaller";
            string STRING_EQUAL = "string_equal";
            string result = "";
            bool? isFirstBiggerSecond = null;
            for (int i = 0; i < maxLength; i++)
            {
                if (a[i] > b[i])
                {
                    isFirstBiggerSecond = true;
                    break;
                }
                if (a[i] < b[i])
                {
                    isFirstBiggerSecond = false;
                    break;
                }
            }
            if (isFirstBiggerSecond == true)
            {
                result = STRING_BIGGER;
            }
            else if (isFirstBiggerSecond == false)
            {
                result = STRING_SMALLER;
            }
            else if (a.Length > b.Length)
            {
                result = STRING_BIGGER;
            }
            else if (a.Length < b.Length)
            {
                result = STRING_SMALLER;
            }
            else
            {
                result = STRING_EQUAL;
            }

            switch (opr)
            {
                case "=":  return (a.CompareTo(b) == 0);
                case "!=": return (a.CompareTo(b) != 0);
                case "like":
                    return new Regex(@"\A" + new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\").Replace(b, ch => @"\" + ch).Replace('_', '.').Replace("%", ".*") + @"\z", RegexOptions.Singleline).IsMatch(a);
                case "not like":
                    Regex regx = new Regex(@"\A" + new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\").Replace(b, ch => @"\" + ch).Replace('_', '.').Replace("%", ".*") + @"\z", RegexOptions.Singleline);
                    if (!regx.IsMatch(a))
                    {
                        return true;
                    }
                    break;
                case ">":
                    if (result == STRING_BIGGER)
                    {
                        return true;
                    }
                    return false;
                case "<":
                    if (result == STRING_SMALLER)
                    {
                        return true;
                    }
                    return false;
                case ">=":
                    if (result == STRING_SMALLER)
                    {
                        return false;
                    }
                    return true;
                case "<=":
                    if (result == STRING_BIGGER)
                    {
                        return false;
                    }
                    return true;
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

        private Boolean ListCompare(Object value, Object input, String opr, string type)
        {
            String stringInput = input.ToString();
            if (stringInput.Contains("("))
            {
                int indexFirst = stringInput.IndexOf("(");
                stringInput = stringInput.Substring(indexFirst + 1, stringInput.Length - indexFirst - 1);
            }

            if (stringInput.Contains(")"))
            {
                int indexLast = stringInput.IndexOf(")");
                stringInput = stringInput.Substring(0, indexLast);
            }

            String[] listInput;
            //if (stringInput.Contains(","))
                listInput = stringInput.Split(',');
            //else
            //listInput = stringInput.
            //else
            //    listInput = stringInput.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
            int count = 0;
            if (type == "String" || type == "DateTime" || type == "UserDefined" || type == "Binary")
            {
                String stringValue = value.ToString();
                for (int i = 0; i < listInput.Length; i++)
                {
                    listInput[i] = listInput[i].Trim();// Prevent user query with spaces
                    if (opr == "in")
                    {
                        if(type == "DateTime")
                        {
                            if (DateTimeCompare(stringValue, listInput[i], "="))
                                return true;
                        }
                        else
                        {
                            if (StringCompare(stringValue, listInput[i], "="))
                                return true;
                        }
                    }
                    else if (opr == "not in")
                    {
                        if (type == "DateTime")
                        {
                            if (DateTimeCompare(stringValue, listInput[i], "="))
                                return false;
                            else count++;
                        }
                        else
                        {
                            if (StringCompare(stringValue, listInput[i], "="))
                                return false;
                            else count++;
                        }
                    }

                }
            }

            if (type == "Int16" || type == "Int64" || type == "Int32" || type == "Byte" || type == "Currency")
            {
                int[] listInt = new int[listInput.Length];
                int intValue = Convert.ToInt32(value);
                for (int i = 0; i < listInt.Length; i++)
                {
                    listInt[i] = int.Parse(listInput[i].Trim());
                    if (opr == "in")
                    {
                        if (IntCompare(intValue, listInt[i], "="))
                                return true;
                   }
                   else if (opr == "not in")
                    {
                        if (IntCompare(intValue, listInt[i], "="))
                            return false;
                        else count++;
                    }
                }
            }

            if (type == "Decimal" || type == "Single" || type == "Double")
            {
                double[] listDouble = new double[listInput.Length];
                double doubleValue = Convert.ToDouble(value);
                for (int i = 0; i < listDouble.Length; i++)
                {
                    listDouble[i] = double.Parse(listInput[i].Trim());
                    if (opr == "in")
                    {
                        if (DoubleCompare(doubleValue, listDouble[i], "="))
                            return true;
                    }
                    else if (opr == "not in")
                    {
                        if (DoubleCompare(doubleValue, listDouble[i], "="))
                            return false;
                        else count++;
                    }
                }
            }
            if (count > 0) return true;
            return false;
        }


        //---edit
        public bool IsNumber(string pText)
        {
            Regex regex = new Regex(@"^[-+]?[0-9]*\.?[0-9]+$");
            return regex.IsMatch(pText);
        }
        //---edit
        public bool DateTimeCompare(string val, string input, string opr)
        {
            if ((input[0] == '\'' && input[input.Length - 1] == '\'') || (input[0] == '\"' && input[input.Length - 1] == '\"'))
            {
                input = input.Remove(input.Length - 1);
                input = input.Remove(0, 1);
                DateTime inpDtime;
                if (DateTime.TryParse(input, out inpDtime) == false)
                {
                    throw new Exception("Invalid DateTime format: MM/dd/yyyy hh:mm:ss");
                }
                DateTime valDtime = DateTime.Parse(val);
                switch (opr)
                {
                    case ">":
                        return valDtime > inpDtime;
                    case "<":
                        return valDtime < inpDtime;
                    case ">=":
                        return valDtime >= inpDtime;
                    case "<=":
                        return valDtime <= inpDtime;
                    case "=":
                        return valDtime == inpDtime;

                }
                return false;
            }
            throw new Exception("Missing quote");
            }
        //-----
        #region Fuzzy Set
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
                case "Currency":
                    if(input.Contains(",") || (input.Contains("(") && input.Contains(")")))
                    {
                        return ListCompare(value, input, opr, type);
                    }
                    else return IntCompare(Convert.ToInt32(value), Convert.ToInt32(input), opr);
                case "DateTime":
                    if (input.Contains(",") || (input.Contains("(") && input.Contains(")")))//do not check quote because it is checked before in checkSyntax()
                        return ListCompare(value.ToString().ToLower(), input, opr, type);
                    else
                    {
                        return DateTimeCompare(value.ToString(), input, opr);
                    }
                        
                case "String":
                case "UserDefined":
                case "Binary":
                    if (input.Contains(",") || (input.Contains("(") && input.Contains(")")))//do not check quote because it is checked before in checkSyntax()
                    {
                        return ListCompare(value.ToString().ToLower(), input, opr, type);
                    }
                    else
                    {     
                        return StringCompare(value.ToString().ToLower(), input.ToLower(), opr);
                    }
                case "Decimal":
                case "Single":
                case "Double":
                    if (input.Contains(",") || (input.Contains("(") && input.Contains(")")))
                    {
                        return ListCompare(value, input, opr, type);
                    }
                    else return DoubleCompare(Convert.ToDouble(value), Convert.ToDouble(input), opr);
                case "Boolean": return BoolCompare(Convert.ToBoolean(value), Convert.ToBoolean(input), opr);
            }

            return false;
        }

        //private Double FuzzyCompare(Double value, ContinuousFuzzySetBLL set, String opr)
        //{
        //    Double result = 0;
        //    Double membership = set.GetMembershipAt(value);

        //    switch (opr)
        //    {
        //        case "→":
        //            if (value >= set.Bottom_Left && value <= set.Bottom_Right)
        //                result = membership;
        //            return result;
        //        case "<"://
        //            if (value < set.Bottom_Left)
        //                result = 1;
        //            return result;

        //        case ">":
        //            if (value > set.Bottom_Right)
        //                result = 1;
        //            return result;

        //        case "<=":
        //            if (value < set.Bottom_Right)
        //                result = 1;//select 
        //            if (value >= set.Bottom_Left && value <= set.Bottom_Right)
        //                result = membership;
        //            return result;

        //        case ">=":
        //            if (value > set.Bottom_Left)
        //                result = 1;//select 
        //            if (value >= set.Bottom_Left && value <= set.Bottom_Right)
        //                result = membership;
        //            return result;

        //        case "=":
        //            if (value >= set.Bottom_Left && value <= set.Bottom_Right)
        //                result = membership;
        //            return result;

        //        case "!="://No need to get the membership
        //            if (value <= set.Bottom_Left || value >= set.Bottom_Right)
        //                result = 1;//selet the tuple
        //            return result;
        //    }

        //    return result;
        //}
        private Double FuzzyCompare(Double value, ConFS set, String opr)
        {
            Double result = 0;
            Double membership = set.GetMembershipAt(value);

            switch (opr)
            {
                case "→":
                    if (value >= set.Bottom_Left && value <= set.Bottom_Right)
                        result = membership;
                    return result;
                case "<"://
                    if (value < set.Bottom_Left)
                        result = 1;
                    return result;

                case ">":
                    if (value > set.Bottom_Right)
                        result = 1;
                    return result;

                case "<=":
                    if (value < set.Bottom_Right)
                        result = 1;//select 
                    if (value >= set.Bottom_Left && value <= set.Bottom_Right)
                        result = membership;
                    return result;

                case ">=":
                    if (value > set.Bottom_Left)
                        result = 1;//select 
                    if (value >= set.Bottom_Left && value <= set.Bottom_Right)
                        result = membership;
                    return result;

                case "=":
                    if (value >= set.Bottom_Left && value <= set.Bottom_Right)
                        result = membership;
                    return result;

                case "!="://No need to get the membership
                    if (value <= set.Bottom_Left || value >= set.Bottom_Right)
                        result = 1;//selet the tuple
                    return result;
            }

            return result;
        }

        //private Double FuzzyCompare(Double value, DiscreteFuzzySetBLL set, String opr)
        //{
        //    Double result = 0;
        //    Double max = set.GetMaxValue();
        //    Double min = set.GetMinValue();
        //    Double membership = set.GetMembershipAt(value);
        //    Boolean isMember = set.IsMember(value);

        //    switch (opr)
        //    {
        //        case "→":
        //            if (isMember)
        //                result = membership;
        //            return result;
        //        case "<"://
        //            if ( value < min)
        //                result = 1;
        //            return result;

        //        case ">":
        //            if (value > max)
        //                result = 1;
        //            return result;

        //        case "<=":

        //            if (value < min)
        //                result = 1;
        //            if (isMember)
        //                result = membership;
        //            return result;

        //        case ">=":
        //            if (value > max)
        //                result = 1;//select 
        //            if (isMember)
        //                result = membership;
        //            return result;

        //        case "=":
        //            if (isMember)
        //                result = membership;
        //            return result;

        //        case "!="://No need to get the membership
        //            if (!isMember)
        //                result = 1;
        //            return result;
        //    }

        //    return result;
        //}
        private Double FuzzyCompare(Double value, DisFS set, String opr)
        {
            Double result = 0;
            Double max = set.GetMaxValue();
            Double min = set.GetMinValue();
            Double membership = set.GetMembershipAt(value);
            Boolean isMember = set.IsMember(value);

            switch (opr)
            {
                case "→":
                    if (isMember)
                        result = membership;
                    return result;
                case "<"://
                    if (value < min)
                        result = 1;
                    return result;

                case ">":
                    if (value > max)
                        result = 1;
                    return result;

                case "<=":

                    if (value < min)
                        result = 1;
                    if (isMember)
                        result = membership;
                    return result;

                case ">=":
                    if (value > max)
                        result = 1;//select 
                    if (isMember)
                        result = membership;
                    return result;

                case "=":
                    if (isMember)
                        result = membership;
                    return result;

                case "!="://No need to get the membership
                    if (!isMember)
                        result = 1;
                    return result;
            }

            return result;
        } 
        #endregion
        #endregion
    }
}
