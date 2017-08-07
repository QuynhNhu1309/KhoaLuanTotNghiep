namespace FRDB_SQLite.Gui
{
    partial class frmDisFSRelation
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDisFSRelation));
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.membershipsCol = new DevExpress.XtraGrid.Columns.GridColumn();
            this.valuesCol = new DevExpress.XtraGrid.Columns.GridColumn();
            this.txtLinguistic = new DevExpress.XtraEditors.TextEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.btnOK = new DevExpress.XtraEditors.SimpleButton();
            this.imageCollection1 = new DevExpress.Utils.ImageCollection(this.components);
            this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
            this.btnViewChart = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLinguistic.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageCollection1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupControl1
            // 
            this.groupControl1.Controls.Add(this.gridControl1);
            this.groupControl1.Controls.Add(this.txtLinguistic);
            this.groupControl1.Controls.Add(this.labelControl1);
            this.groupControl1.Location = new System.Drawing.Point(12, 43);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(327, 243);
            this.groupControl1.TabIndex = 0;
            this.groupControl1.Text = "Descrete Fuzzy Set";
            // 
            // gridControl1
            // 
            this.gridControl1.Location = new System.Drawing.Point(5, 73);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(317, 147);
            this.gridControl1.TabIndex = 1;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.membershipsCol,
            this.valuesCol});
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsView.ShowGroupPanel = false;
            this.gridView1.SynchronizeClones = false;
            // 
            // membershipsCol
            // 
            this.membershipsCol.Caption = "Memberships [0, 1]";
            this.membershipsCol.FieldName = "memberships";
            this.membershipsCol.Name = "membershipsCol";
            this.membershipsCol.Visible = true;
            this.membershipsCol.VisibleIndex = 1;
            // 
            // valuesCol
            // 
            this.valuesCol.Caption = "Values";
            this.valuesCol.FieldName = "values";
            this.valuesCol.Name = "valuesCol";
            this.valuesCol.Visible = true;
            this.valuesCol.VisibleIndex = 0;
            // 
            // txtLinguistic
            // 
            this.txtLinguistic.Location = new System.Drawing.Point(116, 36);
            this.txtLinguistic.Name = "txtLinguistic";
            this.txtLinguistic.Size = new System.Drawing.Size(206, 20);
            this.txtLinguistic.TabIndex = 0;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(15, 39);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(75, 13);
            this.labelControl1.TabIndex = 0;
            this.labelControl1.Text = "Linguistic Label:";
            // 
            // btnOK
            // 
            this.btnOK.Image = global::FRDB_SQLite.Properties.Resources.small_OK;
            this.btnOK.Location = new System.Drawing.Point(249, 310);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 30);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // imageCollection1
            // 
            this.imageCollection1.ImageStream = ((DevExpress.Utils.ImageCollectionStreamer)(resources.GetObject("imageCollection1.ImageStream")));
            // 
            // labelControl6
            // 
            this.labelControl6.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl6.Location = new System.Drawing.Point(102, 12);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(155, 19);
            this.labelControl6.TabIndex = 13;
            this.labelControl6.Text = "Descrete Fuzzy Set";
            // 
            // btnViewChart
            // 
            this.btnViewChart.Image = global::FRDB_SQLite.Properties.Resources.chart_bar;
            this.btnViewChart.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.btnViewChart.Location = new System.Drawing.Point(12, 300);
            this.btnViewChart.Name = "btnViewChart";
            this.btnViewChart.Size = new System.Drawing.Size(40, 40);
            this.btnViewChart.TabIndex = 14;
            this.btnViewChart.Click += new System.EventHandler(this.btnViewChart_Click);
            // 
            // frmDisFSRelation
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(351, 352);
            this.Controls.Add(this.btnViewChart);
            this.Controls.Add(this.labelControl6);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupControl1);
            this.MaximizeBox = false;
            this.Name = "frmDisFSRelation";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Descrete Fuzzy Value";
            this.Load += new System.EventHandler(this.frmDisFSRelation_Load);
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            this.groupControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLinguistic.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageCollection1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.GroupControl groupControl1;
        private DevExpress.XtraEditors.TextEdit txtLinguistic;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.SimpleButton btnOK;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraGrid.Columns.GridColumn valuesCol;
        private DevExpress.XtraGrid.Columns.GridColumn membershipsCol;
        private DevExpress.Utils.ImageCollection imageCollection1;
        private DevExpress.XtraEditors.LabelControl labelControl6;
        private DevExpress.XtraEditors.SimpleButton btnViewChart;
    }
}