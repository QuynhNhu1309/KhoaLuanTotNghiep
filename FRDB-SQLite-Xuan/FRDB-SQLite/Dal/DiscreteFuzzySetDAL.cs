using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;


using FRDB_SQLite;

namespace FRDB_SQLite
{
    public class DiscreteFuzzySetDAL
    {
        #region 1. Fields

        private  FuzzySetLibraryEntities db = new FuzzySetLibraryEntities();

        #endregion

        #region 2. Properties (none)
        #endregion

        #region 3. Contructors (none)
        #endregion

        #region 4. Methods
        public static FzDiscreteFuzzySetEntity GetDisFNByName(String disFNName, FdbEntity fdb)
        {
            foreach (FzDiscreteFuzzySetEntity d in fdb.DiscreteFuzzyNumbers)
            {
                if (d.Name == disFNName)
                {
                    return d;
                }
            }

            return null;
        }
        public static List<String> ListOfDiscreteFNName(FdbEntity fdb)
        {
            List<String> disFNNames = new List<String>();
            foreach (FzDiscreteFuzzySetEntity dis in fdb.DiscreteFuzzyNumbers)
            {
                disFNNames.Add(dis.Name);
            }

            return disFNNames;
        }
        public List<DiscreteFuzzySetBLL> GetAllDiscreteFuzzySet()
        {
            List<DiscreteFuzzySetBLL> resultList = new List<DiscreteFuzzySetBLL>();
            
            ///Get all discrete fuzzy set
            var disc = from d in db.DiscreteLibaries
                         select d;

            foreach (var item in disc.ToList())
            {
                DiscreteFuzzySetBLL result = new DiscreteFuzzySetBLL();
                //Get list values
                result.ValueSet = SplitString(item.Values.ToString());

                //Get list memberships
                result.MembershipSet = SplitString(item.Memberships.ToString());

                //Get Other values
                result.FuzzySetName = item.LanguisticLabel;

                //Some get FuzzySet Fomr referenced object
                
                //finally, add to list
                resultList.Add(result);
            }

            return resultList;
        }

        public  DiscreteFuzzySetBLL GetDiscreteFuzzySetByName(String name)
        {
            DiscreteFuzzySetBLL result = new DiscreteFuzzySetBLL();
            var disc = db.DiscreteLibaries.SingleOrDefault(id => id.LanguisticLabel == name);

            if (disc != null)
            {
                result.FuzzySetName = disc.LanguisticLabel;
                //Some get fuzzySet from referenced object
                result.ValueSet = SplitString(disc.Values);
                result.MembershipSet = SplitString(disc.Memberships);
            }
            else
                result = null;

            return result;
        }

        public int UpdateDiscreteFS(DiscreteFuzzySetBLL disc)
        {
            try
            {
                int result = 0;

                if (!IsExistFSName(disc.FuzzySetName))//Add new object
                {
                    //Insert mother library (contens both discrete library and continuous library)
                    //FuzzySetBLL mother = new FuzzySetBLL(disc.FuzzySetName, disc.FuzzySet);

                    //if (new FuzzySetDAL().UpdateFuzzySet(mother) == 1)
                    //{
                        //Insert child library
                        DiscreteLibary child = new DiscreteLibary();
                        child.LanguisticLabel = disc.FuzzySetName;
                        child.Values = ConvertToString(disc.ValueSet);
                        child.Memberships = ConvertToString(disc.MembershipSet);

                        db.AddToDiscreteLibaries(child);
                        return result = db.SaveChanges(true);
                    //}
                    //else
                    //{
                    //    return -1;
                    //}
                }
                else//Just update values and memberships
                {
                    var _disc = db.DiscreteLibaries.FirstOrDefault(id => id.LanguisticLabel == disc.FuzzySetName);
                    //_disc.LanguisticLabel = disc.FuzzySetName;
                    _disc.Values = ConvertToString(disc.ValueSet);
                    _disc.Memberships = ConvertToString(disc.MembershipSet);

                    return result = db.SaveChanges();
                }
            }
            catch (SQLiteException ex)
            {
                //throw new Exception(ex.Message);
                return -1;
            }
        }

        public int DeleteDiscreteFS(DiscreteFuzzySetBLL disc)
        { 
            try
            {
                int result = 0;

                if (IsExistFSName(disc.FuzzySetName))//Delete object
                {
                    var _disc = db.DiscreteLibaries.FirstOrDefault(id => id.LanguisticLabel == disc.FuzzySetName);

                    db.DeleteObject(_disc);
                    return result = db.SaveChanges();
                }
                else//Doing nothing
                {
                    return result = -1;
                }
            }
            catch
            {
                //throw new Exception(ex.Message);
                return -1;
            }
        }

        public int DeleteList(List<DiscreteFuzzySetBLL> list)
        {
            try
            {
                int result = -1;
                int i = 0;

                foreach (var item in list)
                {
                    result = DeleteDiscreteFS(item);
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

        private  Boolean IsExistFSName(String name)//Also mean is exist an object in db
        {
            var _set = from s in db.DiscreteLibaries
                       where s.LanguisticLabel == name
                       select s;

            if (_set.Count() == 0)
            {
                return false;
            }

            return true;
        }

        public String ConvertToString(List<Double> objs)
        {
            String result = "{";
            foreach (var item in objs)
            {
                result += item + ",";
            }

            result = result.Remove(result.Length - 1);
            result += "}";

            return result;
        }

        public List<Double> SplitString(String str)
        {
            List<Double> result = new List<double>();

            ///Remove "{", "}" and ","
            String tmp = str.Replace("{", "");
            tmp = tmp.Replace("}", "");
            Char[] seperator = { ',' };
            String[] values = tmp.Split(seperator);

            ///Add value to list after remove unesessary
            foreach (var value in values)
            {
                result.Add(Convert.ToDouble(value));
            }

            return result;
        }

        #endregion
    }
}
