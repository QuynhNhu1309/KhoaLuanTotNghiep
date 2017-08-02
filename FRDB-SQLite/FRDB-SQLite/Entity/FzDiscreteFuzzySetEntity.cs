using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FRDB_SQLite
{
    public class FzDiscreteFuzzySetEntity  //This content a schema of database
    {
        #region 1. Fields

        private String _name;
        private String _v;
        private String _m;
        private List<Double> _valueSet;
        private List<Double> _membershipSet;
        #endregion
        #region 2. Properties

        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public String V
        {
            get { return _v; }
            set { _v = value; }
        }
        public String M
        {
            get { return _m; }
            set { _m = value; }
        }
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

        public FzDiscreteFuzzySetEntity()
        {
            this._name = String.Empty;
            this._v = String.Empty;
            this._m = String.Empty;
            this._valueSet = new List<double>();
            this._membershipSet = new List<double>();
        }

        public FzDiscreteFuzzySetEntity(String name, String v, String m, List<Double> valueSet, List<Double> membershipSet)
        {
            this._name = name;
            this._v = v;
            this._m = m;
            this._membershipSet = membershipSet;
            this._valueSet = valueSet;
        }

        public FzDiscreteFuzzySetEntity(String name)
        {
            this._name = name;
            this._v = String.Empty;
            this._m = String.Empty;
            this._valueSet = new List<double>();
            this._membershipSet = new List<double>();
        }

        #endregion

        #region 4. Methods



        #endregion

        #region 5. Privates



        #endregion
    }
}
