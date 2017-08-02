using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.IO;
using FRDB_SQLite;
using FRDB_SQLite;

namespace FRDB_SQLite.Gui
{
    public partial class frmContinuousEditor : DevExpress.XtraEditors.XtraForm
    {
        public frmContinuousEditor(bool isMembership)
        {
            InitializeComponent();
            this.isMembership = isMembership;
            ReadOnlytxtLinguistic();
        }
        public frmContinuousEditor(List<FzContinuousFuzzySetEntity> continuousFSs,String name,bool isMembership)
        {
            InitializeComponent();
            this.ContinuousFSs = continuousFSs;
            txtLinguistic.Text = name;
            this.isMembership = isMembership;
            ReadOnlytxtLinguistic();
        }
        public frmContinuousEditor(List<FzContinuousFuzzySetEntity> continuousFSs,bool isMembership)
        {
            InitializeComponent();
            this.ContinuousFSs = continuousFSs;
            this.isMembership = isMembership;
            //this.oldName = txtLinguistic.Text;
        }


        public frmContinuousEditor(List<FzContinuousFuzzySetEntity> continuousFSs,String name, Double bl, Double tl, Double tr, Double br, bool isMembership)
        {
            InitializeComponent();
            this.ContinuousFSs = continuousFSs;
            txtLinguistic.Text = name;
            txtBottomLeft.Text = bl.ToString();
            txtTopLeft.Text = tl.ToString();
            txtTopRight.Text = tr.ToString();
            txtBottomRight.Text = br.ToString();
            this.isMembership = isMembership;
            ReadOnlytxtLinguistic();
            //this.oldName = name;
        }
        public List<FzContinuousFuzzySetEntity> ContinuousFSs = new List<FzContinuousFuzzySetEntity>();
        public bool isMembership = false; // this fuzzy set is a membership fuzzy set or not
        //public String oldName;
        private void ReadOnlytxtLinguistic()
        {
            if (isMembership)
                txtLinguistic.ReadOnly = true;
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!CheckNull()) return;
            if (!CheckLogicValue()) return;
            //ContinuousFuzzySetBLL newFS = new ContinuousFuzzySetBLL();
            FuzzyProcess fz = new FuzzyProcess();
            //ConFS newFS = new ConFS();
            //newFS.FuzzySetName = txtLinguistic.Text.Trim();
            var name = txtLinguistic.Text.Trim();
            var topLeft = txtTopLeft.Text.Trim();
            var topRight = txtTopRight.Text.Trim();
            var bottomLeft = txtBottomLeft.Text.Trim();
            var bottomRight = txtBottomRight.Text.Trim();
            String content = bottomLeft;

            string path = Directory.GetCurrentDirectory() + @"\lib\";
            if (topLeft == "" && topRight != "")
            {
                topLeft = topRight;
            }
               
            else if (topLeft != "" && topRight == "")
            {
                topRight = topLeft;
            }
            content += "," + topLeft + "," + topRight + "," + bottomRight;
            if (isMembership)
            {
                //if (DBValues.conFSName.Contains(name))
                //{
                //    bool flag = false;
                //    if (oldName != name)
                //    {
                //        DialogResult result = new DialogResult();
                //        result = MessageBox.Show("This name has already existed in the database. Re it?", "Rep Membership Continuous FS.", MessageBoxButtons.YesNo);
                //        if (result == DialogResult.Yes)
                //            flag = true;
                //    }
                //    else
                //        flag = true;
                //    if (flag)
                //    {
                int count = 0;
                        foreach (var item in ContinuousFSs)
                        {
                            if (item.Name.Equals(name))
                            {
                                item.Bottom_Left = Double.Parse(bottomLeft);
                                item.Top_Left = Double.Parse(topLeft);
                                item.Top_Right = Double.Parse(topRight);
                                item.Bottom_Right = Double.Parse(bottomRight);
                                MessageBox.Show("Update item done!");
                                break;
                            }
                    count++;
                        }
                //    }
                //}
                //else
                //{
                if (count == ContinuousFSs.Count)
                {
                    ContinuousFSs.Add(new FzContinuousFuzzySetEntity() { Name = name, Bottom_Left = Double.Parse(bottomLeft), Top_Left = Double.Parse(topLeft), Top_Right = Double.Parse(topRight), Bottom_Right = Double.Parse(bottomRight) });
                    MessageBox.Show("Save item done!");
                }
                //}
            }
            else
            {
                if (fz.UpdateFS(path, content, name + ".conFS") == 1)
                {
                    MessageBox.Show("Save Fuzzy Set DONE!");
                }
                else
                {
                    frmRunAsAdministrator frm = new frmRunAsAdministrator();
                    frm.ShowDialog();
                }
            }
        }

        private bool CheckNull()
        {
            if (txtLinguistic.Text.Trim() == "")
            {
                MessageBox.Show("The linguistic does not empty!");
                return false;
            }

            if (txtBottomLeft.Text.Trim() == "" || txtBottomLeft.Text.Trim() == null)
            {
                MessageBox.Show("Bottom-Left is empty!");
                return false;
            }

            if ((txtTopLeft.Text.Trim() == "" && txtTopRight.Text == ""))
            {
                MessageBox.Show("It' just allow one of Top-Left and Top-Right null!");
                return false;
            }

            if (txtBottomRight.Text.Trim() == "" || txtBottomRight.Text.Trim() == null)
            {
                MessageBox.Show("Bottom-Right is empty!");
                return false;
            }

            return true;
        }

        private Boolean CheckLogicValue()
        {

            Double bl = 0; if (txtBottomLeft.Text != "") bl = Double.Parse(txtBottomLeft.Text);
            Double tl = 0; if (txtTopLeft.Text != "") tl = Double.Parse(txtTopLeft.Text);
            Double tr = 0; if (txtTopRight.Text != "") tr = Double.Parse(txtTopRight.Text);
            Double br = 0; if (txtBottomRight.Text != "") br = double.Parse(txtBottomRight.Text);

            if (tl < bl || tr < tl || br < tr)
            {
                MessageBox.Show("Values of fuzzy set must be continous!");
                return false;
            }
            if (isMembership)
            {
                if (bl < 0 || bl > 1|| tl < 0 || tl > 1 || tr < 0 || tr > 1 || br < 0 || br > 1 )
                {
                    MessageBox.Show("Some values of value weren't correct data\n(Value values must be in [0, 1]!");
                    return false;
                }
            }
            return true;

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

   
    }
}