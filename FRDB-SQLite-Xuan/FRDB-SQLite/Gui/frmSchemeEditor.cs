using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using FRDB_SQLite.Class;

namespace FRDB_SQLite.Gui
{
    public partial class frmSchemeEditor : DevExpress.XtraEditors.XtraForm
    {
        public frmSchemeEditor()
        {
            InitializeComponent();
            GetListScheme();
            CreateScheme = OpenScheme = DelecteScheme = null;
        }

        public String CreateScheme { get; set; }
        public String DelecteScheme { get; set; }
        public String OpenScheme { get; set; }

        private void GetListScheme()
        {
            if (DBValues.schemesName.Count != 0 && DBValues.schemesName != null)
            {
                foreach (var item in DBValues.schemesName)//DBValues.schemesName not null because we assigned before load in OpenScheme method
                {
                    cboSchemes.Items.Add(item);
                }
                cboSchemes.SelectedIndex = 0;
            }
        }

        private void btbCreate_Click(object sender, EventArgs e)
        {
            if (txtSchemeName.Text.Trim() == "")
            {
                MessageBox.Show("Scheme Name empty!");
            }
            else if (DBValues.schemesName.Contains(txtSchemeName.Text))
            {
                MessageBox.Show("Scheme name existed!");
            }
            else
            {
                if (!Checker.NameChecking(txtSchemeName.Text.Trim()))
                {
                    MessageBox.Show("Your name can not contain special characters: " + Checker.GetSpecialCharaters());
                    return;
                }
                this.CreateScheme = txtSchemeName.Text.Trim();
                this.Close();
            }
        }

        private void btnOK_Click_1(object sender, EventArgs e)
        {
            if (cboSchemes.Items.Count <= 0)
            {
                MessageBox.Show("There are no schemes, please create new scheme!");
            }
            else if ( cboSchemes.SelectedItem.ToString() == String.Empty)
            {
                MessageBox.Show("Please select an item from combobox!");
            }
            else
            {
                this.OpenScheme = cboSchemes.SelectedItem.ToString();
                this.Close();
            }
        }

        private void btnCancel_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtDelete_Click(object sender, EventArgs e)
        {
            if (cboSchemes.Items.Count <= 0)
            {
                MessageBox.Show("There are no schemes for deleting!");
                return;
            }
            else if ( cboSchemes.SelectedItem.ToString().Equals(""))
            {
                DelecteScheme = String.Empty;
            }
            else
            {
                DelecteScheme = cboSchemes.SelectedItem.ToString();
            }

            this.Close();
        }

        

       
    }
}