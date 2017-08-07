namespace FRDB_SQLite.Gui
{
    partial class frmFuzzySetAction
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.lbDisFS = new DevExpress.XtraEditors.ListBoxControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.lbConFS = new DevExpress.XtraEditors.ListBoxControl();
            this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
            this.groupControl2 = new DevExpress.XtraEditors.GroupControl();
            this.cboConFS = new DevExpress.XtraEditors.CheckedComboBoxEdit();
            this.cboDisFS = new DevExpress.XtraEditors.CheckedComboBoxEdit();
            this.btnOK = new DevExpress.XtraEditors.SimpleButton();
            this.txtValues = new DevExpress.XtraEditors.TextEdit();
            this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lbDisFS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lbConFS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).BeginInit();
            this.groupControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboConFS.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDisFS.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtValues.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // labelControl3
            // 
            this.labelControl3.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl3.Location = new System.Drawing.Point(141, 15);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(143, 19);
            this.labelControl3.TabIndex = 9;
            this.labelControl3.Text = "Fuzzy Sets Action";
            // 
            // groupControl1
            // 
            this.groupControl1.Controls.Add(this.lbDisFS);
            this.groupControl1.Controls.Add(this.labelControl1);
            this.groupControl1.Controls.Add(this.lbConFS);
            this.groupControl1.Controls.Add(this.labelControl6);
            this.groupControl1.Location = new System.Drawing.Point(12, 169);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(407, 271);
            this.groupControl1.TabIndex = 13;
            this.groupControl1.Text = "Result";
            // 
            // lbDisFS
            // 
            this.lbDisFS.Location = new System.Drawing.Point(5, 44);
            this.lbDisFS.Name = "lbDisFS";
            this.lbDisFS.Size = new System.Drawing.Size(178, 222);
            this.lbDisFS.TabIndex = 0;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(19, 25);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(58, 13);
            this.labelControl1.TabIndex = 12;
            this.labelControl1.Text = "Discrete FS:";
            // 
            // lbConFS
            // 
            this.lbConFS.Location = new System.Drawing.Point(224, 41);
            this.lbConFS.Name = "lbConFS";
            this.lbConFS.Size = new System.Drawing.Size(178, 225);
            this.lbConFS.TabIndex = 0;
            // 
            // labelControl6
            // 
            this.labelControl6.Location = new System.Drawing.Point(247, 25);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(73, 13);
            this.labelControl6.TabIndex = 12;
            this.labelControl6.Text = "Continuous FS:";
            // 
            // groupControl2
            // 
            this.groupControl2.Controls.Add(this.cboConFS);
            this.groupControl2.Controls.Add(this.cboDisFS);
            this.groupControl2.Controls.Add(this.btnOK);
            this.groupControl2.Controls.Add(this.txtValues);
            this.groupControl2.Controls.Add(this.labelControl5);
            this.groupControl2.Controls.Add(this.labelControl2);
            this.groupControl2.Controls.Add(this.labelControl7);
            this.groupControl2.Controls.Add(this.labelControl4);
            this.groupControl2.Location = new System.Drawing.Point(12, 49);
            this.groupControl2.Name = "groupControl2";
            this.groupControl2.Size = new System.Drawing.Size(407, 114);
            this.groupControl2.TabIndex = 14;
            this.groupControl2.Text = "Action";
            // 
            // cboConFS
            // 
            this.cboConFS.Location = new System.Drawing.Point(278, 40);
            this.cboConFS.Name = "cboConFS";
            this.cboConFS.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboConFS.Size = new System.Drawing.Size(124, 20);
            this.cboConFS.TabIndex = 22;
            // 
            // cboDisFS
            // 
            this.cboDisFS.Location = new System.Drawing.Point(78, 40);
            this.cboDisFS.Name = "cboDisFS";
            this.cboDisFS.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboDisFS.Size = new System.Drawing.Size(100, 20);
            this.cboDisFS.TabIndex = 23;
            // 
            // btnOK
            // 
            this.btnOK.Image = global::FRDB_SQLite.Properties.Resources.small_OK;
            this.btnOK.Location = new System.Drawing.Point(327, 75);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 34);
            this.btnOK.TabIndex = 21;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click_1);
            // 
            // txtValues
            // 
            this.txtValues.Location = new System.Drawing.Point(78, 89);
            this.txtValues.Name = "txtValues";
            this.txtValues.Size = new System.Drawing.Size(237, 20);
            this.txtValues.TabIndex = 20;
            // 
            // labelControl5
            // 
            this.labelControl5.Location = new System.Drawing.Point(10, 43);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(58, 13);
            this.labelControl5.TabIndex = 17;
            this.labelControl5.Text = "Discrete FS:";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(199, 43);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(73, 13);
            this.labelControl2.TabIndex = 18;
            this.labelControl2.Text = "Continuous FS:";
            // 
            // labelControl7
            // 
            this.labelControl7.Location = new System.Drawing.Point(78, 70);
            this.labelControl7.Name = "labelControl7";
            this.labelControl7.Size = new System.Drawing.Size(237, 13);
            this.labelControl7.TabIndex = 19;
            this.labelControl7.Text = "(You can enter more than one,  seperated by \",\")";
            // 
            // labelControl4
            // 
            this.labelControl4.Location = new System.Drawing.Point(18, 92);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(54, 13);
            this.labelControl4.TabIndex = 19;
            this.labelControl4.Text = "List Values:";
            // 
            // btnCancel
            // 
            this.btnCancel.Image = global::FRDB_SQLite.Properties.Resources.small_cancel;
            this.btnCancel.Location = new System.Drawing.Point(329, 446);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 35);
            this.btnCancel.TabIndex = 15;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // frmFuzzySetAction
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(431, 493);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.groupControl2);
            this.Controls.Add(this.groupControl1);
            this.Controls.Add(this.labelControl3);
            this.MaximizeBox = false;
            this.Name = "frmFuzzySetAction";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Fuzzy Sets Action";
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            this.groupControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lbDisFS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lbConFS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).EndInit();
            this.groupControl2.ResumeLayout(false);
            this.groupControl2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboConFS.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDisFS.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtValues.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.GroupControl groupControl1;
        private DevExpress.XtraEditors.ListBoxControl lbDisFS;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.ListBoxControl lbConFS;
        private DevExpress.XtraEditors.LabelControl labelControl6;
        private DevExpress.XtraEditors.GroupControl groupControl2;
        private DevExpress.XtraEditors.CheckedComboBoxEdit cboConFS;
        private DevExpress.XtraEditors.CheckedComboBoxEdit cboDisFS;
        private DevExpress.XtraEditors.SimpleButton btnOK;
        private DevExpress.XtraEditors.TextEdit txtValues;
        private DevExpress.XtraEditors.LabelControl labelControl5;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private DevExpress.XtraEditors.SimpleButton btnCancel;
        private DevExpress.XtraEditors.LabelControl labelControl7;
    }
}