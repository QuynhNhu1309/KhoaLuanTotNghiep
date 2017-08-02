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
    public partial class frmFuzzySetAction : DevExpress.XtraEditors.XtraForm
    {
        public frmFuzzySetAction()
        {
            InitializeComponent();
            LoadDisFS();
            LoadConFS();
        }

        //private List<DiscreteFuzzySetBLL> disFSs;
        //private List<ContinuousFuzzySetBLL> conFSs;
        private List<ConFS> conFSs;
        private List<DisFS> disFSs;
        String path = Directory.GetCurrentDirectory() + @"\lib\";

        private void LoadDisFS()
        {
            //disFSs = new DiscreteFuzzySetBLL().GetAll();
            disFSs = new FuzzyProcess().GenerateAllDisFS(path);
            foreach (var item in disFSs)
            {
                cboDisFS.Properties.Items.Add(item.Name);
            }
        }

        private void LoadConFS()
        {
            conFSs = new FuzzyProcess().GenerateAllConFS(path);
            foreach (var item in conFSs)
            {
                cboConFS.Properties.Items.Add(item.Name);
            }
        }


        private void btnOK_Click_1(object sender, EventArgs e)
        {
            String input = txtValues.Text.Trim();
            String[] selectedValue = GetValues(input);

            if (!CheckInput(input, selectedValue)) return;
            //List<DiscreteFuzzySetBLL> selectedDisFS = GetSelectedDisFS();
            //List<ContinuousFuzzySetBLL> selectedConFS = GetSelectedConFS();
            List<ConFS> selectedConFS = GetSelectedConFS();
            List<DisFS> selectedDisFS = GetSelectedDisFS();

            lbConFS.Items.Clear();
            lbDisFS.Items.Clear();

            foreach (var item in selectedDisFS)
            {
                lbDisFS.Items.Add(item.Name);
                foreach (var value in selectedValue)
                {
                    Double v = Convert.ToDouble(value);
                    Double m = item.GetMembershipAt(v);
                    lbDisFS.Items.Add("(" + v + ", " + m + ")");
                }
            }
            foreach (var item in selectedConFS)
            {
                lbConFS.Items.Add(item.Name);
                foreach (var value in selectedValue)
                {
                    Double v = Convert.ToDouble(value);
                    Double m = item.GetMembershipAt(v);
                    lbConFS.Items.Add("(" + v + ", " + m + ")");
                }
            }

        }

        //private List<DiscreteFuzzySetBLL> GetSelectedDisFS()
        //{
        //    List<DiscreteFuzzySetBLL> result = new List<DiscreteFuzzySetBLL>();
        //    for (int i = 0; i < cboDisFS.Properties.Items.Count; i++)
        //    {
        //        if (cboDisFS.Properties.Items[i].CheckState == CheckState.Checked)
        //        {
        //            result.Add(disFSs[i]);
        //        }
        //    }
        //    return result;
        //}
        private List<DisFS> GetSelectedDisFS()
        {
            List<DisFS> result = new List<DisFS>();
            for (int i = 0; i < cboDisFS.Properties.Items.Count; i++)
            {
                if (cboDisFS.Properties.Items[i].CheckState == CheckState.Checked)
                {
                    result.Add(disFSs[i]);
                }
            }
            return result;
        }

        //private List<ContinuousFuzzySetBLL> GetSelectedConFS()
        //{
        //    List<ContinuousFuzzySetBLL> result = new List<ContinuousFuzzySetBLL>();
        //    for (int i = 0; i < cboConFS.Properties.Items.Count; i++)
        //    {
        //        if (cboConFS.Properties.Items[i].CheckState == CheckState.Checked)
        //            result.Add(conFSs[i]);
        //    }

        //    return result;
        //}
        private List<ConFS> GetSelectedConFS()
        {
            List<ConFS> result = new List<ConFS>();
            for (int i = 0; i < cboConFS.Properties.Items.Count; i++)
            {
                if (cboConFS.Properties.Items[i].CheckState == CheckState.Checked)
                    result.Add(conFSs[i]);
            }

            return result;
        }

        private String[] GetValues(String input)
        {
            String[] result = null;
            input = input.Replace(" ", "");
            result = input.Split(',');

            return result;
        }

        private bool CheckInput(String input, String[] s)
        {
            if (input == "")
            {
                MessageBox.Show("Please enter value to get the membership!");
                return false;
            }
            int j = 0;
            foreach (var item in s)
            {
                Double t = 0;
                if (Double.TryParse(item, out t) == false)
                {
                    MessageBox.Show("Value " + (j + 1 )+ " incorrect double format");
                    return false;
                }
                j++;
            }

            int c = 0;
            for (int i = 0; i < cboDisFS.Properties.Items.Count; i++)
            {
                if (cboDisFS.Properties.Items[i].CheckState == CheckState.Checked)
                    c++;
            }
            int d = 0;
            for (int i = 0; i < cboConFS.Properties.Items.Count; i++)
            {
                if (cboConFS.Properties.Items[i].CheckState == CheckState.Checked)
                    d++;
            }
            if (c == 0 && d == 0)
            {
                MessageBox.Show("Please select an fuzzy set for getting");
                return false;
            }

            return true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}