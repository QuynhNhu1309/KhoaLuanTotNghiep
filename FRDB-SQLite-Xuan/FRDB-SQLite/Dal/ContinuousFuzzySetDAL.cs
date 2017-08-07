using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using FRDB_SQLite;



namespace FRDB_SQLite
{   
    public class ContinuousFuzzySetDAL
    {
        #region 1. Fields

        private  FuzzySetLibraryEntities db = new FuzzySetLibraryEntities();

        #endregion

        #region 2. Properties (none)
        #endregion

        #region 3. Contructors (none)
        #endregion

        #region 4. Methods
        public static FzContinuousFuzzySetEntity GetConFNByName(String conFNName, FdbEntity fdb)
        {
            foreach (FzContinuousFuzzySetEntity c in fdb.ContinuousFuzzyNumbers)
            {
                if (c.Name == conFNName)
                {
                    return c;
                }
            }

            return null;
        }
        public static List<String> ListOfContinuousFNName(FdbEntity fdb)
        {
            List<String> conFNNames = new List<String>();
            foreach (FzContinuousFuzzySetEntity con in fdb.ContinuousFuzzyNumbers)
            {
                conFNNames.Add(con.Name);
            }

            return conFNNames;
        }

        public DiscreteFuzzySetBLL Discretize(ContinuousFuzzySetBLL ConFS, double StartPoint, double epsilon)
        {
            DiscreteFuzzySetBLL DiscFS = new DiscreteFuzzySetBLL();
            DiscFS.FuzzySetName = ConFS.FuzzySetName;
            DiscFS.FuzzySet = ConFS.FuzzySet;

            for (double value = StartPoint; value <= ConFS.Bottom_Right; value += epsilon)
            {
                double membership = ConFS.GetMembershipAt(value);
                DiscFS.AddPoint(value, membership);
            }

            return DiscFS;
        }

        public List<ContinuousFuzzySetBLL> GetAllConFS()
        {
            try
            {
                var con = from c in db.ContinuousLibraries
                          select c;

                List<ContinuousFuzzySetBLL> result = new List<ContinuousFuzzySetBLL>();
                
                foreach (var item in con.ToList())
                {
                    ContinuousFuzzySetBLL conFs = new ContinuousFuzzySetBLL();
                    conFs.FuzzySetName = item.LanguisticLabel;
                    conFs.Bottom_Left = item.BottomLeft;
                    conFs.Top_Left = item.TopLeft;
                    conFs.Bottom_Right = item.BottomRight;
                    conFs.Top_Right = item.TopRight;

                    result.Add(conFs);
                }

                return result;
            }
            catch 
            {
                //throw new Exception("ERROR:\n" + ex.Message);
                return null;
            }
        }

        public  ContinuousFuzzySetBLL GetConFsByName(String name)
        {
            try
            {
                var con = db.ContinuousLibraries.SingleOrDefault(id => id.LanguisticLabel == name);
                ContinuousFuzzySetBLL conFs = new ContinuousFuzzySetBLL();

                if (con != null)
                {
                    conFs.FuzzySetName = con.LanguisticLabel;
                    conFs.Bottom_Left = con.BottomLeft;
                    conFs.Top_Left = con.TopLeft;
                    conFs.Bottom_Right = con.BottomRight;
                    conFs.Top_Right = con.TopRight;
                }
                else
                    conFs = null;

                return conFs;
            }
            catch (SQLiteException ex)
            {
                //throw new Exception("ERROR:\n" + ex.Message);
                return null;
            }
        }

        public  int UpdateConFs(ContinuousFuzzySetBLL conFs)
        {
            try
            {
                int result = 0;

                if (!IsExistFSName(conFs.FuzzySetName))//Add new object
                {
                    ContinuousLibrary child = new ContinuousLibrary();
                    child.LanguisticLabel = conFs.FuzzySetName;
                    child.BottomLeft = conFs.Bottom_Left;
                    child.TopLeft = conFs.Top_Left;
                    child.BottomRight = conFs.Bottom_Right;
                    child.TopRight = conFs.Top_Right;

                    db.AddToContinuousLibraries(child);
                    return result = db.SaveChanges(true);
                }
                else//Just update value
                {
                    var _conFs = db.ContinuousLibraries.FirstOrDefault(id => id.LanguisticLabel == conFs.FuzzySetName);
                    _conFs.LanguisticLabel = conFs.FuzzySetName;
                    _conFs.BottomLeft = conFs.Bottom_Left;
                    _conFs.TopLeft =conFs.Top_Left;
                    _conFs.BottomRight = conFs.Bottom_Right;
                    _conFs.TopRight = conFs.Top_Right;

                    return result = db.SaveChanges();
                }
            }
            catch (SQLiteException ex)
            {
                //throw new Exception(ex.Message);
                return -1;
            }
        }

        public  int DeleteConFs(ContinuousFuzzySetBLL conFs)
        {
            try
            {
                int result = 0;

                if (IsExistFSName(conFs.FuzzySetName))//Delete Object
                {
                    var _conFs = db.ContinuousLibraries.FirstOrDefault(id => id.LanguisticLabel == conFs.FuzzySetName);

                    db.DeleteObject(_conFs);
                    return result = db.SaveChanges();
                }
                else//Do nothing
                {
                    return result = -1;
                }
            }
            catch (SQLiteException ex)
            {
                //throw new Exception(ex.Message);
                return -1;
            }
        }

        public int DeleteList(List<ContinuousFuzzySetBLL> list)
        {
            try
            {
                int result = -1;
                int i = 0;

                foreach (var item in list)
                {
                    result = DeleteConFs(item);
                    i++;
                }

                if (i == list.Count) return 1;
                else return -1;
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #region 5. Privates

        private Boolean IsExistFSName(String name)//Also mean is exist an object in db
        {
            var _set = from s in db.ContinuousLibraries
                       where s.LanguisticLabel == name
                       select s;

            if (_set.Count() == 0)
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
