using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FRDB_SQLite
{
    public class FzTupleEntity
    {   
        #region 1. Fields
        private List<Object> _valuesOnPerRow;//List of values on a row of a tuple
        private int _memberShipIndex = 0;
        #endregion

        #region 2. Properties

        public List<Object> ValuesOnPerRow
        {
            get { return _valuesOnPerRow; }
            set {
                foreach (Object item in value)
                {
                    _valuesOnPerRow.Add(item);
                }
                this._memberShipIndex = this._valuesOnPerRow.Count - 1;
            }
        }

        public String MemberShip
        {
            get
            {
                return this._valuesOnPerRow[this._memberShipIndex].ToString();
            }
            set
            {
                this._valuesOnPerRow[this._memberShipIndex] = value;
            }
        }

        #endregion

        #region 3. Contructors

        public FzTupleEntity()
        {
            this._valuesOnPerRow = new List<object>();
        }

        public FzTupleEntity(List<string> valuesOnPerRow)
        {
            this._valuesOnPerRow = new List<object>();
            for (int i = 0; i < valuesOnPerRow.Count(); i++)
            {
                this._valuesOnPerRow.Add(valuesOnPerRow[i]);
            }
            this._memberShipIndex = this._valuesOnPerRow.Count - 1;
        }

        public FzTupleEntity(String valuesOnPerRow)///consist of values of column on the same row
        {
            this._valuesOnPerRow = new List<Object>();

            Char[] seperator = { ',' };
            String[] values = valuesOnPerRow.Split(seperator);

            for (int i = 0; i < values.Length; i++)
            {
                this._valuesOnPerRow.Add(values[i]);
            }
            this._memberShipIndex = this._valuesOnPerRow.Count - 1;
        }

        public FzTupleEntity(FzTupleEntity old)
        {
            this._valuesOnPerRow = new List<object>();
            foreach (Object item in old._valuesOnPerRow)
            {
                this.ValuesOnPerRow.Add(item);
            }
            this._memberShipIndex = this._valuesOnPerRow.Count - 1;
        }

        public FzTupleEntity(FzTupleEntity old, String newMemeberShip)
        {
            this._valuesOnPerRow = new List<object>();
            foreach (Object item in old._valuesOnPerRow)
            {
                this.ValuesOnPerRow.Add(item);
            }
            this._memberShipIndex = this._valuesOnPerRow.Count - 1;
            this.MemberShip = newMemeberShip;
        }


        #endregion

        #region 4. Methods (none)

        public void Add(object item)
        {
            this._valuesOnPerRow.Add(item);
            this._memberShipIndex = this._valuesOnPerRow.Count - 1;
        }
        
        public bool Equals(FzTupleEntity compareTuple)
        {
            bool isEqual = true;
            for (int i = 0; i <= this._valuesOnPerRow.Count - 2; i++)
            {
                if (!this._valuesOnPerRow[i].ToString().Equals(compareTuple.ValuesOnPerRow[i].ToString()))
                {
                    isEqual = false;
                    break;
                }
            }
            return isEqual;
        }

        #endregion

        #region 5. Privates (none)



        #endregion
    }
}
