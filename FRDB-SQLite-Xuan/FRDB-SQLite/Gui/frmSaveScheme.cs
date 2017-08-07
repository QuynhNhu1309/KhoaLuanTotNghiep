using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace FRDB_SQLite.Gui
{
    public partial class frmSaveScheme : DevExpress.XtraEditors.XtraForm
    {
        public frmSaveScheme()
        {
            InitializeComponent();
            txtNewScheme.Focus();
            GetListScheme();
            SchemeName = String.Empty;
        }

        public String SchemeName { get; set; }

        private void GetListScheme()
        {
            checkEdit1.Checked = true;
            txtNewScheme.Focus();

            try
            {
                if (DBValues.schemesName.Count != 0 && DBValues.schemesName != null)
                {
                    foreach (var item in DBValues.schemesName)
                    {
                        cboSchemes.Items.Add(item);
                    }

                    cboSchemes.SelectedIndex = 0;
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (checkEdit1.Checked)
            {
                if (txtNewScheme.Text.Trim() == String.Empty)
                {
                    MessageBox.Show("Enter new scheme name");
                }
                else if (DBValues.schemesName.Contains(txtNewScheme.Text))
                {
                    MessageBox.Show("This scheme name has already existed!");
                }
                else
                {
                    SchemeName = txtNewScheme.Text.Trim();
                    this.Close();
                }
            }
            else// That mean update attribute to existed scheme which not inherited
            {
                if (cboSchemes.SelectedItem.ToString() == String.Empty)
                {
                    MessageBox.Show("Please select an item!");
                }
                else
                {
                    SchemeName = cboSchemes.SelectedItem.ToString();
                    this.Close();
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkEdit1_Click(object sender, EventArgs e)
        {
            if (checkEdit1.Checked)
            {
                checkEdit2.Checked = false;
                txtNewScheme.Enabled = true;
                cboSchemes.Enabled = false;
            }
        }

        private void checkEdit2_Click(object sender, EventArgs e)
        {
            if (checkEdit2.Checked)
            {
                checkEdit1.Checked = false;
                cboSchemes.Enabled = true;
                txtNewScheme.Enabled = false;
            }
        }
    }
}