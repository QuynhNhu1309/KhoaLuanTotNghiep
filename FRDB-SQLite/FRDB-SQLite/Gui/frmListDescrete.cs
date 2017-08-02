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
using FRDB_SQLite;

namespace FRDB_SQLite.Gui
{
    public partial class frmListDescrete : DevExpress.XtraEditors.XtraForm
    {
        public frmListDescrete()
        {
            InitializeComponent();
            RefreshData1();
            PointList = null;
        }

        private DataTable dt;
        public List<DiscreteFuzzySetBLL> PointList { get; set; }

        #region 1. Button Click

        private void btnAdd_Click(object sender, EventArgs e)
        {
            frmDescreteEditor frm = new frmDescreteEditor(false);
            frm.ShowDialog();

            //Refresh gridView1 after form closed
            RefreshData1();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            //List<DiscreteFuzzySetBLL> list = GetSelectedRows();
            List<String> list = GetSelectedRows1();

            if (list.Count == 0)
            {
                MessageBox.Show("Select some rows or check to check box!");
                return;
            }

            //if (new DiscreteFuzzySetBLL().Delete(list) == 1)
            if (new FuzzyProcess().DeleteList(list) == 1)
            {
                //Update gridView1
                //for (int i = 0; i < gridView1.DataRowCount; i++)
                //{
                //    if (gridView1.GetRowCellValue(i, "check").ToString() == "True")
                //        gridView1.DeleteRow(i);
                //}
                RefreshData1();

                MessageBox.Show("Delete selected DONE!");
            }
            else
            {
                frmRunAsAdministrator frm = new frmRunAsAdministrator();
                frm.ShowDialog();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        #region 2. Data Process

        private List<DiscreteFuzzySetBLL> GetSelectedRows()
        {
            List<DiscreteFuzzySetBLL> result = new List<DiscreteFuzzySetBLL>();

            for (int i = 0; i < gridView1.DataRowCount; i++)
            {
                if (gridView1.GetRowCellValue(i, "check").ToString() == "True")
                {
                    DiscreteFuzzySetBLL set = new DiscreteFuzzySetBLL();
                    set.FuzzySetName = gridView1.GetRowCellValue(i, "name").ToString().Trim();
                    set.ValueSet = SplitString(gridView1.GetRowCellValue(i, "values").ToString().Trim());
                    set.MembershipSet = SplitString(gridView1.GetRowCellValue(i, "memberships").ToString().Trim());

                    result.Add(set);
                }
            }

            return result;
        }

        private List<String> GetSelectedRows1()
        {
            List<String> result = new List<String>();

            for (int i = 0; i < gridView1.DataRowCount; i++)
            {
                if (gridView1.GetRowCellValue(i, "check").ToString() == "True")
                {
                    string path = Directory.GetCurrentDirectory() + @"\lib\";
                    string name = path + gridView1.GetRowCellValue(i, "name").ToString();
                    name += ".disFS";

                    result.Add(name);
                }
            }

            return result;
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
            string path = Directory.GetCurrentDirectory() + @"\lib\";
            List<DisFS> list = new FuzzyProcess().GenerateAllDisFS(path);
            dt = new DataTable();

            dt.Columns.Add("check", typeof(Boolean));
            dt.Columns.Add(new DataColumn("name"));
            dt.Columns.Add(new DataColumn("values"));
            dt.Columns.Add(new DataColumn("memberships"));

            foreach (var item in list)
            {
                DataRow dr = dt.NewRow();
                dr[0] = false;
                dr[1] = item.Name;
                dr[2] = "{" + item.V + "}";
                dr[3] = "{"+ item.M +"}";
                dt.Rows.Add(dr);
            }

            gridControl1.DataSource = dt;
        }

        private void RefreshData()
        {
            BindingList<Data> gridDataList = new BindingList<Data>();
            List<DiscreteFuzzySetBLL> list = new DiscreteFuzzySetBLL().GetAll();

            foreach (var item in list)
            {
                gridDataList.Add(new Data(false, item.FuzzySetName,
                    ConvertToString(item.ValueSet), ConvertToString(item.MembershipSet)));
            }

            gridControl1.DataSource = null;
            gridControl1.DataSource = gridDataList;
        }

        private String ConvertToString(List<Double> objs)
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

        private List<Double> SplitString(String str)
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

        #region 3. Events Handler

        private void gridView1_CustomRowCellEditForEditing(object sender, DevExpress.XtraGrid.Views.Grid.CustomRowCellEditEventArgs e)
        {
            if (gridView1.DataRowCount != 0 && gridView1.GetRowCellValue(e.RowHandle, "name") != null)
            {
                String name = gridView1.GetRowCellValue(e.RowHandle, "name").ToString();
                List<Double> values = SplitString(gridView1.GetRowCellValue(e.RowHandle, "values").ToString());
                List<Double> memberships = SplitString(gridView1.GetRowCellValue(e.RowHandle, "memberships").ToString());
                frmDescreteEditor frm = new frmDescreteEditor(null,name, values, memberships,false);
                frm.ShowDialog();

                chkSelectAll.Checked = false;
                //Update gridView1 after Update Descrete Fuzzy Set on frmDescreteEditor Form. (after form closed).
                RefreshData1();
            }
        }

        private void gridView1_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            int[] indexRows = gridView1.GetSelectedRows();

            for (int i = 0; i < indexRows.Length; i++)
            {
                if (gridView1.GetRowCellValue(indexRows[i], "check").ToString() == "True")
                {
                    gridView1.SetRowCellValue(indexRows[i], "check", false);
                }
                else
                {
                    gridView1.SetRowCellValue(indexRows[i], "check", true);
                }
            }

            CheckCheckBox();

        }

        private void CheckCheckBox()
        {
            //Count item checked to check to checkbox select all
            int count = 0;
            for (int i = 0; i < gridView1.DataRowCount; i++)
            {
                if (gridView1.GetRowCellValue(i, "check").ToString() == "True")
                {
                    count++;
                }
            }

            if (count == gridView1.DataRowCount)
            {
                chkSelectAll.Checked = true;
            }
            else
            {
                chkSelectAll.Checked = false;
                //chkSelectAll.Checked = false;
            }
        }

        private void chkSelectAll_MouseClick(object sender, MouseEventArgs e)
        {
            if (!chkSelectAll.Checked)
            {
                for (int i = 0; i < gridView1.DataRowCount; i++)
                {
                    gridView1.SetRowCellValue(i, "check", true);
                }
            }
            else
            {
                for (int i = 0; i < gridView1.DataRowCount; i++)
                {
                    gridView1.SetRowCellValue(i, "check", false);
                }
            }
        }

        private void chkSelectAll_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            chkSelectAll_MouseClick(sender, e);
        }

        private void txtFill_EditValueChanged(object sender, EventArgs e)
        {
            DataTable tmpDt = new DataTable();
            tmpDt.Columns.Add("check", typeof(Boolean));
            tmpDt.Columns.Add(new DataColumn("name"));
            tmpDt.Columns.Add(new DataColumn("values"));
            tmpDt.Columns.Add(new DataColumn("memberships"));

            String filterText = txtFill.Text.Trim().ToLower();
            foreach (DataRow row in dt.Rows)
            {
                if (row[1].ToString().ToLower().Contains(filterText) ||
                    row[2].ToString().ToLower().Contains(filterText) ||
                    row[3].ToString().ToLower().Contains(filterText))
                {
                    tmpDt.ImportRow(row);
                }
            }

            gridControl1.DataSource = tmpDt;
        }

        #endregion

        #region Fuzzy Set Chart
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
        #endregion

    }

    class Data
    {
        private bool _check;

        public bool Check
        {
            get { return _check; }
            set { _check = value; }
        }
        private String _name;

        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private String _values;

        public String Values
        {
            get { return _values; }
            set { _values = value; }
        }
        private String _memberships;

        public String Memberships
        {
            get { return _memberships; }
            set { _memberships = value; }
        }

        public Data(bool check, String name, String values, String memberships)
        {
            _check = check;
            _name = name;
            _values = values;
            _memberships = memberships;
        }
    }
}