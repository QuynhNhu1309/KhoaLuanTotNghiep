using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Data.SQLite;
//using BusinessLayer.FuzzySetLibrary;

namespace FRDB_SQLite
{
    public class FuzzySetBLL
    {
        #region 1. Fields

        private String _fuzzySetName;
        private Hashtable _fuzzySet;

        #endregion

        #region 2. Properties

        public String FuzzySetName
        {
            get { return _fuzzySetName; }
            set { _fuzzySetName = value; }
        }

        public Hashtable FuzzySet
        {
            get { return _fuzzySet; }
            set { _fuzzySet = value; }
        }
        
        #endregion

        #region 3. Contructors

        public FuzzySetBLL()
        {
            this._fuzzySetName = String.Empty;
            this._fuzzySet = new Hashtable();
        }

        public FuzzySetBLL(Hashtable valueSet)
        {
            this._fuzzySet = valueSet;
            this._fuzzySetName = String.Empty;
        }

        public FuzzySetBLL(String fuzzySetName, Hashtable valueSet)
            :this(valueSet)
        {
            this._fuzzySetName = fuzzySetName;
        }



        #endregion

        #region 4. Methods

        public virtual void AddPoint(Double value, Double membership)
        {
            if (membership >= 0 && membership <= 1)
            {
                this._fuzzySet.Add(value, membership);
            }
            else
            {
                throw new Exception("ERROR:\n The memberships must be in [0, 1]");
                //return;
            }
        }

        #endregion

        #region 5. Privates
        
        
        #endregion
    }
}
