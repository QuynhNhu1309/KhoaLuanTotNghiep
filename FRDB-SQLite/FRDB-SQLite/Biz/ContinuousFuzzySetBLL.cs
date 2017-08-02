using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FRDB_SQLite;

namespace FRDB_SQLite
{
    public class ContinuousFuzzySetBLL : FuzzySetBLL
    {
        #region 1. Fields

        private Double _bottom_left;
        private Double _top_left;
        private Double _top_right;
        private Double _bottom_right;

        #endregion

        #region 2. Properties

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

        public ContinuousFuzzySetBLL()
            : base()
        {
            this.FuzzySetName = "";
            this.FuzzySet = new System.Collections.Hashtable();
            this._bottom_left = 0.0;
            this._bottom_right = 0.0;
            this._top_left = 0.0;
            this._top_right = 0.0;
        }

        #endregion

        #region 4. Methods
        public static FzContinuousFuzzySetEntity GetConFNByName(String conFNName, FdbEntity fdb)
        {
            return ContinuousFuzzySetDAL.GetConFNByName(conFNName, fdb);
        }
        public static List<String> ListOfContinuousFNName(FdbEntity fdb)
        {
            return ContinuousFuzzySetDAL.ListOfContinuousFNName(fdb);
        }
        public bool SetValue(Double bottom_left, Double top_left, Double top_right, Double bottom_right)
        {
            if (!CheckLegal(bottom_left, top_left, top_right, bottom_right))
            {
                return false;
            }
            else
            {
                this._bottom_left = bottom_left;
                this._top_right = top_right;
                this._top_left = top_left;
                this._bottom_right = bottom_right;

                return true;
            }
        }

        public Double GetMembershipAt(Double value)//value is a point on Ox
        {
            Double result = -1;

            //Validate the value is between two point which limit trapezoidal on Ox.
            if ((value >= this._bottom_left && value <= this._top_left) || (value >= this._top_left && value <= this._bottom_left) ||
                (value >= this._top_right && value <= this._bottom_right) || (value >= this._bottom_right && value <= this._top_right) ||
                value >= this._top_left && value <= this._top_right)
            {
                if (value == this._bottom_left || value == this._bottom_right)
                {
                    result = 0;
                }
                else if (value >= this._top_left && value <= this._top_right)
                {
                    result = 1;
                }
                else
                {
                    //A (__bottom_left, 0)
                    //B (__top_left, 1)
                    //C (_top_right, 1)
                    //D (_bottom_right, 0)

                    Double OxAB = Math.Abs(this._top_left - this._bottom_left);
                    Double OxCD = Math.Abs(this._bottom_right - this._top_right);
                    Double OxValueLeft = Math.Abs(value - this._bottom_left);
                    Double OxValueRight = Math.Abs(value - this._bottom_right);

                    //The value is on the left side of the trapezoidal between _top_right and _bottom_right
                    if ((value > this._bottom_left && value < this._top_left) || (value > this._top_right && value < this._bottom_right))
                    {
                        //Base on congruent triangles
                        result = 1 * OxValueLeft / OxAB;
                    }

                    //The value is on the right side of the trapezoidal between __bottom_left and __top_left
                    if ((value > this._top_left && value < this._bottom_right) || (value > this._bottom_right && value < this._top_right))
                    {
                        //Base on congruent triangles
                        result = 1 * OxValueRight / OxCD;
                    }
                }
            }

            return Math.Round(result, 4);
        }

        public DiscreteFuzzySetBLL ConvertToDiscrete(Double startPoint, Double epsilon)
        {
            return new ContinuousFuzzySetDAL().Discretize(this, startPoint, epsilon);
        }

        public List<ContinuousFuzzySetBLL> GetAll()
        {
            return new ContinuousFuzzySetDAL().GetAllConFS();
        }

        public ContinuousFuzzySetBLL GetByName(String name)
        {
            return new ContinuousFuzzySetDAL().GetConFsByName(name);
        }

        public int Update()
        {
            return new ContinuousFuzzySetDAL().UpdateConFs(this);
        }

        public int Delete()
        {
            return new ContinuousFuzzySetDAL().DeleteConFs(this);
        }

        public int Delete(List<ContinuousFuzzySetBLL> list)
        {
            return new ContinuousFuzzySetDAL().DeleteList(list);
        }

        #endregion

        #region 5. Privates
        private bool CheckLegal(Double bl, Double tl, Double tr, Double br)
        {
            if (bl < 0 || bl > br)
                return false;

            if (tl < 0 || tl > tr)
                return false;

            if (tr < 0 || tr < tl)
                return false;

            if (br < 0 || br < bl)
                return false;

            return true;
        }
        #endregion
    }
}
