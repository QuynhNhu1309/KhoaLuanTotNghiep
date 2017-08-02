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
    public partial class frmDataType : DevExpress.XtraEditors.XtraForm
    {
        public frmDataType()
        {
            InitializeComponent();
            InitializeValues();
        }

        #region 1. Properties

        public String  TypeName { get; set; }
        public String  DataType { get; set; }
        public String Domain { get; set; }

        private char[] specialCharacters;
        private String specialCharacter;

        #endregion

        #region 2. Methods

        private void FillDataType()
        { 
            cboDataType.Items.Add("Int16");
            cboDataType.Items.Add("Int32");
            cboDataType.Items.Add("Int64");
            cboDataType.Items.Add("Byte");
            cboDataType.Items.Add("String");
            cboDataType.Items.Add("Single");
            cboDataType.Items.Add("Double");
            cboDataType.Items.Add("Boolean");
            cboDataType.Items.Add("Decimal");
            cboDataType.Items.Add("DateTime");
            cboDataType.Items.Add("Binary");
            cboDataType.Items.Add("Currency");
            cboDataType.Items.Add("UserDefined");
        }

        private void InitializeValues()
        {
            TypeName = DataType = "";
            groupControl2.Enabled = false;
            specialCharacters = new char[] { '~', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '+',
                                            '`', ';', ',', '<', '>', '?', '/', ':', '\"', '\'', '=', '{', '}', '[', ']', '\\', '|' };
            specialCharacter = "";

            for (int i = 0; i < specialCharacters.Length; i++)
            {
                specialCharacter += specialCharacters[i].ToString();
            }

            FillDataType();
            cboDataType.SelectedIndex = 0;
            cboDataType.Focus();
        }

        private void cboDataType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboDataType.SelectedIndex == cboDataType.Items.Count - 1)
            {
                groupControl2.Enabled = true;
                txtTypeName.Focus();
            }
            else
            {
                groupControl2.Enabled = false;
                txtListValue.Clear();
            }
        }

        private void txtListValue_TextChanged(object sender, EventArgs e)
        {
            int start = txtListValue.TextLength;
            String charInput = txtListValue.Text[start - 1].ToString();

            if (specialCharacter.Contains(charInput))
            {
                MessageBox.Show("Do not input the special character '" + charInput + "'");
                txtListValue.Text = txtListValue.Text.Remove(start - 1, 1);
                txtListValue.SelectionStart = start;
                charInput = null;
            }
        }

        private string Standardize(string S) //Standardize String
        {
            // Standardize String, remove the excess
            string R = "";
            int i = 0;
            while (S[i] == ',') i++;
            int k = S.Length - 1;
            while (S[k] == ',') k--;
            for (int j = i; j <= k; j++)
                if (S[j] != ',') R += S[j];
                else if (S[j - 1] != ',') R += S[j] + " ";
            return R;
        }

        private String SetDomain(string s) //Set value record for datatype
        {
            switch (s)
            {
                case "Int16": return "[-32768  ...  32767]";
                case "Int32": return "[-2147483648  ...  2147483647]";
                case "Int64": return " [-9223372036854775808  ...  9223372036854775807]";
                case "Byte": return "[0  ...  255]";
                case "String": return "[0  ...  32767] characters";
                case "Single": return "[1.5 x 10^-45  ...  3.4 x 10^38]";
                case "Double": return "[5.0 x 10^-324  ...  1.7 x 10^308]";
                case "Boolean": return "true  /  false";
                case "Decimal": return "[1.0 x 10^-28  ...  7.9 x 10^28]";
                case "DateTime": return "[01/01/0001 C.E  ...  31/12/9999 A.D]";
                case "Binary": return "[1  ...  8000] bytes";
                case "Currency": return "[-2^-63  ...  2^63 - 1]";
            }
            return "";
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (cboDataType.Text != "")// Check whether user select the datatype
            {
                if (txtTypeName.Enabled == true)//Check whether user select UserDefine datatype
                {
                    if (txtListValue.Text == "" && txtTypeName.Text == "")
                    {
                        MessageBox.Show("Please enter type name and list values!");
                    }
                    else if (txtTypeName.Text == "")
                    {
                        MessageBox.Show("You have not enter a type name");
                    }
                    else if (txtListValue.Text == "")
                    {
                        MessageBox.Show("You have not entered a value type");
                    }
                    else
                    {
                        TypeName = txtTypeName.Text;
                        DataType = cboDataType.Items[cboDataType.SelectedIndex].ToString();
                        Domain = "{" + Standardize(txtListValue.Text.Replace("\r\n", ",")) + "}"; //Record after standardize
                        this.Close();
                    }
                }
                else//Check if user select normal data type
                {
                    TypeName = "";
                    DataType = cboDataType.Items[cboDataType.SelectedIndex].ToString();
                    Domain = SetDomain(DataType);
                    this.Close();
                }
            }
            else MessageBox.Show("Please selected data type!");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DataType = "Int32";
            Domain = "[-2147483648  ...  2147483647]";
            this.Close();
        }

        #endregion

        

    }
}