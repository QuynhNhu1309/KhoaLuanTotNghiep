using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Diagnostics;

namespace FRDB_SQLite
{
    public partial class frmRunAsAdministrator : DevExpress.XtraEditors.XtraForm
    {
        public frmRunAsAdministrator()
        {
            InitializeComponent();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            this.Close();
            Process.Start("UserGuide.txt");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("Huong dan cai dat va su dung.doc");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Your PC is missing word document reader");
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}