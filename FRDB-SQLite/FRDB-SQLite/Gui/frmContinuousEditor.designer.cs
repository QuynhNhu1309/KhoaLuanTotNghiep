namespace FRDB_SQLite.Gui
{
    partial class frmContinuousEditor
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
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.groupControl2 = new DevExpress.XtraEditors.GroupControl();
            this.txtBottomRight = new DevExpress.XtraEditors.TextEdit();
            this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
            this.txtTopRight = new DevExpress.XtraEditors.TextEdit();
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.txtTopLeft = new DevExpress.XtraEditors.TextEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.txtBottomLeft = new DevExpress.XtraEditors.TextEdit();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.txtLinguistic = new DevExpress.XtraEditors.TextEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.btnOK = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
            this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).BeginInit();
            this.groupControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtBottomRight.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTopRight.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTopLeft.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtBottomLeft.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLinguistic.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // groupControl1
            // 
            this.groupControl1.Controls.Add(this.groupControl2);
            this.groupControl1.Controls.Add(this.txtLinguistic);
            this.groupControl1.Controls.Add(this.labelControl1);
            this.groupControl1.Location = new System.Drawing.Point(12, 46);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(327, 273);
            this.groupControl1.TabIndex = 1;
            this.groupControl1.Text = "Continuous Fuzzy Set";
            // 
            // groupControl2
            // 
            this.groupControl2.Controls.Add(this.txtBottomRight);
            this.groupControl2.Controls.Add(this.labelControl5);
            this.groupControl2.Controls.Add(this.txtTopRight);
            this.groupControl2.Controls.Add(this.labelControl4);
            this.groupControl2.Controls.Add(this.txtTopLeft);
            this.groupControl2.Controls.Add(this.labelControl3);
            this.groupControl2.Controls.Add(this.txtBottomLeft);
            this.groupControl2.Controls.Add(this.labelControl2);
            this.groupControl2.Location = new System.Drawing.Point(5, 62);
            this.groupControl2.Name = "groupControl2";
            this.groupControl2.Size = new System.Drawing.Size(317, 194);
            this.groupControl2.TabIndex = 1;
            this.groupControl2.Text = "X-Coordinates for the FuzzySet";
            // 
            // txtBottomRight
            // 
            this.txtBottomRight.Location = new System.Drawing.Point(120, 153);
            this.txtBottomRight.Name = "txtBottomRight";
            this.txtBottomRight.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            this.txtBottomRight.Size = new System.Drawing.Size(163, 20);
            this.txtBottomRight.TabIndex = 4;
            // 
            // labelControl5
            // 
            this.labelControl5.Location = new System.Drawing.Point(47, 155);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(67, 13);
            this.labelControl5.TabIndex = 0;
            this.labelControl5.Text = "Bottom-Right:";
            // 
            // txtTopRight
            // 
            this.txtTopRight.Location = new System.Drawing.Point(120, 115);
            this.txtTopRight.Name = "txtTopRight";
            this.txtTopRight.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            this.txtTopRight.Size = new System.Drawing.Size(163, 20);
            this.txtTopRight.TabIndex = 3;
            // 
            // labelControl4
            // 
            this.labelControl4.Location = new System.Drawing.Point(53, 118);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(51, 13);
            this.labelControl4.TabIndex = 0;
            this.labelControl4.Text = "Top-Right:";
            // 
            // txtTopLeft
            // 
            this.txtTopLeft.Location = new System.Drawing.Point(120, 78);
            this.txtTopLeft.Name = "txtTopLeft";
            this.txtTopLeft.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            this.txtTopLeft.Size = new System.Drawing.Size(163, 20);
            this.txtTopLeft.TabIndex = 2;
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(53, 81);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(45, 13);
            this.labelControl3.TabIndex = 0;
            this.labelControl3.Text = "Top-Left:";
            // 
            // txtBottomLeft
            // 
            this.txtBottomLeft.Location = new System.Drawing.Point(120, 39);
            this.txtBottomLeft.Name = "txtBottomLeft";
            this.txtBottomLeft.Properties.AllowNullInput = DevExpress.Utils.DefaultBoolean.True;
            this.txtBottomLeft.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.txtBottomLeft.Properties.ExportMode = DevExpress.XtraEditors.Repository.ExportMode.Value;
            this.txtBottomLeft.Properties.Mask.BeepOnError = true;
            this.txtBottomLeft.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            this.txtBottomLeft.ShowToolTips = false;
            this.txtBottomLeft.Size = new System.Drawing.Size(163, 20);
            this.txtBottomLeft.TabIndex = 1;
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(47, 42);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(61, 13);
            this.labelControl2.TabIndex = 0;
            this.labelControl2.Text = "Bottom-Left:";
            // 
            // txtLinguistic
            // 
            this.txtLinguistic.Location = new System.Drawing.Point(125, 31);
            this.txtLinguistic.Name = "txtLinguistic";
            this.txtLinguistic.Size = new System.Drawing.Size(183, 20);
            this.txtLinguistic.TabIndex = 0;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(28, 34);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(75, 13);
            this.labelControl1.TabIndex = 0;
            this.labelControl1.Text = "Linguistic Label:";
            // 
            // btnOK
            // 
            this.btnOK.Image = global::FRDB_SQLite.Properties.Resources.small_OK;
            this.btnOK.Location = new System.Drawing.Point(137, 325);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 30);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Image = global::FRDB_SQLite.Properties.Resources.small_cancel;
            this.btnCancel.Location = new System.Drawing.Point(249, 325);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 30);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // labelControl6
            // 
            this.labelControl6.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl6.Location = new System.Drawing.Point(64, 12);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(228, 19);
            this.labelControl6.TabIndex = 12;
            this.labelControl6.Text = "Continuous Fuzzy Set Editor";
            // 
            // frmContinuousEditor
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(351, 362);
            this.Controls.Add(this.labelControl6);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupControl1);
            this.Controls.Add(this.btnCancel);
            this.MaximizeBox = false;
            this.Name = "frmContinuousEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Continuous Fuzzy Value Editor";
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            this.groupControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).EndInit();
            this.groupControl2.ResumeLayout(false);
            this.groupControl2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtBottomRight.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTopRight.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTopLeft.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtBottomLeft.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLinguistic.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.GroupControl groupControl1;
        private DevExpress.XtraEditors.TextEdit txtLinguistic;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.SimpleButton btnOK;
        private DevExpress.XtraEditors.SimpleButton btnCancel;
        private DevExpress.XtraEditors.GroupControl groupControl2;
        private DevExpress.XtraEditors.TextEdit txtBottomRight;
        private DevExpress.XtraEditors.LabelControl labelControl5;
        private DevExpress.XtraEditors.TextEdit txtTopRight;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private DevExpress.XtraEditors.TextEdit txtTopLeft;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.TextEdit txtBottomLeft;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.LabelControl labelControl6;
    }
}