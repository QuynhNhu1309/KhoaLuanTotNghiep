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
    public partial class frmNewName : DevExpress.XtraEditors.XtraForm
    {
        public frmNewName()
        {
            InitializeComponent();
            Name = null;
            txtName.Focus();
        }
        public frmNewName(int action)
        {
            InitializeComponent();
            Name = null;
            txtName.Focus();
            this.action = action;
        }

        private int action = 0;//action = 1 mean rename database, action = 2 mean rename query

        private void btnRename_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Checker.NameChecking(txtName.Text.Trim()))
                {
                    MessageBox.Show("Your name can not contain special characters: " + Checker.GetSpecialCharaters());
                    return;
                }
                if (action == 1)// rename database
                {
                    if (txtName.Text == null)
                        MessageBox.Show("Please enter a name");
                    else
                    {
                        this.Name = txtName.Text.Trim();
                        this.Close();
                   } 
                }
                else if (action == 2)//rename query
                {
                    if (txtName.Text == null)
                        MessageBox.Show("Please enter a name");
                    else if (DBValues.queriesName.Contains(txtName.Text.Trim()))
                        MessageBox.Show("This name has already existed in the database");
                    else
                    {
                        Name = txtName.Text.Trim();
                        this.Close();
                    }
                }
                else if (action == 3)//rename scheme
                {
                    if (txtName.Text == null)
                        MessageBox.Show("Please enter a name");
                    else if (DBValues.schemesName.Contains(txtName.Text))
                        MessageBox.Show("This name has already existed in the database");
                    else
                    {
                        Name = txtName.Text.Trim();
                        this.Close();
                    }
                }
                else if (action == 4)//rename relation
                {
                    if (txtName.Text == null)
                        MessageBox.Show("Please enter a name");
                    else if (DBValues.relationsName.Contains(txtName.Text))
                        MessageBox.Show("This name has already existed in the database");
                    else
                    {
                        Name = txtName.Text.Trim();
                        this.Close();
                    }
                }
                else if (action == 5)//rename membership discrete FS
                {
                    if (txtName.Text == null)
                        MessageBox.Show("Please enter a name");
                    else if (DBValues.disFNName.Contains(txtName.Text))
                        MessageBox.Show("This name has already existed in the database");
                    else
                    {
                        Name = txtName.Text.Trim();
                        this.Close();
                    }
                }
                else if (action == 6)//rename membership continuous FS
                {
                    if (txtName.Text == null)
                        MessageBox.Show("Please enter a name");
                    else if (DBValues.conFNName.Contains(txtName.Text))
                        MessageBox.Show("This name has already existed in the database");
                    else
                    {
                        Name = txtName.Text.Trim();
                        this.Close();
                    }
                }
                else
                {
                    Name = null;
                    this.Close();
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Name = null;
            this.Close();
        }


    }
}