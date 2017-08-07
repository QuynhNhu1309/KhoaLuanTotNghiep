namespace FRDB_SQLite.Gui
{
    partial class frmMessBoxCreateFN
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
            this.label1 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnCon = new System.Windows.Forms.Button();
            this.btnDis = new System.Windows.Forms.Button();
            this.lbMessage = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(409, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Create new Discrete membership fuzzy set or new Continuous membership fuzzy set ?" +
    "";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(345, 58);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnCon
            // 
            this.btnCon.Location = new System.Drawing.Point(264, 58);
            this.btnCon.Name = "btnCon";
            this.btnCon.Size = new System.Drawing.Size(75, 23);
            this.btnCon.TabIndex = 7;
            this.btnCon.Text = "Continuous";
            this.btnCon.UseVisualStyleBackColor = true;
            this.btnCon.Click += new System.EventHandler(this.btnCon_Click);
            // 
            // btnDis
            // 
            this.btnDis.Location = new System.Drawing.Point(183, 58);
            this.btnDis.Name = "btnDis";
            this.btnDis.Size = new System.Drawing.Size(75, 23);
            this.btnDis.TabIndex = 6;
            this.btnDis.Text = "Discrete";
            this.btnDis.UseVisualStyleBackColor = true;
            this.btnDis.Click += new System.EventHandler(this.btnDis_Click);
            // 
            // lbMessage
            // 
            this.lbMessage.AutoSize = true;
            this.lbMessage.Location = new System.Drawing.Point(13, 16);
            this.lbMessage.Name = "lbMessage";
            this.lbMessage.Size = new System.Drawing.Size(206, 13);
            this.lbMessage.TabIndex = 5;
            this.lbMessage.Text = "This membership fuzzy set does not exists.";
            // 
            // frmMessBoxCreateFN
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(441, 97);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnCon);
            this.Controls.Add(this.btnDis);
            this.Controls.Add(this.lbMessage);
            this.Name = "frmMessBoxCreateFN";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnCon;
        private System.Windows.Forms.Button btnDis;
        private System.Windows.Forms.Label lbMessage;
    }
}