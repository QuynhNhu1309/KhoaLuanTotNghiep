using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using FRDB_SQLite;
using System.Text.RegularExpressions;
using FRDB_SQLite.Class;

namespace FRDB_SQLite.Gui
{
    public partial class frmQueryEditor : DevExpress.XtraEditors.XtraForm
    {
        public frmQueryEditor()
        {
            InitializeComponent();
            QueryName = null;
            QueryText = null;
            txtQueryName.Focus();
        }
        public frmQueryEditor(List<FzQueryEntity> queries)
        {
            InitializeComponent();
            QueryName = null;
            QueryText = null;
            txtQueryName.Focus();
            this.Queries = queries;
        }

        public List<FzQueryEntity> Queries = new List<FzQueryEntity>();
        public String QueryName { get; set; }
        public String QueryText { get; set; }

        public String TxtQueryName
        {
            get { return this.txtQueryName.Text; }
            set { this.txtQueryName.Text = value; }
        }
        public String TxtQueryText
        {
            get { return this.txtQuery.Text; }
            set { this.txtQuery.Text = value; }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtQueryName.Text.Trim() == "")
                {
                    MessageBox.Show("Please input the query name!");
                    txtQueryName.Focus();
                }
                else if (txtQuery.Text.Trim() == "")
                {
                    MessageBox.Show("Please input the query text!");
                    txtQuery.Focus();
                }
                else
                {
                    QueryName = txtQueryName.Text.Trim().Replace("'", "");
                    QueryText = txtQuery.Text.Trim().Replace("'", "");
                    if (!Checker.NameChecking(txtQueryName.Text.Trim()))
                    {
                        MessageBox.Show("Your name can not contain special characters: " + Checker.GetSpecialCharaters());
                        return;
                    }
                    int count = 0;
                    foreach (var item in Queries)
                    {
                        if (item.QueryName.Equals(QueryName))
                        {
                            item.QueryString = QueryText;
                            MessageBox.Show("Update item done!");
                            break;
                        }
                        count++;
                    }
                    if (count == Queries.Count)
                    {
                        Queries.Add(new FzQueryEntity() { QueryString = QueryText, QueryName = QueryName });

                        frmQueryEditor_Load(sender, e);
                        MessageBox.Show("Save item done!");
                    }
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (txtQueryName.Text.Trim() == "")
            {
                MessageBox.Show("There are no queries (tip: open query by choosing from drop down list)");
                return;
            }
            this.QueryName = txtQueryName.Text.Trim();
            this.QueryText = txtQuery.Text.Trim();
            this.Close();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (txtQueryName.Text.Trim() == "")
            {
                MessageBox.Show("There are no queries to delete");
                return;
            }
            DialogResult result = new DialogResult();
            result = MessageBox.Show("Are you want to delete this query ?", "Delete query " + txtQueryName.Text, MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                foreach (var item in Queries)
                {
                    if (item.QueryName.Equals(txtQueryName.Text.Trim()))
                    {
                        this.Queries.Remove(item);

                        frmQueryEditor_Load(sender, e);
                        txtQueryName.Text = txtQuery.Text = null;
                        MessageBox.Show("Delete item done!");
                        break;
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            QueryName = QueryText = null;
            this.Close();
        }

        private void frmQueryEditor_Load(object sender, EventArgs e)
        {
            // Hilight the text specify
            txtQuery_TextChanged();
            // Clear all the items of combobox
            ComboBox_OpenQuery.Items.Clear();

            if (this.Queries.Count > 0)
            {
                foreach (FzQueryEntity item in this.Queries)
                {
                    ComboBox_OpenQuery.Items.Add(item.QueryName);
                }

                ComboBox_OpenQuery.SelectedIndex = -1;
            }
        }

        private void ComboBox_OpenQuery_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (this.Queries.Count > 0)
            {
                foreach (var item in this.Queries)
                {
                    if (ComboBox_OpenQuery.SelectedItem.ToString() == item.QueryName)
                    {
                        txtQueryName.Text = item.QueryName;
                        txtQuery.Text = item.QueryString;
                        break;
                    }
                }
            }
        }

        private void txtQuery_TextChanged()
        {
            //textBox1.SelectionStart = 0;
            //textBox1.SelectionLength = textBox1.Text.Length;
            // Add the keywords to the list.
            txtQuery.Settings.Keywords.Add("select");
            txtQuery.Settings.Keywords.Add("from");
            txtQuery.Settings.Keywords.Add("where");
            // The operators and logicality
            txtQuery.Settings.Keywords2.Add("and");
            txtQuery.Settings.Keywords2.Add("or");
            txtQuery.Settings.Keywords2.Add("not");

            // Set the comment identifier. For Lua this is two minus-signs after each other (--). 
            // For C++ we would set this property to "//".
            txtQuery.Settings.Comment = "--";
            txtQuery.Settings.Between = "\"";

            // Set the colors that will be used.
            txtQuery.Settings.KeywordColor = Color.Blue;
            txtQuery.Settings.KeywordColor2 = Color.Gray;
            txtQuery.Settings.CommentColor = Color.Green;
            txtQuery.Settings.StringColor = Color.Black;
            txtQuery.Settings.IntegerColor = Color.DarkOrchid;//DarkLayGray

            // Let's not process strings and integers.
            txtQuery.Settings.EnableStrings = false;
            //txtQuery.Settings.EnableIntegers = false;

            // Let's make the settings we just set valid by compiling
            // the keywords to a regular expression.
            txtQuery.CompileKeywords();

            // LUpdate the syntax highlighting.
            txtQuery.ProcessAllLines();
        }
    }
}