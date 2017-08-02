using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Diagnostics;

namespace FRDB_SQLite.Gui
{
    public partial class frmHelp : DevExpress.XtraEditors.XtraForm
    {
        public frmHelp()
        {
            InitializeComponent();
            //Process.Start("mailto:trinhluu33@gmail.com");
            //Process.Start("SQLite-1.0.66.0-setup.exe");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabel1.Text);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabel2.Text);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
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
    }
}