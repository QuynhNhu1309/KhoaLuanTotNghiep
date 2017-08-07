using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data.SQLite;


using FRDB_SQLite;


namespace FRDB_SQLite
{
    public class FuzzySetDAL
    {
        #region 1. Fields
        FuzzySetLibraryEntities db = new FuzzySetLibraryEntities();
        #endregion

        #region 2. Properties (None)
        #endregion

        #region 3. Contructors (None)
        #endregion

        #region 4. Methods

        public int UpdateFuzzySet(FuzzySetBLL set)
        {
            try
            {
                int result = 0;

                if (IsExistFSName(set.FuzzySetName))//Update value
                {
                    var _set = db.MotherLibraries.SingleOrDefault(name => name.LanguisticLabel == set.FuzzySetName);
                    
                    //Only update value
                    _set.FuzzySet = ConvertToString(set.FuzzySet);

                    return result = db.SaveChanges(true);
                }
                else//Mean add new object to DB
                {
                    MotherLibrary mother = new MotherLibrary();
                    mother.ID = GetId();
                    mother.LanguisticLabel = set.FuzzySetName;
                    mother.FuzzySet = ConvertToString(set.FuzzySet);

                    db.AddToMotherLibraries(mother);
                    return result = db.SaveChanges(true);
                }
            }
            catch (SQLiteException ex)
            {
                //throw new Exception(ex.Message);
                return -1;
            }
        }

        public int DeleteFuzzySet(FuzzySetBLL set)
        {
            try
            {
                int result = 0;

                if (IsExistFSName(set.FuzzySetName))//Delete object
                {
                    var _set = db.MotherLibraries.SingleOrDefault(name => name.LanguisticLabel == set.FuzzySetName);

                    db.DeleteObject(_set);//Need some check some references
                    return result = db.SaveChanges(true);
                }
                else//Nothing to delete
                {
                    return result = -1;
                }
            }
            catch (SQLiteException ex)
            {
                return -1;
            }
        }

        #endregion

        #region 5. Privates

        private int GetId()
        {
            if (db.MotherLibraries.Count() == 0)
            {
                return 0;
            }
            else
            {
                return db.MotherLibraries.Last().ID + 1;
            }
        }

        private Boolean IsExistFSName(String name)//Also mean is exist an object in db
        {
            var _set = from s in db.MotherLibraries
                        where s.LanguisticLabel == name
                        select s;

            if (_set.Count() == 0)
            {
                return false;
            }

            return true;
        }

        private String ConvertToString(Hashtable fuzzySet)
        {
            String _fuzzySet = "{";

            foreach (KeyValuePair<Double, Double> item in fuzzySet)
            {
                _fuzzySet += "{" + item.Key + "," + item.Value + "},";
            }

            _fuzzySet = _fuzzySet.Remove(_fuzzySet.Length - 1);
            _fuzzySet += "}";

            return _fuzzySet;
        }

        #endregion
    }
}
