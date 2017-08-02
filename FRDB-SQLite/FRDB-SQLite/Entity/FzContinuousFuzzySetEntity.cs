using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FRDB_SQLite
{
   public class FzContinuousFuzzySetEntity
    {
        #region 1. Fields

        private String _name;
        private Double _bottom_left;
        private Double _top_left;
        private Double _top_right;
        private Double _bottom_right;

        #endregion
        #region 2. Properties

        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public Double Bottom_Left
        {
            get { return _bottom_left; }
            set { _bottom_left = value; }
        }
        public Double Top_Left
        {
            get { return _top_left; }
            set { _top_left = value; }
        }
        public Double Top_Right
        {
            get { return _top_right; }
            set { _top_right = value; }
        }
        public Double Bottom_Right
        {
            get { return _bottom_right; }
            set { _bottom_right = value; }
        }

        #endregion

        #region 3. Contructors

        public FzContinuousFuzzySetEntity()
        {
            this._bottom_right = new Double();
            this._top_right = new Double();
            this._name = string.Empty;
            this._top_left = new Double();
            this._bottom_left = new Double(); 

        }
        public FzContinuousFuzzySetEntity(String name, Double bottom_left,Double top_left, Double top_right, Double bottom_right)
        {
            this._name = name;
            this._bottom_left = bottom_left;
            this._top_left = top_left;
            this._top_right = top_right;
            this._bottom_right = bottom_right;
        }

        public FzContinuousFuzzySetEntity(String name)
        {
            this._name = name;
            this._bottom_left = new Double();
            this._top_left = new Double();
            this._bottom_right = new Double();
            this._top_right = new Double();
        }

        #endregion

        #region 4. Methods


        #endregion

        #region 5. Privates



        #endregion
    }
}
