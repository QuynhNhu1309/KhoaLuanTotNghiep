using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FRDB_SQLite;
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
    //--
        //private Double _uRelation;
        //private List<String> _memberships;

        private FzTupleEntity _resultTuple;
        private List<FzRelationEntity> _selectedRelations;
        private List<Item> _itemConditions;
        private String _errorMessage;
        private FdbEntity _fdbEntity;
        private List<Double> random_array = new List<Double>();
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
                    Item item = new Item() { elements = items.elements, nextLogic = items.nextLogic };
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
            //_uRelation = Convert.ToDouble(tuple.ValuesOnPerRow[tuple.ValuesOnPerRow.Count - 1]);
            _uRelation = (tuple.ValuesOnPerRow[tuple.ValuesOnPerRow.Count - 1]).ToString();
            List<string> _memberships = new List<string>();
            //String logicText = String.Empty;

            for (int i = 0; i < list.Count; i++)
            {
                String membership = string.Empty;
                if (list[i].elements.Count == 3)// single expression: age='young'.
                {
                   membership= SatisfyItem(list[i].elements, tuple, i - 1);
                    //    logicText += "1" + list[i].nextLogic;
                    //else
                    //    logicText += "0" + list[i].nextLogic;
                }
                else// Multiple expression: weight='heavy' and height>165
                {
                   List<Item> subItems = CreateSubItems(list[i].elements);
                    membership= Satisfy(subItems, tuple);
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
                if (i != 0 && _memberships.Count > 0)// Getting previous logicality
                    _memberships.Add(list[i-1].nextLogic);
                _memberships.Add(membership.ToString());
            }
            string NewMembership = UpdateMembership(_memberships);
            _resultTuple.ValuesOnPerRow[_resultTuple.ValuesOnPerRow.Count - 1] = NewMembership;
            return NewMembership;
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
                    }
                    if (dis1 != null && dis2 == null)
                    {
                        v2 = Convert.ToDouble(memberships[2]);
                        dis2 = new DisFS();
                        dis2.ValueSet.Add(v2);
                        dis2.MembershipSet.Add(1);
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
                else if (!IsNumber(_uRelation))
                    return "FN not exists";

            }
            if (conFS_uRelation!= null || disFS_uRelation != null)
                check_uRelation = 1; //_uRelation is a fuzzyset
            if (itemCondition[1] == "->" || itemCondition[1] == "→")
            {
                //fs = fs.Substring(1, fs.Length - 2);
                //conFS = new FuzzyProcess().ReadEachConFS(path + fs + ".conFS");
                //disFS = new FuzzyProcess().ReadEachDisFS(path + fs + ".disFS");//2 is value of user input
                conFS = GetConFS(path, fs);
                disFS = GetDisFS(path, fs);
            }
            if (conFS != null)//continuous fuzzy set is priorer than discrete fuzzy set
            {
                //itemCondition[1] is operator, uValue is the membership of the value on current cell for the selected fuzzy set
                Double uValue = FuzzyCompare(Convert.ToDouble(value), conFS, itemCondition[1]);
                if (check_uRelation == 0) //if _uRelation is a number
                {
                    uValue = Math.Min(uValue, Convert.ToDouble(_uRelation));//Update the min value
                    result= uValue.ToString();
                        //if (i != -1 && _memberships.Count > 0)// Getting previous logicality
                        //    _memberships.Add(_itemConditions[i].nextLogic);
                        //_memberships.Add(uValue.ToString());
                }
                if (check_uRelation == 1) //if _uRelation is fuzzyset 
                {
                    DisFS uDisFS = new DisFS();
                    uDisFS.ValueSet.Add(uValue);
                    uDisFS.MembershipSet.Add(1);
                    string FSName = Min_DisFS(disFS_uRelation, uDisFS);
                    result= FSName;
                        //if (i != -1 && _memberships.Count > 0)// Getting previous logicality
                        //    _memberships.Add(_itemConditions[i].nextLogic);
                        //_memberships.Add(FSName);
                }
            }
            if (disFS != null && conFS == null)
            {
                Double uValue = FuzzyCompare(Convert.ToDouble(value), disFS, itemCondition[1]);
                if (check_uRelation == 0) //if _uRelation is a number
                {
                    uValue = Math.Min(uValue, Convert.ToDouble(_uRelation));//Update the min value
                    result= uValue.ToString();
                        //if (i != -1 && _memberships.Count > 0)// Getting previous logicality
                        //    _memberships.Add(_itemConditions[i].nextLogic);
                        //_memberships.Add(uValue.ToString());
                }
                if (check_uRelation == 1) //if _uRelation is fuzzyset //hỏi lại
                {
                    DisFS uDisFS = new DisFS();
                    uDisFS.ValueSet.Add(uValue);
                    uDisFS.MembershipSet.Add(1);
                    string FSName = Min_DisFS(disFS_uRelation, uDisFS);
                    result= FSName;
                        //if (i != -1 && _memberships.Count > 0)// Getting previous logicality
                        //    _memberships.Add(_itemConditions[i].nextLogic);
                        //_memberships.Add(FSName);
                }
            }
            if (disFS == null && conFS == null)
            {
                //if (fs.Contains("\""))
                //    fs = fs.Substring(1, fs.Length - 2);
                if (ObjectCompare(value, fs, itemCondition[1], dataType))
                {
                    if (check_uRelation==0)
                    {
                        result= tuple.ValuesOnPerRow[tuple.ValuesOnPerRow.Count - 1].ToString();
                        //if (i != -1 && _memberships.Count > 0)// Getting previous logicality
                        //    _memberships.Add(_itemConditions[i].nextLogic);
                        //_memberships.Add(tuple.ValuesOnPerRow[tuple.ValuesOnPerRow.Count - 1].ToString());
                    }
                    if (check_uRelation==1)
                    {
                        DisFS uDisFS = new DisFS();
                        uDisFS.ValueSet.Add(1);
                        uDisFS.MembershipSet.Add(1);
                        string FSName = Min_DisFS(disFS_uRelation, uDisFS);
                        result= FSName;
                        //if (i != -1 && _memberships.Count > 0)// Getting previous logicality
                        //    _memberships.Add(_itemConditions[i].nextLogic);
                        //_memberships.Add(FSName);

                    }
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

        private DisFS transContoDis (ConFS con)
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
        private string Diff_DisFS (DisFS FuzzySet1, DisFS FuzzySet2) //FuzzySet1 - FuzzySet2
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
        private string Min_DisFS(DisFS FuzzySet1, DisFS FuzzySet2)
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
                if ((FuzzySet1.ValueSet.Count == 1 && FuzzySet1.ValueSet[0] == 1))
                {
                    result.Add(FuzzySet2.V);
                    result.Add(FuzzySet2.M);
                }
                if ((FuzzySet2.ValueSet.Count == 1 && FuzzySet2.ValueSet[0] == 1))
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
        private string Max_DisFS(DisFS FuzzySet1, DisFS FuzzySet2)
        {
                try
                {
                    String Name = "appox_" + Random();
                    List<String> result = new List<String>();
                    String path = Directory.GetCurrentDirectory() + @"\lib\temp\";
                    String values = "";
                    String memberships = "";
                    DisFS dis = new DisFS();
                    if ((FuzzySet1.ValueSet.Count == 1 && FuzzySet1.ValueSet[0] == 1) || (FuzzySet2.ValueSet.Count == 1 && FuzzySet2.ValueSet[0] == 1))
                    {
                        return "1";
                    }
                    if ((FuzzySet1.ValueSet.Count == 1 && FuzzySet1.ValueSet[0] == 0))
                    {
                        result.Add(FuzzySet2.V);
                        result.Add(FuzzySet2.M);
                    }
                    if ((FuzzySet2.ValueSet.Count == 1 && FuzzySet2.ValueSet[0] == 0))
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

       

        private String Random()
        {
            Random rd = new Random();
            String result="";
            Boolean flag = false;
            while (!flag)
            {
                result=Math.Round(rd.NextDouble(), 2).ToString();
                if (random_array.Count  != 0)
                {
                    foreach (var r in random_array)
                    {
                        if (r == Double.Parse(result))
                            break;
                        flag = true;
                    }
                }
                else
                {
                    flag = true;
                }
            }
            Double re = Double.Parse(result);
            random_array.Add(re);
            return result;
        }
        private Boolean StringCompare(String a, String b, String opr)
        {
            //a = "\"" + a + "\"";
            int indexOpen = 0, indexClose = 0;
            if(b.Contains("\""))
            {
                //indexOpen = b.IndexOf("\"");
                //indexClose = b.IndexOf("\"");
                a = "\"" + a + "\"";
            }
            else if (b.Contains("\'"))
            {
                //indexOpen = b.IndexOf("\'");
                //indexClose = b.IndexOf("\'");
                a = "\'" + a + "\'";
            }
            //if(indexOpen < 0 || indexClose < 0)
            //{
            //    return false;
            //}

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
                    }break;
    
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
            if (stringInput.Contains(","))
                listInput = stringInput.Split(',');
            else
                listInput = stringInput.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
            int count = 0;
            if (type == "String" || type == "DateTime" || type == "UserDefined" || type == "Binary")
            {
                String stringValue = value.ToString();
                for (int i = 0; i < listInput.Length; i++)
                {
                    listInput[i] = listInput[i].Trim();// Prevent user query with spaces
                    if (opr == "in")
                    {
                        if (StringCompare(stringValue, listInput[i], "="))
                        {
                            return true;
                        }
                    }
                    else if (opr == "not in")
                    {
                        if (StringCompare(stringValue, listInput[i], "="))
                            return false;
                        else count++;
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
                case "String":
                case "DateTime":
                case "UserDefined":
                case "Binary":
                    if (input.Contains(",") || (input.Contains("(") && input.Contains(")")))
                    {
                        return ListCompare(value.ToString().ToLower(), input, opr, type);
                    }
                    else return StringCompare(value.ToString().ToLower(), input.ToLower(), opr);
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
