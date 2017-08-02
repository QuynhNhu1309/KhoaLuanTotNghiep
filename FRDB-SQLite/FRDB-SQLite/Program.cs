using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using FRDB_SQLite;
using FRDB_SQLite.Gui;


namespace FRDB_SQLite.Gui
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            DevExpress.Skins.SkinManager.EnableFormSkins();
            DevExpress.UserSkins.OfficeSkins.Register();
            DevExpress.UserSkins.BonusSkins.Register();
            //UserLookAndFeel.Default.SetSkinStyle("DevExpress Style");
            UserLookAndFeel.Default.SetSkinStyle("McSkin");

            Application.Run(new frmMain());
        }
    }

    public struct DBValues
    {
        public static String dbName;
        public static String connString;
        public static List<String> schemesName;
        public static List<String> relationsName;
        public static List<String> queriesName;
        public static List<String> disFNName;
        public static List<String> conFNName;
    }
}