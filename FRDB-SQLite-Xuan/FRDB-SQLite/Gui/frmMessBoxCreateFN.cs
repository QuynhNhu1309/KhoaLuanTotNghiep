using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FRDB_SQLite.Gui
{
    public partial class frmMessBoxCreateFN : Form
    {
        public frmMessBoxCreateFN()
        {
            InitializeComponent();
           
        }
        
        public string newFS;
        public string select;
        public List<FzDiscreteFuzzySetEntity> DiscreteFSs = new List<FzDiscreteFuzzySetEntity>();
        public List<FzContinuousFuzzySetEntity> ContinuousFSs = new List<FzContinuousFuzzySetEntity>();
        private void btnDis_Click(object sender, EventArgs e)
        {
            this.Close();
            select = "dis";
        }

        private void btnCon_Click(object sender, EventArgs e)
        {
            this.Close();
            select = "con";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
            select = "cancel";
        }
    }
}
