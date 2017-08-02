using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FRDB_SQLite;


namespace FRDB_SQLite
{
    public class FdbBLL
    {
        #region 1. Fields

        FdbDAL fzDBTbbDal = new FdbDAL();
        
        #endregion

        #region 2. Properties (none)
        #endregion

        #region 3. Contructors (none)
        #endregion

        #region 4. Methods
        
        public static String GetRootPath(String path)
        {
            return FdbDAL.GetRootPath(path);
        }

        public static Boolean CheckConnection(FdbEntity fdb)
        {
            return FdbDAL.CheckConnection(fdb);
        }

        public bool CreateBlankDatabaseTbb(FdbEntity fdb)
        {
            return fzDBTbbDal.CreateBlankFdb(fdb);
        }

        public bool CreateFuzzyDatabase(FdbEntity fdb)
        {
            return fzDBTbbDal.CreateFuzzyDatabase(fdb);
        }

        public bool OpenFuzzyDatabase(FdbEntity fdb)
        {
            return fzDBTbbDal.OpenFuzzyDatabase(fdb);
        }

        public void DropFuzzyDatabase(FdbEntity fdb)
        {
            fzDBTbbDal.DropFuzzyDatabase(fdb);
        }

        public bool SaveFuzzyDatabaseAs(FdbEntity fdb)
        {
            return fzDBTbbDal.SaveFuzzyDatabaseAs(fdb);
        }

        public bool SaveFuzzyDatabase(FdbEntity fdb)
        { 
            return fzDBTbbDal.SaveFuzzyDatabase(fdb);
        }

        #endregion

        #region 5. Privates
        #endregion
        

    }
}
