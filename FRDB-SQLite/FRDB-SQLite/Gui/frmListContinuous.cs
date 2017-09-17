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
    public partial class frmListContinuous : DevExpress.XtraEditors.XtraForm
    {
        public frmListContinuous()
        {
            InitializeComponent();
            RefreshData();
            PointList = null;

        }

        private DataTable dt;
        public List<ContinuousFuzzySetBLL> PointList { get; set; }

        #region 1. Data Process
        private List<ContinuousFuzzySetBLL> GetSelectedRows()
        {
            List<ContinuousFuzzySetBLL> result = new List<ContinuousFuzzySetBLL>();

            for (int i = 0; i < gridView1.DataRowCount; i++)
            {
                if (gridView1.GetRowCellValue(i, "check").ToString() == "True")
                {
                    ContinuousFuzzySetBLL set = new ContinuousFuzzySetBLL();
                    set.FuzzySetName = gridView1.GetRowCellValue(i, "name").ToString();
                    set.Bottom_Left = Convert.ToDouble(gridView1.GetRowCellValue(i, "bottomLeft"));
                    set.Top_Left = Convert.ToDouble(gridView1.GetRowCellValue(i, "topLeft"));
                    set.Top_Right = Convert.ToDouble(gridView1.GetRowCellValue(i, "topRight"));
                    set.Bottom_Right = Convert.ToDouble(gridView1.GetRowCellValue(i, "bottomRight"));

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
                    name += ".conFS";
                    result.Add(name);
                }
            }

            return result;
        }

        private void RefreshData()
        {
            ///////Old database//////////////////////////////
            //gridControl1.DataSource = null;
            //List<ContinuousFuzzySetBLL> list = new ContinuousFuzzySetBLL().GetAll();
            //dt = new DataTable();

            //dt.Columns.Add("check", typeof(Boolean));
            //dt.Columns.Add("name", typeof(String));
            //dt.Columns.Add("bottomLeft", typeof(Double));
            //dt.Columns.Add("topLeft", typeof(Double));
            //dt.Columns.Add("topRight", typeof(Double));
            //dt.Columns.Add("bottomRight", typeof(Double));

            //foreach (var item in list)
            //{
            //    DataRow dr = dt.NewRow();
            //    dr[0] = false;
            //    dr[1] = item.FuzzySetName;
            //    dr[2] = item.Bottom_Left;
            //    dr[3] = item.Top_Left;
            //    dr[4] = item.Top_Right;
            //    dr[5] = item.Bottom_Right;
            //    dt.Rows.Add(dr);
            //}

            gridControl1.DataSource = dt;
            gridControl1.DataSource = null;
            FuzzyProcess fz = new FuzzyProcess();
            List<ConFS> list = fz.GenerateAllConFS(Directory.GetCurrentDirectory() + @"\lib\");

            dt = new DataTable();

            dt.Columns.Add("check", typeof(Boolean));
            dt.Columns.Add("name", typeof(String));
            dt.Columns.Add("bottomLeft", typeof(Double));
            dt.Columns.Add("topLeft", typeof(Double));
            dt.Columns.Add("topRight", typeof(Double));
            dt.Columns.Add("bottomRight", typeof(Double));

            foreach (var item in list)
            {
                DataRow dr = dt.NewRow();
                dr[0] = false;
                dr[1] = item.Name;
                dr[2] = item.Bottom_Left;
                dr[3] = item.Top_Left;
                dr[4] = item.Top_Right;
                dr[5] = item.Bottom_Right;
                dt.Rows.Add(dr);
            }

            gridControl1.DataSource = dt;

        }
        #endregion

        #region 2. Button Click
        private void btnAdd_Click(object sender, EventArgs e)
        {
            frmContinuousEditor frm = new frmContinuousEditor(false);
            frm.ShowDialog();

            //Refresh gridView1 after form closed
            RefreshData();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            List<String> list = GetSelectedRows1();

            if (list.Count == 0)
            {
                MessageBox.Show("Select some rows or check to check box!");
                return;
            }

            if (new FuzzyProcess().DeleteList(list) == 1)
            {
                RefreshData();

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

        #region 3. Events Handler
        private void gridView1_CustomRowCellEditForEditing(object sender, DevExpress.XtraGrid.Views.Grid.CustomRowCellEditEventArgs e)
        {
            if (gridView1.DataRowCount != 0 && gridView1.GetRowCellValue(e.RowHandle, "name") != null)
            {
                String name = gridView1.GetRowCellValue(e.RowHandle, "name").ToString();
                Double bl = Convert.ToDouble(gridView1.GetRowCellValue(e.RowHandle, "bottomLeft"));
                Double tl = Convert.ToDouble(gridView1.GetRowCellValue(e.RowHandle, "topLeft"));
                Double tr = Convert.ToDouble(gridView1.GetRowCellValue(e.RowHandle, "topRight"));
                Double br = Convert.ToDouble(gridView1.GetRowCellValue(e.RowHandle, "bottomRight"));

                frmContinuousEditor frm = new frmContinuousEditor(null, name, bl, tl, tr, br,false);
                frm.ShowDialog();

                chkSelectAll.Checked = false;
                //Update gridView1 after Update Descrete Fuzzy Set on frmDescreteEditor Form. (after form closed).
                RefreshData();
                Refresh();
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

        private void txtFill_EditValueChanged(object sender, EventArgs e)
        {
            DataTable tmpDt = new DataTable();
            tmpDt.Columns.Add("check", typeof(Boolean));
            tmpDt.Columns.Add("name", typeof(String));
            tmpDt.Columns.Add("bottomLeft", typeof(Double));
            tmpDt.Columns.Add("topLeft", typeof(Double));
            tmpDt.Columns.Add("topRight", typeof(Double));
            tmpDt.Columns.Add("bottomRight", typeof(Double));

            String filterText = txtFill.Text.Trim().ToLower();
            foreach (DataRow row in dt.Rows)
            {
                if (row[1].ToString().ToLower().Contains(filterText) ||
                    row[2].ToString().ToLower().Contains(filterText) ||
                    row[3].ToString().ToLower().Contains(filterText) ||
                    row[4].ToString().ToLower().Contains(filterText) ||
                    row[5].ToString().ToLower().Contains(filterText))
                {
                    tmpDt.ImportRow(row);
                }
            }

            gridControl1.DataSource = tmpDt;

        }

        private void chkSelectAll_Click(object sender, EventArgs e)
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

        private void chkSelectAll_DoubleClick(object sender, EventArgs e)
        {
            chkSelectAll_Click(sender, e);
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
}