using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.Skins;
using DevExpress.LookAndFeel;
using DevExpress.UserSkins;
using DevExpress.XtraEditors;
using DevExpress.XtraBars.Helpers;
using DevExpress.Utils.Menu;

namespace FRDB_SQLite.Gui
{
    public partial class frmMessageBoxCreateMemFS : Form
    {
        public frmMessageBoxCreateMemFS()
        {
            InitializeComponent();
        }
        public frmMessageBoxCreateMemFS(string newFS)
        {
            this.newFS = newFS;
        }
        public string newFS;
        private void btnDis_Click(object sender, EventArgs e)
        {
            this.Close();
            frmDescreteEditor frm = new frmDescreteEditor(null,newFS, null, null, true);
            frm.Show();
        }

        private void btnCon_Click(object sender, EventArgs e)
        {
            this.Close();
            frmContinuousEditor frm = new frmContinuousEditor(null,newFS, 0, 0, 0, 0, true);
            frm.Show();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void lbMessage_Click(object sender, EventArgs e)
        {

        }
    }
}
