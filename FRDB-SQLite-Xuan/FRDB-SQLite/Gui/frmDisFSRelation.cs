using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using FRDB_SQLite;
using System.IO;

namespace FRDB_SQLite.Gui
{
    public partial class frmDisFSRelation : Form
    {
        private string _FSName;
        public frmDisFSRelation()
        {
            InitializeComponent();
            PointList = null;
        }
        public List<DiscreteFuzzySetBLL> PointList { get; set; }
        public string FSName
        {
            get { return _FSName; }
            set {_FSName = value; }
        }
        private DataTable dt;
        private void frmDisFSRelation_Load(object sender, EventArgs e)
        {
            txtLinguistic.Text = _FSName;
            RefreshData1();
        }
        private void RefreshData1()
        {
            //gridControl1.DataSource = null;
            //List<DiscreteFuzzySetBLL> list = new DiscreteFuzzySetBLL().GetAll();
            //dt = new DataTable();

            //dt.Columns.Add("check", typeof(Boolean));
            //dt.Columns.Add(new DataColumn("name"));
            //dt.Columns.Add(new DataColumn("values"));
            //dt.Columns.Add(new DataColumn("memberships"));

            //foreach (var item in list)
            //{
            //    DataRow dr = dt.NewRow();
            //    dr[0] = false;
            //    dr[1] = item.FuzzySetName;
            //    dr[2] = ConvertToString(item.ValueSet);
            //    dr[3] = ConvertToString(item.MembershipSet);
            //    dt.Rows.Add(dr);
            //}

            //gridControl1.DataSource = dt;

            gridControl1.DataSource = null;
            string path = Directory.GetCurrentDirectory() + @"\lib\temp\"+_FSName+".disFS";
            DisFS fuzzyset = new FuzzyProcess().ReadEachDisFS(path);
            dt = new DataTable();

            dt.Columns.Add(new DataColumn("values"));
            dt.Columns.Add(new DataColumn("memberships"));
            int i = -1;
            foreach(Double value in fuzzyset.ValueSet)
            {
                DataRow dr = dt.NewRow();
                dr[0] = value;
                dr[1] = fuzzyset.MembershipSet[++i].ToString();
                dt.Rows.Add(dr);
            }

            gridControl1.DataSource = dt;
        }
        private List<DiscreteFuzzySetBLL> GetSelectedRows()
        {
            List<DiscreteFuzzySetBLL> result = new List<DiscreteFuzzySetBLL>();

            string path = Directory.GetCurrentDirectory() + @"\lib\temp\" + _FSName + ".disFS";
            DisFS fuzzyset = new FuzzyProcess().ReadEachDisFS(path);
            DiscreteFuzzySetBLL set = new DiscreteFuzzySetBLL();
            set.FuzzySetName = _FSName;
            set.ValueSet = fuzzyset.ValueSet;
            set.MembershipSet = fuzzyset.MembershipSet;
            result.Add(set);
            return result;
        }
        private void btnViewChart_Click(object sender, EventArgs e)
        {
            this.PointList = GetSelectedRows();
            if (PointList.Count == 0)
            {
                MessageBox.Show("Please select a fuzzy set to view the chart!\n You can choose more than one");
                return;
            }

            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        

    }
}
