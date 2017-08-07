using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using FRDB_SQLite;

namespace FRDB_SQLite
{
    public class DiscreteFuzzySetBLL: FuzzySetBLL
    {

        #region 1. Fields
        //Note: value set and membership set is two set which have the same elements
        private List<Double> _valueSet = new List<Double>();
        private List<Double> _membershipSet = new List<Double>();
        #endregion

        #region 2. Properties
        public List<Double> ValueSet
        {
            get { return _valueSet; }
            set { _valueSet = value; }
        }
        public List<Double> MembershipSet
        {
            get { return _membershipSet; }
            set { _membershipSet = value; }
        }
        #endregion

        #region 3. Contructors
        public DiscreteFuzzySetBLL()
            : base()
        {
            this._valueSet = new List<Double>();
            this._membershipSet = new List<Double>();
        }

        public DiscreteFuzzySetBLL(String fuzzySetName, Hashtable fuzzySet, List<Double> valueSet, List<Double> membershipSet)
            : base(fuzzySetName, fuzzySet)
        {
            this._valueSet = valueSet;
            this._membershipSet = membershipSet;
        }
        #endregion

        #region 4. Methods
        public static FzDiscreteFuzzySetEntity GetDisFNByName(String disFNName, FdbEntity fdb)
        {
            return DiscreteFuzzySetDAL.GetDisFNByName(disFNName, fdb);
        }
        public static List<String> ListOfDiscreteFNName(FdbEntity fdb)
        {
            return DiscreteFuzzySetDAL.ListOfDiscreteFNName(fdb);
        }

        public override void AddPoint(Double value, Double membership)
        {
            base.AddPoint(value, membership);
            this._valueSet.Add(value);//Add each value to the set of value
            this._membershipSet.Add(membership);//add each membership of value to the set of membership
        }

        public Double GetMembershipAt(Double value)
        {
            Double result = 0;
            int i = 0;

            foreach (var item in this._valueSet)
            {
                if (item == value)
                {
                    result = _membershipSet[i];
                    break;
                }
                else
                {
                    i++;
                }
            }

            return result;
        }

        public Boolean IsMember(Double value)
        {
            foreach (Double item in this.ValueSet)
            {
                if (value == item)
                {
                    return true;
                }
            }

            return false;
        }

        public Double GetMinValue()
        {
            Double result = Double.MaxValue;

            for (int i = 0; i < this.ValueSet.Count; i++)
            {
                if (ValueSet[i] <= result)
                    result = ValueSet[i];
            }

            return result;
        }

        public Double GetMaxValue()
        {
            Double result = Double.MinValue;

            for (int i = 0; i < this.ValueSet.Count; i++)
            {
                if (ValueSet[i] >= result)
                    result = ValueSet[i];
            }

            return result;
        }

        public List<DiscreteFuzzySetBLL> GetAll()
        {
            return new DiscreteFuzzySetDAL().GetAllDiscreteFuzzySet();
        }

        public DiscreteFuzzySetBLL GetByName(String name)
        {
            return new DiscreteFuzzySetDAL().GetDiscreteFuzzySetByName(name);
        }

        public int Update()
        {
            return new DiscreteFuzzySetDAL().UpdateDiscreteFS(this);
        }

        public int Delete()
        {
            return new DiscreteFuzzySetDAL().UpdateDiscreteFS(this);
        }

        public int Delete(List<DiscreteFuzzySetBLL> list)
        { 
            return new DiscreteFuzzySetDAL().DeleteList(list);
        }

        #endregion

        #region 5. Privates
        #endregion
    }
}
