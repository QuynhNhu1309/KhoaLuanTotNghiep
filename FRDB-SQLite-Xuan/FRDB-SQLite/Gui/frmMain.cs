using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.Skins;
using DevExpress.LookAndFeel;
using DevExpress.UserSkins;
using DevExpress.XtraEditors;
using DevExpress.XtraBars.Helpers;
using System.Timers;
using DevExpress.Utils.Menu;
using System.Threading;
using FRDB_SQLite;
using FRDB_SQLite;
using FRDB_SQLite;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using FRDB_SQLite.Class;
using DevExpress.XtraBars.Ribbon;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace FRDB_SQLite.Gui
{
    public partial class frmMain : XtraForm
    {
        public frmMain()
        {
            InitializeComponent();
            InitSkinGallery();
            //bool i = false && false != true && true || false;//false why?
            //bool j = false && true != true && true || true;// true
            //bool h = false && false != false && true || true || false;
            //MessageBox.Show(h.ToString());//
            ContextMenu_Database.Items[0].Visible = false;
            
        }

        #region 0. Declare

        ////////////////////////////////////////////////////////////////////////////
        ///fuzzy database object
        ////////////////////////////////////////////////////////////////////////////
        private FdbEntity fdbEntity;
        private FzSchemeEntity newScheme, currentScheme;
        private FzRelationEntity currentRelation, newRelation, renamedRelation;
        private FzQueryEntity currentQuery;
        private FzDiscreteFuzzySetEntity currentDis;//edit
        private FzContinuousFuzzySetEntity currentCon;
        private String path = "";
        ////////////////////////////////////////////////////////////////////////////
        ///BLL object
        ////////////////////////////////////////////////////////////////////////////
        private FdbBLL fdbBll;
        private ThreadBLL threadBLL;

        ////////////////////////////////////////////////////////////////////////////
        ///Tree List
        ////////////////////////////////////////////////////////////////////////////
        //TreeListNode parentFdbNode, childSchemeNode, childRelationNode, childQueryNode, childCurrentNode, childNewNode;
        private TreeNode parentFdbNode, childSchemeNode, childRelationNode, childQueryNode, childCurrentNode, childNewNode, childDiscreteFNNode, childContinuousFNNode;//edit
        public struct ImageTree
        {
            public int unselectedState;
            public int selectedState;
        }
        private ImageTree parentFdbImageTree, folderImageTree, schemeImageTree, relationImageTree, queryImageTree,  discreteFNImageTree,continuousFNImageTree;    //edit

        ////////////////////////////////////////////////////////////////////////////
        ///other object and declare
        ////////////////////////////////////////////////////////////////////////////
        private System.Timers.Timer timer;
        private int currentRow, currentCell;
        private Boolean flag, validated, rollbackCell;
        
        #endregion

        #region 1. Home Ribbon Page

        private void CreateFuzzyDatabase()
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Create New Fuzzy Relational Database (FRDB)";
                sfd.Filter = "Database file (*.tbb) | *.tbb | All files (*.*) | *.* ";
                sfd.DefaultExt = "tbb";
                sfd.AddExtension = true;
                sfd.RestoreDirectory = true;
                sfd.InitialDirectory = FdbBLL.GetRootPath(AppDomain.CurrentDomain.BaseDirectory.ToString());
                
                sfd.SupportMultiDottedExtensions = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    //Todo code here
                    siStatus.Caption = "Creating new blank fuzzy database...";
                    fdbEntity = null;

                    NewFuzzyDatabaseEntity(sfd.FileName);

                    if (!fdbBll.CreateFuzzyDatabase(fdbEntity))
                    {
                        MessageBox.Show("Cannot create new blank fuzzy database!");
                    }
                    else
                    {
                        ShowTreeList();//Create successfully and show treeList
                        ShowTreeListNode();
                        ActiveDatabase(true);
                        iNewDatabase.Enabled = false;
                        iOpenExistingDatabase.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                DialogResult result = MessageBox.Show("You haven't installed SQLite yet, do you want to install SQLite right now?", "SQLite"
                    + fdbEntity.FdbName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    Process.Start("SQLite-1.0.66.0-setup.exe");
                }
            }
        }

        private void OpenFuzzyDatabase()
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.DefaultExt = "tbb";
                ofd.CheckFileExists = true;
                ofd.Filter = "Fuzzy Database File (*.tbb) | *.tbb";
                ofd.AddExtension = true;
                ofd.Multiselect = false;
                ofd.RestoreDirectory = true;
                ofd.Title = "Open Fuzzy Database...";
                
                String tmp = ReadPath();// Read the path
                if (tmp != "" && Directory.Exists(tmp))
                    ofd.InitialDirectory = tmp;
                else
                    ofd.InitialDirectory = FdbBLL.GetRootPath(AppDomain.CurrentDomain.BaseDirectory.ToString());

                ofd.SupportMultiDottedExtensions = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    siStatus.Caption = "Opening fuzzy database...";

                    NewFuzzyDatabaseEntity(ofd.FileName);
                    this.path = ofd.FileName;
                    WritePath(this.path.Substring(0, this.path.LastIndexOf("\\")));//Save the last path

                    threadBLL = new ThreadBLL(DBValues.connString);
                    threadBLL.WorkerThread = new Thread(new ThreadStart(threadBLL.StartWorker));
                    threadBLL.WorkerThread.Name = "Database Client Worker Thread";
                    threadBLL.WorkerThread.Start();
                    Cursor oldCursor = Cursor;
                    Cursor = Cursors.WaitCursor;
                    frmProgressBar frm = new frmProgressBar();
                    frm.Show();
                    frm.Refresh();
                    Boolean success = threadBLL.Connecting();
                    success = success && fdbBll.OpenFuzzyDatabase(fdbEntity);
                    frm.Close();
                    Cursor = oldCursor;

                    if (!success)
                    {
                        threadBLL.Dispose();
                        throw new Exception("ERROR:\n Can not connect to fuzzy database!");
                    }
                    else
                    {
                        ShowTreeList();
                        ShowTreeListNode();
                        ActiveDatabase(true);
                        iOpenExistingDatabase.Enabled = false;
                        iNewDatabase.Enabled = false;
                        threadBLL.Dispose();
                    }
                }

                ofd.Dispose();
            }
            catch (Exception ex)
            {
                DialogResult result = MessageBox.Show("You haven't installed SQLite yet, do you want to install SQLite right now?", "SQLite"
                   + fdbEntity.FdbName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    Process.Start("SQLite-1.0.66.0-setup.exe");
                }
            }
        }

        private void SaveFuzzyDatabase()
        {
            try
            {
                // Record to database
                Cursor oldCursor = Cursor;
                Cursor = Cursors.WaitCursor;
                frmProgressBar frm = new frmProgressBar();
                frm.LblName.Text = "Saving...";
                frm.Show();
                frm.Refresh();
               
                fdbBll = new FdbBLL();
                
                fdbBll.DropFuzzyDatabase(fdbEntity);
                if (!fdbBll.SaveFuzzyDatabase(fdbEntity))//Why fdbEntity doesn't null? Because it was created in  OpenFuzzyDatabase or CreateFuzzyDatabase
                {
                    siStatus.Caption = "Cannnot save the Database!";
                    timer.Start();
                }
                else
                {
                    siStatus.Caption = "The Database has been saved!";
                    timer.Start();
                }

                frm.Close();
                Cursor = oldCursor;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR:\n" + ex.Message);
            }
        }

        private void SaveFuzzyDatabaseAs()
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.DefaultExt = "tbb";                                                                   // Default extension
                sfd.Filter = "Fuzzy Database File (*.tbb)|*.tbb|All files (*.*)|*.*";              // add extension to dialog
                sfd.AddExtension = true;                                                                // enable adding extension
                sfd.RestoreDirectory = true;                                                           // Automatic restore path for another time
                sfd.Title = "Save as...";
                sfd.InitialDirectory = FdbBLL.GetRootPath(AppDomain.CurrentDomain.BaseDirectory.ToString());
                sfd.SupportMultiDottedExtensions = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    siStatus.Caption = "Saving Database...";

                    //NewFuzzyDatabaseEntity(sfd.FileName);
                    Clone(sfd.FileName);
                    this.path = sfd.FileName;

                    Cursor oldCursor = Cursor;
                    Cursor = Cursors.WaitCursor;

                    frmProgressBar frm = new frmProgressBar();
                    frm.LblName.Text = "Saving...";
                    frm.Show();
                    frm.Refresh();
                    //if (!fdbBll.SaveFuzzyDatabaseAs(fdbEntity))//Why doesn't fdbEntity null? Because it was created in  OpenFuzzyDatabase or CreateFuzzyDatabase
                    //{
                    //    siStatus.Caption = "Cannnot save the database!";
                    //    timer.Start();
                    //}
                    //else
                    //{
                    //    siStatus.Caption = "The database has been saved!";
                    //    timer.Start();
                    //}

                    frm.Close();
                    Cursor = oldCursor;
                    ShowTreeList();
                    ShowTreeListNode();
                    //Some enable control
                }
                
                sfd.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CloseFuzzyDatabase()
        {
            try
            {
                DialogResult result = MessageBox.Show("Close current fuzzy database ?", "Close Fuzzy Database " 
                    + fdbEntity.FdbName , MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    treeList1.Nodes.Clear();
                    fdbEntity = null;
                    CloseCurrentRelation();
                    AddRowDefault();        
                    ResetObject();
                    ActiveDatabase(false);
                    iOpenExistingDatabase.Enabled = true;
                    iNewDatabase.Enabled = true;
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        private void Clone(String path)
        {
            timer.Start();
            FdbBLL bll = new FdbBLL();
            FdbEntity newDatabase = new FdbEntity(path);
            bll.CreateFuzzyDatabase(newDatabase);
            newDatabase.Schemes = fdbEntity.Schemes;
            newDatabase.Relations = fdbEntity.Relations;
            newDatabase.Queries = fdbEntity.Queries;
            newDatabase.DiscreteFuzzyNumbers = fdbEntity.DiscreteFuzzyNumbers;
            newDatabase.ContinuousFuzzyNumbers = fdbEntity.ContinuousFuzzyNumbers;
            fdbEntity = null;
            DBValues.connString = newDatabase.ConnString;
            DBValues.dbName = newDatabase.FdbName;

            parentFdbNode.Text = DBValues.dbName.ToUpper();
            parentFdbNode.ToolTipText = "Database " + newDatabase.FdbName;
            fdbEntity = newDatabase;
        }

        private void NewFuzzyDatabaseEntity(String path)
        {
            timer.Start();

            fdbEntity = null;

            fdbBll = new FdbBLL();
            fdbEntity = new FdbEntity(path);
            this.path = path;

            DBValues.connString = fdbEntity.ConnString;
            DBValues.dbName = fdbEntity.FdbName;
        }

        private void iNewDatabase_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            CreateFuzzyDatabase();
        }

        private void iOpenExistingDatabase_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenFuzzyDatabase();
        }

        private void iSave_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SaveFuzzyDatabase();
        }

        private void iSaveAs_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SaveFuzzyDatabaseAs();
        }

        private void iCloseDatabase_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            CloseFuzzyDatabase();
        }

        private void iRefreshDatabase_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (treeList1.Nodes.Count > 0)
            {
                ShowTreeList();
                ShowTreeListNode();
            }
        }

        private void iConnectDatabase_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (fdbEntity != null && FdbBLL.CheckConnection(fdbEntity))
            {
                MessageBox.Show("OK","Connection is OK!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Connection is FAIL!");
            }
        }

        private void iExit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ClosingForm();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
                ClosingForm();
            else
            { }
        }

        private void ClosingForm()
        { 
            try
            {
                if (fdbEntity != null)
                {
                    DialogResult result = MessageBox.Show("Do you want to save any change to database?", "Save changed", MessageBoxButtons.YesNoCancel);
                    if (result == DialogResult.Yes)
                    {
                        SaveFuzzyDatabase();
                        Application.Exit();
                    }
                    else if (result == DialogResult.No)
                        Application.Exit();
                    else
                    {
                        return;
                    }
                }
                else
                {
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can not save the database because of null values!", "ERROR", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            }
            
        }

        private void OnTimeEvent(Object sender, ElapsedEventArgs e)
        {
            siStatus.Caption = "Ready";
        }

        private String ReadPath()
        {
            String result = "";
            try
            {
                //FileStream fs = new FileStream("previous_path.txt", FileMode.Open);
                //StreamReader sr = new StreamReader(fs);
                //while (!sr.EndOfStream)
                //    result = sr.ReadLine();
                //fs.Close();
                using (StreamReader reader = new StreamReader("previous_path.txt"))
                {
                    result = reader.ReadLine();
                }
                return result;
            }
            catch (Exception ex)
            { return result; }
        }

        private void WritePath(String path)
        {
            try
            {
                //FileStream fs = new FileStream("previous_path.txt", FileMode.Create, FileAccess.Write);
                //StreamWriter sw = new StreamWriter(fs);
                //sw.WriteLine(path);
                //sw.Close();
                //fs.Close();
                using (StreamWriter writer = new StreamWriter("previous_path.txt"))
                {
                    writer.WriteLine(path);
                    writer.Close();
                }
            }
            catch (Exception ex)
            { }
        }

        #endregion
            #region Context Menu Fuzzy Database
        private void CTMenuDB_CloseDB_Click(object sender, EventArgs e)
        {
            CloseFuzzyDatabase();
        }

        private void CTMenuDB_Rename_Click(object sender, EventArgs e)
        {
            try
            {
                if (fdbEntity == null)
                {
                    MessageBox.Show("Current database is empty!");
                    return;
                }
                frmNewName frm = new frmNewName(1);
                frm.ShowDialog();
                if (frm.Name != null)
                {
                    //Save the database
                    DialogResult result = MessageBox.Show("Do you want to save all changed to the database ?", "Close Fuzzy Database "
                   + fdbEntity.FdbName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        new FdbBLL().DropFuzzyDatabase(fdbEntity);
                        new FdbBLL().SaveFuzzyDatabase(fdbEntity);
                    }

                    //Close the databse
                    treeList1.Nodes.Clear();
                    fdbEntity = null;
                    ResetObject();
                    ActiveDatabase(false);
                    iOpenExistingDatabase.Enabled = true;
                    iNewDatabase.Enabled = true;

                    //Change the name of the database
                    String path = "";
                    int length = this.path.LastIndexOf("\\");

                    path = this.path.Substring(0, length);
                    path += "\\" + frm.Name + ".tbb";

                    System.IO.File.Move(this.path, path);

                    //ReOpen the database
                    NewFuzzyDatabaseEntity(path);
                    if (new FdbBLL().OpenFuzzyDatabase(fdbEntity))
                    {
                        ShowTreeList();
                        ShowTreeListNode();
                        ActiveDatabase(true);
                        iOpenExistingDatabase.Enabled = false;
                        iNewDatabase.Enabled = false;
                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region 2. Scheme Ribbon Page

        private void CreateNewBlankScheme()
        {
            try
            {
                if (fdbEntity == null)
                {
                    MessageBox.Show("ERROR:\n Haven't loaded Database yet!");
                    return;
                }

                DBValues.schemesName = FzSchemeBLL.GetListSchemeName(fdbEntity);
                frmSchemeEditor frm = new frmSchemeEditor();
                frm.ShowDialog();

                SchemeEditor(frm.CreateScheme, frm.OpenScheme, frm.DelecteScheme);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR:\n"+ ex.Message);
            }
        }

        private void OpenScheme()
        {
            try
            {
                if (fdbEntity == null)
                {
                    MessageBox.Show("ERROR:\n Haven't loaded Database yet!");
                    return;
                }

                DBValues.schemesName = FzSchemeBLL.GetListSchemeName(fdbEntity);
                frmSchemeEditor frm = new frmSchemeEditor();
                frm.ShowDialog();

                SchemeEditor(frm.CreateScheme, frm.OpenScheme, frm.DelecteScheme);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR:\n" + ex.Message);
            }
        }

        private void SaveScheme()
        {
            try
            {
                if (!CheckSchemeData()) return;

                ///Current scheme is null that mean you dont open any schemes (the GridView is empty)
                ///Save new scheme OR save  attributes to an existed scheme
                ///Save new scheme: Save scheme name and list attributes
                ///Save to an existed scheme: Update list attributes (if it is not inherited)
                if (AllowSavingNewScheme())
                {
                    DBValues.schemesName = FzSchemeBLL.GetListSchemeName(fdbEntity);
                    frmSaveScheme frm = new frmSaveScheme();
                    frm.ShowDialog();

                    if (frm.SchemeName != String.Empty)
                    {
                        newScheme = FzSchemeBLL.GetSchemeByName(frm.SchemeName, fdbEntity);

                        if (newScheme == null)///Mean the scheme doesn't exist in the database. Saving scheme name
                        {
                            AddSchemeNode(frm.SchemeName);//Add scheme name to tree node and database
                        }
                        
                        currentScheme = newScheme;
                        SaveCurrentScheme(frm.SchemeName);///Clear all attributes and update
                    }
                }
                ///This is exact "currentScheme"
                ///Save current scheme opened on GridView
                ///Check the inheritance (checked in SaveCurrentScheme())
                else
                {
                    String schemeName = currentScheme.SchemeName;
                    SaveCurrentScheme(schemeName);
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR:\n" + ex.Message);
            }
        }

        private void DeleteScheme()
        {
            try
            {
                if (fdbEntity == null)
                {
                    MessageBox.Show("ERROR:\n Haven't loaded Database yet!");
                    return;
                }

                DBValues.schemesName = FzSchemeBLL.GetListSchemeName(fdbEntity);
                frmSchemeEditor frm = new frmSchemeEditor();
                frm.ShowDialog();

                SchemeEditor(frm.CreateScheme, frm.OpenScheme, frm.DelecteScheme);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR:\n" + ex.Message);
            }
        }

        private void SaveCurrentScheme(String schemeName)///mean also save its attributes
        {
            if (FzSchemeBLL.IsInherited(currentScheme, fdbEntity.Relations))//Or check the readOnly on GridView
            {
                MessageBox.Show("Current Scheme is opened and \ninherited by some relations!");
                return;
            }

            xtraTabDatabase.TabPages[0].Text = "Scheme " + schemeName;
            xtraTabDatabase.SelectedTabPage = xtraTabDatabase.TabPages[0]; ;
            currentScheme.Attributes.Clear();
            GridViewDesign.CurrentCell = GridViewDesign.Rows[GridViewDesign.Rows.Count - 1].Cells[0];

            for (int i = 0; i < GridViewDesign.Rows.Count - 1; i++)// The end row is new row
            {
                Boolean primaryKey = Convert.ToBoolean(GridViewDesign.Rows[i].Cells[0].Value);
                String attributeName = GridViewDesign.Rows[i].Cells[1].Value.ToString();
                String typeName = GridViewDesign.Rows[i].Cells[2].Value.ToString();
                String description = (GridViewDesign.Rows[i].Cells[4].Value == null ? "" : GridViewDesign.Rows[i].Cells[4].Value.ToString());
                String domain = (GridViewDesign.Rows[i].Cells[3].Value.ToString());
                
                FzDataTypeEntity dataType = new FzDataTypeEntity(typeName, domain);
                FzAttributeEntity attribute = new FzAttributeEntity(primaryKey, attributeName, dataType, description);

                currentScheme.Attributes.Add(attribute);
            }

            if (GridViewDesign.Rows[GridViewDesign.Rows.Count - 2].Cells[1].Value.ToString() != "µ")
                AddMembership();
            
            MessageBox.Show("Current Scheme is saved OK!");
        }

        private void AddMembership()
        {
            MessageBox.Show("The default membership attribute \nwill be added automatically to this scheme", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Boolean primaryKey = false;
            String attributeName = "µ";
            String typeName = "Double";
            String description = "";
            String domain = "[5.0 x 10^-324  ...  1.7 x 10^308]";

            FzDataTypeEntity dataType = new FzDataTypeEntity(typeName, domain);
            FzAttributeEntity attribute = new FzAttributeEntity(primaryKey, attributeName, dataType, description);
            currentScheme.Attributes.Add(attribute);
        }

        private void SchemeEditor(String create, String open, String delete)
        {
            if (create != null)
            {
                AddSchemeNode(create);
                if (MessageBox.Show("Add attributes to this scheme?", "Add attributess", MessageBoxButtons.YesNo)
                    == DialogResult.Yes)
                {
                    currentScheme = FzSchemeBLL.GetSchemeByName(create, fdbEntity);
                    AddRowDefault();
                    xtraTabDatabase.TabPages[0].Text = "Scheme " + create;
                    xtraTabDatabase.SelectedTabPage = xtraTabDatabase.TabPages[0];
                }
            }
            if (open != null)
            {
                currentScheme = FzSchemeBLL.GetSchemeByName(open, fdbEntity);
                OpenScheme(currentScheme);
            }
            if (delete != null)
            {
                FzSchemeEntity delScheme = FzSchemeBLL.GetSchemeByName(delete, fdbEntity);
                DeleteScheme(delScheme);
            }
        }

        private void OpenScheme(FzSchemeEntity currentScheme)
        {
            //is not contented any attributes, need to add some attributes
            if (currentScheme.Attributes.Count == 0)
            {
                AddRowDefault();
                MessageBox.Show("There are no attribute in this scheme, let create some attributes!");
                xtraTabDatabase.TabPages[0].Text = "Create Attribute to Scheme " + currentScheme.SchemeName;
                xtraTabDatabase.SelectedTabPage = xtraTabDatabase.TabPages[0];
                
                UnsetReadOnlyGridView();
            }
            else//is contented, show text and attributes
            {
                xtraTabDatabase.TabPages[0].Text = "Scheme " + currentScheme.SchemeName;
                xtraTabDatabase.SelectedTabPage = xtraTabDatabase.TabPages[0];

                if (FzSchemeBLL.IsInherited(currentScheme, fdbEntity.Relations))
                {
                    SetReadOnlyGridView();//To prevent add attributes to current scheme
                }
                else UnsetReadOnlyGridView();

                ///Finally, show list attributes
                ShowAttribute();
            }
        }

        private void DeleteScheme(FzSchemeEntity delScheme)
        {
            if (FzSchemeBLL.IsInherited(delScheme, fdbEntity.Relations))
            {
                MessageBox.Show("Scheme is being inherited!");
            }
            else
            {
                DialogResult result = new DialogResult();
                result = MessageBox.Show("Delete this scheme ?", "Delete scheme " + delScheme.SchemeName, MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    if (currentScheme != null && delScheme.Equals(currentScheme))
                    {
                        //CloseCurrentScheme();
                        AddRowDefault();
                    }

                    DeleteTreeNode(delScheme.SchemeName, delScheme, null, null,null,null);
                }
            }
        }

        private void AddSchemeNode(String schemeName)
        {
            newScheme = new FzSchemeEntity(schemeName);
            fdbEntity.Schemes.Add(newScheme);

            TreeNode tempNode = new TreeNode();
            tempNode.Name = schemeName;
            tempNode.Text = schemeName;
            tempNode.ToolTipText = "Scheme " + schemeName;
            tempNode.ContextMenuStrip = ContextMenu_SchemaNode;
            tempNode.ImageIndex = schemeImageTree.selectedState;
            tempNode.SelectedImageIndex = schemeImageTree.unselectedState;
            childSchemeNode.Nodes.Add(tempNode);
        }

        private Boolean AllowSavingNewScheme()
        {
            if (currentScheme == null || xtraTabDatabase.TabPages[0].Text.Length <= 7) 
                return true;
            return false;
        }

        private void SetReadOnlyGridView()
        {
            GridViewDesign.Columns[0].ReadOnly = true;
            GridViewDesign.Columns[1].ReadOnly = true;
            GridViewDesign.Columns[2].ReadOnly = true;
        }

        private void UnsetReadOnlyGridView()
        {
            GridViewDesign.Columns[0].ReadOnly = false;
            GridViewDesign.Columns[1].ReadOnly = false;
            GridViewDesign.Columns[2].ReadOnly = false;
        }

        private void ShowAttribute()
        {
            int n = GridViewDesign.Rows.Count - 2;
            for (int i = n; i >= 0; i--)
            {
                GridViewDesign.Rows.Remove(GridViewDesign.Rows[i]);
            }
            toolStripLabel1.Text = lblDesignRowNumberIndicator.Text = "1 / 1";

            CheckBox chkbox;

            for (int i = 0; i < currentScheme.Attributes.Count - 1; i++)
            {
                FzAttributeEntity attr = currentScheme.Attributes[i];
                GridViewDesign.Rows.Add();
                chkbox = new CheckBox();
                chkbox.Checked = attr.PrimaryKey;
                GridViewDesign.Rows[i].Cells[0].Value = chkbox.CheckState;
                GridViewDesign.Rows[i].Cells[1].Value = attr.AttributeName;
                GridViewDesign.Rows[i].Cells[2].Value = attr.DataType.TypeName;
                GridViewDesign.Rows[i].Cells[3].Value = attr.DataType.DomainString;
                GridViewDesign.Rows[i].Cells[4].Value = (attr.Description != null ? attr.Description : null);
            }
           
           // GridViewDesign.CurrentCell = GridViewDesign.Rows[j].Cells[0];

            if (GridViewDesign.CurrentRow != null)
            {
                toolStripLabel1.Text = lblDesignRowNumberIndicator.Text = (GridViewDesign.CurrentRow.Index + 1).ToString() + " / " + GridViewDesign.Rows.Count.ToString();
            }
            else toolStripLabel1.Text = lblDesignRowNumberIndicator.Text = "1 / " + GridViewDesign.Rows.Count.ToString();
        }

        private bool CheckSchemeData()
        {
            if (fdbEntity == null)
            {
                MessageBox.Show("Please load database before you add any schemes!");
                return false;
            }

            if (currentScheme != null && FzSchemeBLL.IsInherited(currentScheme, fdbEntity.Relations))
            {
                MessageBox.Show("Current scheme is inherited, close this scheme and create new scheme!");
                return false;
            }

            ///GridViewDesign empty, nothing to save -> return
            if (GridViewDesign.Rows.Count <= 1)
            {
                MessageBox.Show("Please enter some attributes!");
                return false;
            }
            for (int i = 0; i < GridViewDesign.Rows.Count - 1; i++)
            {
                if (GridViewDesign.Rows[i].Cells[1].Value == null)
                {
                    MessageBox.Show("Attributes name is require at row[" + i + "]"); return false;
                }
                if (GridViewDesign.Rows[i].Cells[2].Value == null)
                {
                    MessageBox.Show("Datatype is require at row[" + i + "]"); return false;
                }
            }
            return true;
        }

        private void iNewScheme_ItemClick(object sender, EventArgs e)// DevExpress.XtraBars.ItemClickEventArgs e)
        {
            CreateNewBlankScheme();
        }

        private void iOpenScheme_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenScheme();
        }

        private void iSaveScheme_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SaveScheme();
        }

        private void iDeleteScheme_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DeleteScheme();
        }

        private void iCloseCurrentScheme_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            currentScheme = null;
            AddRowDefault();
        }

        #endregion
            #region Context Menu Scheme
        private void CTMenuSchema_NewSchema_Click(object sender, EventArgs e)
        {
            CreateNewBlankScheme();
        }

        private void CTMenuSchNode_OpenSchema_Click(object sender, EventArgs e)
        {
            String schemeName = childCurrentNode.Name;
            treeList1.SelectedNode = childCurrentNode;
            currentScheme = FzSchemeBLL.GetSchemeByName(schemeName, fdbEntity);
            OpenScheme(currentScheme);
        }

        private void CTMenuSchNode_DeleteSchema_Click(object sender, EventArgs e)
        {
            try
            {
                String schemeName = childCurrentNode.Name;
                FzSchemeEntity deleteScheme = FzSchemeBLL.GetSchemeByName(schemeName, fdbEntity);

                DeleteScheme(deleteScheme);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void renameSchemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                String oldName = childCurrentNode.Text;
                FzSchemeEntity newScheme = FzSchemeBLL.GetSchemeByName(oldName, fdbEntity);
                currentScheme = newScheme;

                if (FzSchemeBLL.IsInherited(newScheme, fdbEntity.Relations))
                {
                    MessageBox.Show("This scheme is inherited by some relation!");
                    return;
                }
                DBValues.schemesName = FzSchemeBLL.GetListSchemeName(fdbEntity);
                frmNewName frm = new frmNewName(3);
                frm.ShowDialog();
                if (frm.Name == null) return;
                if (currentScheme != null && newScheme.Equals(currentScheme))
                {
                    if (xtraTabDatabase.TabPages[1].Text.Contains("Create Relation"))
                        xtraTabDatabase.TabPages[1].Text = "Create Relation " + frm.Name;
                    else
                        xtraTabDatabase.TabPages[1].Text = "Relation " + frm.Name;
                    childCurrentNode.Name = childCurrentNode.Text = frm.Name;
                }
                else
                    childCurrentNode.Name = childCurrentNode.Text = frm.Name;
                //Save to database
                FzSchemeBLL.RenameScheme(oldName, frm.Name, fdbEntity);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void deleteAllSchemesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!FzSchemeBLL.IsInherited(fdbEntity))
                {
                    MessageBox.Show("Some scheme is being inherited\n Can not delete all schemes");
                    return;
                }
                DBValues.schemesName = FzSchemeBLL.GetListSchemeName(fdbEntity);

                foreach (var item in DBValues.schemesName)
                {
                    FzSchemeEntity tmp = new FzSchemeEntity();
                    tmp = FzSchemeBLL.GetSchemeByName(item, fdbEntity);
                    DeleteTreeNode(item, tmp, null, null,null,null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region 3. Relation Ribbon Page

        private void CreateNewRelation()
        {
            try
            {
                if (fdbEntity == null)
                {
                    MessageBox.Show("Current database is empty! Please open!");
                    return;
                }

                if (fdbEntity.Schemes.Count == 0)
                {
                    MessageBox.Show("You must create scheme before creating any relations!");
                    return;
                }

                DBValues.schemesName = FzSchemeBLL.GetListSchemeName(fdbEntity);
                DBValues.relationsName = FzRelationBLL.GetListRelationName(fdbEntity);

                frmRelationEditor frm = new frmRelationEditor();
                frm.ShowDialog();

                RelationEditor(frm.CreateRelation, frm.OpenRelation, frm.DeleteRelation, frm.SchemeName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR:\n" + ex.Message);
            }
        }

        private void OpenRelation()
        {
            try
            {
                if (fdbEntity == null)
                {
                    MessageBox.Show("Current database is empty! Please open!");
                    return;
                }

                DBValues.schemesName = FzSchemeBLL.GetListSchemeName(fdbEntity);
                DBValues.relationsName = FzRelationBLL.GetListRelationName(fdbEntity);

                frmRelationEditor frm = new frmRelationEditor();
                frm.ShowDialog();

                RelationEditor(frm.CreateRelation, frm.OpenRelation, frm.DeleteRelation, frm.SchemeName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR:\n" + ex.Message);
            }
        }

        private void SaveRelation()
        {
            try
            {
                if (!CheckRelationData()) return;
                ///Current relation is assign when we create new relation, so it not null
                ///DBValues.schemesName = FzSchemeBLL.GetListSchemeName(fdbEntity);
                String RelationName = currentRelation.RelationName;
                xtraTabDatabase.TabPages[1].Text = "Relation " + RelationName;
                xtraTabDatabase.SelectedTabPage = xtraTabDatabase.TabPages[1]; ;

                SaveTuples(currentRelation);
                
                
                MessageBox.Show("Relation is saved OK!");
            }
            catch (Exception ex)
            {
                //MessageBox.Show("ERROR:\n" + ex.Message);
            }
        }

        private void DeleteRelation()
        {
            try
            {
                if (fdbEntity == null)
                {
                    MessageBox.Show("Current database is empty! Please open!");
                    return;
                }

                DBValues.schemesName = FzSchemeBLL.GetListSchemeName(fdbEntity);
                DBValues.relationsName = FzRelationBLL.GetListRelationName(fdbEntity);
                frmRelationEditor frm = new frmRelationEditor();
                frm.ShowDialog();

                RelationEditor(frm.CreateRelation, frm.OpenRelation, frm.DeleteRelation, frm.SchemeName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR:\n" + ex.Message);
            }
        }

        private void RelationEditor(String create, String open, String delete, String schemeName)
        {
            if (create != String.Empty)
            {
                AddRelationNode(create, schemeName);//Also add referenced scheme to Relation and add Relation to DB

            }
            if (open != String.Empty)
            {
                currentRelation = FzRelationBLL.GetRelationByName(open, fdbEntity);

                ///Show the columns attributes of current relation in order to add values
                ShowColumnsAttribute(currentRelation);

                ///Add tuples to relation
                ShowTuples(currentRelation);
            }
            if (delete != String.Empty)
            {
                FzRelationEntity deleteRelation = FzRelationBLL.GetRelationByName(delete, fdbEntity);

                DialogResult result = new DialogResult();
                result = MessageBox.Show("Delete this relation ?", "Delete relation " + delete, MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    if (currentRelation != null)
                    {
                        if (deleteRelation.Equals(currentRelation))
                        {
                            xtraTabDatabase.TabPages[1].Text = "Relation";
                            GridViewData.Rows.Clear();
                            GridViewData.Columns.Clear();
                            UpdateDataRowNumber();
                        }
                    }

                    ///Finally, remove NodeRelation and Relation in DB
                    TreeNode deletedNode = childRelationNode.Nodes[delete];
                    deletedNode.Remove();
                    fdbEntity.Relations.Remove(deleteRelation);
                    deleteRelation = null;

                    if (childRelationNode.Nodes.Count == 0)
                    {
                        childRelationNode.ImageIndex = childRelationNode.SelectedImageIndex = folderImageTree.unselectedState;
                    }
                }
            }
        }

        private void CloseCurrentRelation()
        {
            try
            {
                currentRelation = null;
                xtraTabDatabase.TabPages[1].Text = "Relation";
                GridViewData.Rows.Clear();
                GridViewData.Columns.Clear();
                UpdateDataRowNumber();
                //SwitchValueState(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR:\n" + ex.Message);
            }
        }

        private void SaveTuples(FzRelationEntity currentRelation)
        {
            int nRow, nCol;
            nRow = GridViewData.Rows.Count - 1;
            nCol = GridViewData.Columns.Count;

            if (GridViewData.Rows.Count <= 1) return;
            GridViewData.CurrentCell = GridViewData.Rows[nRow].Cells[0];

            currentRelation.Tuples.Clear();

            for (int i = 0; i < nRow; i++)
            {
                List<Object> objs = new List<object>();

                for (int j = 0; j < nCol; j++)
                {
                    if (GridViewData.Rows[i].Cells[j].Value == null)
                    {
                        throw new Exception("Value cell is empty!");
                    }

                    objs.Add(GridViewData.Rows[i].Cells[j].Value);
                }

                FzTupleEntity tuple = new FzTupleEntity() { ValuesOnPerRow = objs };
                currentRelation.Tuples.Add(tuple);
            }
        }

        private void ShowTuples(FzRelationEntity currentRelation)
        {
            if (currentRelation.Tuples.Count > 0)
            {
                int nRow = currentRelation.Tuples.Count;
                int nCol = currentRelation.Scheme.Attributes.Count;

                FzTupleEntity tuple;

                for (int i = 0; i < nRow; i++)      // Assign data for GridViewData
                {
                    tuple = currentRelation.Tuples[i];
                    GridViewData.Rows.Add();

                    for (int j = 0; j < nCol; j++)
                    {
                        GridViewData.Rows[i].Cells[j].Value = tuple.ValuesOnPerRow[j];
                    }

                }

                UpdateDataRowNumber();
            }
        }

        private void AddRelationNode(String relationName, String schemeName)
        {
            newRelation = new FzRelationEntity(relationName);
            newRelation.Scheme = FzSchemeBLL.GetSchemeByName(schemeName, fdbEntity);
            fdbEntity.Relations.Add(newRelation);
            TreeNode newNode = new TreeNode();
            newNode.Text = relationName;
            newNode.Name = relationName;
            newNode.ToolTipText = "Relation " + relationName;
            newNode.ContextMenuStrip = ContextMenu_RelationNode;
            newNode.ImageIndex = relationImageTree.unselectedState;
            newNode.SelectedImageIndex = relationImageTree.unselectedState;
            childRelationNode.Nodes.Add(newNode);

            currentRelation = newRelation;//Advoid null
           
            if (MessageBox.Show("Add values to this relation?", "Add values", MessageBoxButtons.YesNo)
                == DialogResult.Yes)
            {
                ShowColumnsAttribute(newRelation);
            }
        }

        private void ShowColumnsAttribute(FzRelationEntity currentRelation)
        {
            
            xtraTabDatabase.TabPages[1].Text = "Relation " + currentRelation.RelationName;
            xtraTabDatabase.SelectedTabPage = xtraTabDatabase.TabPages[1];
            GridViewData.Rows.Clear();
            GridViewData.Columns.Clear();

            ///Add columns to relation
            int i = 0;
            foreach (FzAttributeEntity attr in currentRelation.Scheme.Attributes)
            {
                GridViewData.Columns.Add("Column " + i, attr.AttributeName);
                i++;
            }
        }

        private bool CheckRelationData()
        {
            if (fdbEntity == null)
            {
                MessageBox.Show("Current database is empty! Please open!");
                return false;
            }

            if (currentRelation == null)
            {
                MessageBox.Show("Current relation is null!"); return false;
            }
            for (int i = 0; i < GridViewData.Rows.Count - 1; i++)
            {
                for (int j = 0; j < GridViewData.Columns.Count; j++)
                {
                    if (GridViewData.Rows[i].Cells[j].Value == null)
                    {
                        MessageBox.Show("Value can not be empty!");
                        return false;
                    }
                }
            }
            return true;
        }

        private void iNewRelation_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            CreateNewRelation();
        }

        private void iOpenRelation_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenRelation();
        }

        private void iSaveRelation_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SaveRelation();
        }

        private void iDeleteRelation_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DeleteRelation();
        }

        private void iCloseCurrentRelation_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            CloseCurrentRelation();
        }

        #endregion
            #region Context Menu Relation

        private void CTMenuRelation_NewRelation_Click(object sender, EventArgs e)
        {
            CreateNewRelation();
        }

        private void CTMenuRelNode_OpenRelation_Click(object sender, EventArgs e)
        {
            try
            {
                string relationName = childCurrentNode.Name;
                treeList1.SelectedNode = childCurrentNode;
                currentRelation = FzRelationBLL.GetRelationByName(relationName, fdbEntity);

                ShowColumnsAttribute(currentRelation);
                ShowTuples(currentRelation);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CTMenuRelation_DeleteRelations_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = new DialogResult();
                result = MessageBox.Show("Are you want to delete all relation ?", "Delete All Relations", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    childRelationNode.Nodes.Clear();
                    childCurrentNode = null;
                    xtraTabDatabase.TabPages[1].Text = "Relation";
                    GridViewData.Rows.Clear();
                    GridViewData.Columns.Clear();
                    UpdateDataRowNumber();
                    fdbEntity.Relations.Clear();
                    childRelationNode.ImageIndex = childRelationNode.SelectedImageIndex = 2;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CTMenuRelNode_DeleteRelation_Click(object sender, EventArgs e)
        {
            try
            {
                string relationName = childCurrentNode.Name;
                FzRelationEntity deleteRelation = FzRelationBLL.GetRelationByName(relationName, fdbEntity);

                DialogResult result = new DialogResult();
                result = MessageBox.Show("Are you  want to delete this relation ?", "Delete relation " + relationName, MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    if (deleteRelation.Equals(currentRelation))
                    {
                        xtraTabDatabase.TabPages[1].Text = "Relation";
                        GridViewData.Rows.Clear();
                        GridViewData.Columns.Clear();
                        UpdateDataRowNumber();
                    }

                    DeleteTreeNode(relationName, null, deleteRelation, null,null,null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CTMenuRelNode_RenameRelation_Click(object sender, EventArgs e)
        {
            try
            {
                if (fdbEntity == null) { MessageBox.Show("Current Database is empty!"); return; }


                String relationName = "";
                if (currentRelation != null)
                    relationName = currentRelation.RelationName;
                else
                    relationName = childCurrentNode.Name;

                //Set currtn relation
                currentRelation = FzRelationBLL.GetRelationByName(relationName, fdbEntity);

                renamedRelation = FzRelationBLL.GetRelationByName(relationName, fdbEntity);

                DBValues.relationsName = FzRelationBLL.GetListRelationName(fdbEntity);
                frmNewName frm = new frmNewName(4);
                frm.ShowDialog();

                renamedRelation.RelationName = frm.Name;
                if (frm.Name == null) return;
                if (currentRelation != null)
                {
                    if (renamedRelation.Equals(currentRelation))
                    {
                        if (xtraTabDatabase.TabPages[1].Text.Contains("Create Relation"))
                            xtraTabDatabase.TabPages[1].Text = "Create Relation " + frm.Name;
                        else xtraTabDatabase.TabPages[1].Text = "Relation " + frm.Name;
                        childCurrentNode.Name = childCurrentNode.Text = frm.Name;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region 4. Query Ribbon Page
        private void QueryEditor()
        {
            try
            {
                if (fdbEntity == null) { MessageBox.Show("Current database is empty!"); return; }
                
                frmQueryEditor frm = new frmQueryEditor(fdbEntity.Queries);
                frm.ShowDialog();
                //After form close
                fdbEntity.Queries = frm.Queries;
                DBValues.queriesName = FzQueryBLL.ListOfQueryName(fdbEntity);
                ShowTreeList();
                ShowTreeListNode();
                treeList1.ExpandAll();

                //Open query
                OpenQuery(frm.QueryName);

                if (currentQuery == null)
                    CloseCurrentQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OpenQuery(String queryName)
        {
            if (queryName != null)
            {
                currentQuery = FzQueryBLL.GetQueryByName(queryName, fdbEntity);

                xtraTabDatabase.TabPages[2].Text = "Query " + queryName;
                xtraTabDatabase.SelectedTabPage = xtraTabDatabase.TabPages[2];
                txtQuery.Text = currentQuery.QueryString;
            }
        }

        private void SaveQuery()
        {
            try
            {
                if (txtQuery.Text != "")
                {
                    String queryName = xtraTabDatabase.TabPages[2].Text.Substring(5);
                    String queryText = txtQuery.Text;

                    frmQueryEditor frm = new frmQueryEditor(fdbEntity.Queries) { TxtQueryName = queryName, TxtQueryText = queryText };
                    frm.ShowDialog();

                    //After form close
                    fdbEntity.Queries = frm.Queries;
                    DBValues.queriesName = FzQueryBLL.ListOfQueryName(fdbEntity);
                    ShowTreeList();
                    ShowTreeListNode();
                    treeList1.ExpandAll();

                    currentQuery = null;
                    OpenQuery(frm.QueryName);
                }
                else
                {
                    xtraTabDatabase.SelectedTabPageIndex = 2;
                    txtQuery.Focus();
                    MessageBox.Show("Please input some text to save!");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CloseCurrentQuery()
        { 
            txtQuery.Text = "";
            xtraTabDatabase.SelectedTabPageIndex = 2;
            xtraTabDatabase.TabPages[2].Text = "Query";
        }

        private void iNewQuery_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            QueryEditor();
        }

        private void iOpenQuery_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            QueryEditor();
        }

        private void iSaveQuery_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SaveQuery();
        }

        private void iDeleteQuery_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            currentQuery = null;
            QueryEditor();
            //CloseCurrentQuery();
        }

        private void iCloseCurrentQuery_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            CloseCurrentQuery();
        }

        #endregion
            #region Context Menu Query
        private void CTMenuQuery_NewQuery_Click(object sender, EventArgs e)
        {
            QueryEditor();
        }

        private void CTMenuQuery_DeleteQueries_Click(object sender, EventArgs e)
        {
            try
            {
                fdbEntity.Queries.Clear();
                ShowTreeList();
                ShowTreeListNode();
                treeList1.ExpandAll();
                currentQuery = null;
                CloseCurrentQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CTMenuQueryNode_OpenQuery_Click(object sender, EventArgs e)
        {
            String queryName = childCurrentNode.Name;
            OpenQuery(queryName);
        }

        private void CTMenuQuery_DeleteQuery_Click(object sender, EventArgs e)
        {
            String queryName = childCurrentNode.Name;
            FzQueryEntity deleteQuery = FzQueryBLL.GetQueryByName(queryName, fdbEntity);

            DialogResult result = new DialogResult();
            result = MessageBox.Show("Are you  want to delete this query ?", "Delete query " + queryName, MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            { 
                if (deleteQuery != null)
                    DeleteTreeNode(queryName, null, null, deleteQuery,null,null);
            }

        }

        private void CTMenuQuery_RenameQuery_Click(object sender, EventArgs e)
        {
            try
            {
                 if (fdbEntity == null) { MessageBox.Show("Current Database is empty!"); return; }

                String queryName = "";
                if (currentQuery != null)
                    queryName = currentQuery.QueryName;
                else
                    queryName = childCurrentNode.Name;

                currentQuery = FzQueryBLL.GetQueryByName(queryName, fdbEntity);

                FzQueryEntity renamedQuery = FzQueryBLL.GetQueryByName(queryName, fdbEntity);

                DBValues.queriesName = FzQueryBLL.ListOfQueryName(fdbEntity);
                frmNewName frm = new frmNewName(2);
                frm.ShowDialog();

                renamedQuery.QueryName = frm.Name;
                if (frm.Name == null) return;
                if (currentQuery != null)
                {
                    if (renamedQuery.Equals(currentQuery))
                    {
                        if (xtraTabDatabase.TabPages[2].Text.Contains("Create Query"))
                            xtraTabDatabase.TabPages[2].Text = "Create Query " + frm.Name;
                        else xtraTabDatabase.TabPages[2].Text = "Query " + frm.Name;
                        childCurrentNode.Name = childCurrentNode.Text = frm.Name;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void executeQueryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String queryName = childCurrentNode.Name;
            FzQueryEntity execute = FzQueryBLL.GetQueryByName(queryName, fdbEntity);

            if (execute != null)
            {
                xtraTabDatabase.SelectedTabPageIndex = 2;
                txtQuery.Text = execute.QueryString;
                ExecutingQuery();
            }
        }
        #endregion
            #region Query Processing
        private void frmMain_Load(object sender, EventArgs e)
        {
            (new frmAbout(true)).Show(); 
            QueryPL.txtQuery_TextChanged(txtQuery);
            AddRowDefault();
            StartApp();
        }

        private void ShowMessage(String message, Color color)
        {
            xtraTabDatabase.SelectedTabPageIndex = 2;
            xtraTabQueryResult.SelectedTabPageIndex = 1;
            // The type of error
            txtMessage.ForeColor = color;
            txtMessage.Text = message;
            return;
        }
       
        private void iOperator_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            int pos = txtQuery.SelectionStart;
            if (txtQuery.Text == "") txtQuery.Text = "→";
            else txtQuery.Text = txtQuery.Text.Insert(pos, "→");
            txtQuery.SelectionStart = pos + 2;
        }


        private void DeleteTemp()
        {
            string path = Directory.GetCurrentDirectory() + @"\lib\temp";
            DirectoryInfo dir = new DirectoryInfo(path);

            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }
        }
        private void ExecutingQuery()
        {
            try
            {
                DeleteTemp();
                PrepareQuery();
                String query = QueryPL.StandardizeQuery(txtQuery.Text.Trim());
                String message = QueryPL.CheckSyntax(query);
                if ( message != "")
                {
                    ShowMessage(message, Color.Red);
                    return;
                }
                FdbEntity newFdb = new FdbEntity() {
                    Relations = fdbEntity.Relations,
                    Schemes = fdbEntity.Schemes,
                    DiscreteFuzzyNumbers = fdbEntity.DiscreteFuzzyNumbers,
                    ContinuousFuzzyNumbers = fdbEntity.ContinuousFuzzyNumbers
                };

                QueryExcutetionBLL excutetion = new QueryExcutetionBLL(query.ToLower(), newFdb);
                //edit---
                QueryConditionBLL condition = new QueryConditionBLL();
                string temp_path = Directory.GetCurrentDirectory() + @"\lib\temp\";
                //GridViewResult.Enabled = false;
                //---
                FzRelationEntity result = excutetion.ExecuteQuery();
                if (excutetion.Error)
                {
                    ShowMessage(excutetion.ErrorMessage, Color.Red); return;
                }

                if (result != null)
                {
                    foreach (FzAttributeEntity attribute in result.Scheme.Attributes)
                        GridViewResult.Columns.Add(attribute.AttributeName, attribute.AttributeName);
                    int countColum = result.Scheme.Attributes.Count; //edit
                    int j, i = -1;
                    foreach (FzTupleEntity tuple in result.Tuples)
                    {
                        GridViewResult.Rows.Add();
                        i++; j = -1;
                        foreach (Object value in tuple.ValuesOnPerRow)
                        {
                            GridViewResult.Rows[i].Cells[++j].Value = value.ToString();
                            //edit--
                            if (j == countColum - 1)
                            {
                                if (condition.GetConFS(temp_path, value.ToString()) != null|| condition.GetDisFS(temp_path, value.ToString()) != null)
                                {
                                    GridViewResult.Rows[i].Cells[j].Style.ForeColor = Color.Blue;
                                    //enable ô
                                }
                                else if (value.ToString()== "FN not exists")
                                {
                                    GridViewResult.Rows[i].Cells[j].Style.ForeColor = Color.Red;
                                }
                            }
                            //----
                        }
                    }

                    xtraTabQueryResult.SelectedTabPageIndex = 0;
                }
                else
                {
                    txtMessage.Text = "There no relation satisfy the condition";
                    xtraTabQueryResult.SelectedTabPageIndex = 1;
                }

                siStatus.Caption = "Ready";
                txtMessage.ForeColor = Color.Black;
                txtMessage.Text = result.Tuples.Count + " row(s) affected";

            }
            catch (Exception ex)
            {
                return;
                //MessageBox.Show("ERROR:\n" + ex.Message);
            }
        }

        private void PrepareQuery()
        {
            GridViewResult.Rows.Clear();
            GridViewResult.Columns.Clear();
            txtMessage.Text = "";
            siStatus.Caption = "Executing query...";
        }

        private void iExecuteQuery_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ExecutingQuery();
        }

        private void iStopExecute_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            MessageBox.Show("Function is under contruction!");
        }
        //edit---
        private void GridViewResult_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            QueryConditionBLL condition = new QueryConditionBLL();
            string temp_path = Directory.GetCurrentDirectory() + @"\lib\temp\";
            string FN_Name = GridViewResult.CurrentCell.Value.ToString();
            DisFS dis = condition.GetDisFS(temp_path, FN_Name);
            if (dis != null )
            {
                frmDisFSRelation frm = new frmDisFSRelation();
                frm.FSName = FN_Name;
                frm.ShowDialog();
                DrawChart(frm.PointList);
            }
        }
        //----
        #endregion

        #region 5. Fuzzy Set Ribbon Page
        //edit---
        private void DiscreteFNEditor()
        {
            try
            {
                if (fdbEntity == null) { MessageBox.Show("Current database is empty!"); return; }
                DBValues.disFNName = DiscreteFuzzySetBLL.ListOfDiscreteFNName(fdbEntity);
                frmDescreteEditor frm = new frmDescreteEditor(fdbEntity.DiscreteFuzzyNumbers, true);
                frm.ShowDialog();
                //After form close
                fdbEntity.DiscreteFuzzyNumbers = frm.DiscreteFSs;
                DBValues.disFNName = DiscreteFuzzySetBLL.ListOfDiscreteFNName(fdbEntity);
                ShowTreeList();
                ShowTreeListNode();
                treeList1.ExpandAll();

                //Open query
                //OpenQuery(frm.QueryName);

                //if (currentDis == null)
                  //  CloseCurrentQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ContinuousFNEditor()
        {
            try
            {
                if (fdbEntity == null) { MessageBox.Show("Current database is empty!"); return; }

                frmContinuousEditor frm = new frmContinuousEditor(fdbEntity.ContinuousFuzzyNumbers, true);
                frm.ShowDialog();
                //After form close
                fdbEntity.ContinuousFuzzyNumbers = frm.ContinuousFSs;
                DBValues.conFNName = ContinuousFuzzySetBLL.ListOfContinuousFNName(fdbEntity);
                ShowTreeList();
                ShowTreeListNode();
                treeList1.ExpandAll();

                //Open query
                //OpenQuery(frm.QueryName);

                //if (currentDis == null)
                //  CloseCurrentQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //---
        private void iDiscreteFuzzySet_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            frmDescreteEditor frm = new frmDescreteEditor(false);
            frm.ShowDialog();
        }

        private void barButtonItem11_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            frmContinuousEditor frm = new frmContinuousEditor(false);
            frm.ShowDialog();
        }

        private void iListDescrete_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            frmListDescrete frm = new frmListDescrete();
            frm.ShowDialog();

            DrawChart(frm.PointList);
        }

        private void iListContinuous_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            frmListContinuous frm = new frmListContinuous();
            frm.ShowDialog();

            DrawChart(frm.PointList);
        }

        private void iGetMemberships_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            frmFuzzySetAction frm = new frmFuzzySetAction();
            frm.ShowDialog();
        }

        private void DrawChart(List<ContinuousFuzzySetBLL> conFS)
        {
            if (conFS == null || conFS.Count == 0) return;

            xtraTabDatabase.SelectedTabPageIndex = 3;
            chart1.ChartAreas.Clear();
            chart1.Series.Clear();
            Double max = int.MinValue;
            Color[] colors = GetUniqueRandomColor(conFS.Count);
            // Get max on Cordinate-X
            foreach (var item in conFS)
            {
                if (item.Bottom_Right > max)
                    max = item.Bottom_Right;
            }

            chart1.ChartAreas.Add("FuzzySets");
            chart1.ChartAreas["FuzzySets"].AxisX.Minimum = 0;
            chart1.ChartAreas["FuzzySets"].AxisX.Maximum = max;
            chart1.ChartAreas["FuzzySets"].AxisX.Interval = 5;
            chart1.ChartAreas["FuzzySets"].AxisY.Minimum = 0;
            chart1.ChartAreas["FuzzySets"].AxisY.Maximum = 2;
            chart1.ChartAreas["FuzzySets"].AxisY.Interval = 0.1;

            int i = 0;
            foreach (var item in conFS)
            {
                chart1.Series.Add(item.FuzzySetName);
                chart1.Series[item.FuzzySetName].Color = colors[i];
                chart1.Series[item.FuzzySetName].BorderWidth = 5;
                chart1.Series[item.FuzzySetName].Label = item.FuzzySetName;
                chart1.Series[item.FuzzySetName].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                chart1.Series[item.FuzzySetName].Points.AddXY(item.Bottom_Left, 0);
                chart1.Series[item.FuzzySetName].Points.AddXY(item.Top_Left, 1);
                chart1.Series[item.FuzzySetName].Points.AddXY(item.Top_Right, 1);
                chart1.Series[item.FuzzySetName].Points.AddXY(item.Bottom_Right, 0);

                i++;
            }
        }

        private void DrawChart(List<DiscreteFuzzySetBLL> disFS)
        {
            if (disFS == null || disFS.Count == 0) return;

            xtraTabDatabase.SelectedTabPageIndex = 3;
            chart1.ChartAreas.Clear();
            chart1.Series.Clear();
            Double max = int.MinValue;
            Color[] colors = GetUniqueRandomColor(disFS.Count);
            // Get max on Cordinate-X
            foreach (var item in disFS)
            {
                Double tmp = item.GetMaxValue();
                if (tmp > max)
                    max = tmp;
            }

            chart1.ChartAreas.Add("FuzzySets");
            chart1.ChartAreas["FuzzySets"].AxisX.Minimum = 0;
            chart1.ChartAreas["FuzzySets"].AxisX.Maximum = max;
            chart1.ChartAreas["FuzzySets"].AxisX.Interval = 5;
            chart1.ChartAreas["FuzzySets"].AxisY.Minimum = 0;
            chart1.ChartAreas["FuzzySets"].AxisY.Maximum = 1;
            chart1.ChartAreas["FuzzySets"].AxisY.Interval = 0.1;

            int i = 0;
            foreach (var item in disFS)
            {
                chart1.Series.Add(item.FuzzySetName);
                chart1.Series[item.FuzzySetName].Color = colors[i];
                chart1.Series[item.FuzzySetName].BorderWidth = 2;
                chart1.Series[item.FuzzySetName].Label = item.FuzzySetName;
                chart1.Series[item.FuzzySetName].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
                for (int j = 0; j < item.ValueSet.Count; j++)
                {
                    chart1.Series[item.FuzzySetName].Points.AddXY(item.ValueSet[j], item.MembershipSet[j]);
                }

                i++;
            }
        }

        private Color[] GetUniqueRandomColor(int count)
        {
            Color[] colors = new Color[count];
            HashSet<Color> hs = new HashSet<Color>();
            Random randomColor = new Random();

            for (int i = 0; i < count; i++)
            {
                Color color;
                while (!hs.Add(color = Color.FromArgb(randomColor.Next(1, 100), randomColor.Next(1, 225), randomColor.Next(1, 230)))) ;
                colors[i] = color;
            }

            return colors;
        }
            #region Context Menu DiscreteFN
            private void CTMenuDiscreteFN_NewDiscreteFN_Click(object sender, EventArgs e)
            {
                 DiscreteFNEditor();
            }

            private void CTMenuDiscreteFN_DeleteDiscreteFNs_Click(object sender, EventArgs e)
            {
                try
                {
                    fdbEntity.DiscreteFuzzyNumbers.Clear();
                    ShowTreeList();
                    ShowTreeListNode();
                    treeList1.ExpandAll();
                    currentDis = null;
                   // CloseCurrentDiscreteFS();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            private void CTMenuDiscreteFNNode_OpenDiscreteFN_Click(object sender, EventArgs e)
            {
            try
            {
                String name = childCurrentNode.Name;
                treeList1.SelectedNode = childCurrentNode;
                currentDis = DiscreteFuzzySetBLL.GetDisFNByName(name, fdbEntity);

                List<Double> values = SplitString(currentDis.V);
                List<Double> memberships = SplitString(currentDis.M);
                frmDescreteEditor frm = new frmDescreteEditor(fdbEntity.DiscreteFuzzyNumbers, name, values, memberships,true); //discrete FS is membership
                frm.ShowDialog();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CTMenuDiscreteFN_DeleteDiscreteFN_Click(object sender, EventArgs e)
            {
                String disFNName = childCurrentNode.Name;
                FzDiscreteFuzzySetEntity deleteDisFN = DiscreteFuzzySetBLL.GetDisFNByName(disFNName, fdbEntity);

                DialogResult result = new DialogResult();
                result = MessageBox.Show("Are you  want to delete this Discrete Fuzzy Number ?", "Delete Discrete Fuzzy Number " + disFNName, MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {

                    if (deleteDisFN != null)
                        DeleteTreeNode(disFNName, null, null,null, deleteDisFN,null);
                 }

            }

        private void CTMenuDiscreteFN_RenameDiscreteFN_Click(object sender, EventArgs e)
        {

            try
            {
                if (fdbEntity == null) { MessageBox.Show("Current Database is empty!"); return; }

                String disFNName = "";
                if (currentDis != null)
                    disFNName = currentDis.Name;
                else
                    disFNName = childCurrentNode.Name;

                currentDis = DiscreteFuzzySetBLL.GetDisFNByName(disFNName, fdbEntity);

                FzDiscreteFuzzySetEntity renamedDisFN = DiscreteFuzzySetBLL.GetDisFNByName(disFNName, fdbEntity);

                DBValues.disFNName = DiscreteFuzzySetBLL.ListOfDiscreteFNName(fdbEntity);
                frmNewName frm = new frmNewName(5);
                frm.ShowDialog();
                if (frm.Name == null) return;
                if (currentDis != null)
                {
                    renamedDisFN.Name = frm.Name;
                    if (renamedDisFN.Equals(currentDis))
                    {
                        if (xtraTabDatabase.TabPages[3].Text.Contains("Create Query"))//edittttttttttttttttttttttttttttttttttttttttt
                            xtraTabDatabase.TabPages[3].Text = "Create Query " + frm.Name;
                        else xtraTabDatabase.TabPages[3].Text = "Query " + frm.Name;
                        childCurrentNode.Name = childCurrentNode.Text = frm.Name;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion
        #region Context Menu ContinuousFN
        private void CTMenuContinuousFN_NewContinuousFN_Click(object sender, EventArgs e)
            {
                ContinuousFNEditor();
            }

            private void CTMenuContinuousFN_DeleteContinuousFNs_Click(object sender, EventArgs e)
            {
                try
                {
                    fdbEntity.ContinuousFuzzyNumbers.Clear();
                    ShowTreeList();
                    ShowTreeListNode();
                    treeList1.ExpandAll();
                    currentCon = null;
                    // CloseCurrentDiscreteFS();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            private void CTMenuContinuousFNNode_OpenContinuousFN_Click(object sender, EventArgs e)
            {
                try
                {
                    String name = childCurrentNode.Name;
                    treeList1.SelectedNode = childCurrentNode;
                    currentCon = ContinuousFuzzySetBLL.GetConFNByName(name, fdbEntity);

                frmContinuousEditor frm = new frmContinuousEditor(fdbEntity.ContinuousFuzzyNumbers, name,currentCon.Bottom_Left,currentCon.Top_Left,currentCon.Top_Right,currentCon.Bottom_Right, true); //discrete FS is membership
                    frm.ShowDialog();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            private void CTMenuContinuousFN_DeleteContinuousFN_Click(object sender, EventArgs e)
            {
                String conFNName = childCurrentNode.Name;
            FzContinuousFuzzySetEntity deleteConFN = ContinuousFuzzySetBLL.GetConFNByName(conFNName, fdbEntity);

                DialogResult result = new DialogResult();
                result = MessageBox.Show("Are you  want to delete this Continuous Fuzzy Number ?", "Delete Continuous Fuzzy Number " + conFNName, MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {

                    if (deleteConFN != null)
                        DeleteTreeNode(conFNName, null, null, null, null,deleteConFN);
                }

            }

            private void CTMenuContinuousFN_RenameContinuousFN_Click(object sender, EventArgs e)
            {

                try
                {
                    if (fdbEntity == null) { MessageBox.Show("Current Database is empty!"); return; }

                    String conFNName = "";
                    if (currentCon != null)
                        conFNName = currentCon.Name;
                    else
                        conFNName = childCurrentNode.Name;

                    currentCon = ContinuousFuzzySetBLL.GetConFNByName(conFNName, fdbEntity);

                FzContinuousFuzzySetEntity renamedConFN = ContinuousFuzzySetBLL.GetConFNByName(conFNName, fdbEntity);

                    DBValues.conFNName = ContinuousFuzzySetBLL.ListOfContinuousFNName(fdbEntity);
                    frmNewName frm = new frmNewName(6);
                    frm.ShowDialog();

                    if (frm.Name == null) return;
                    if (currentCon != null)
                    {
                    renamedConFN.Name = frm.Name;
                    if (renamedConFN.Equals(currentCon))
                        {
                            if (xtraTabDatabase.TabPages[3].Text.Contains("Create Query"))//editttttttttttttttttt
                                xtraTabDatabase.TabPages[3].Text = "Create Query " + frm.Name;
                            else xtraTabDatabase.TabPages[3].Text = "Query " + frm.Name;
                            childCurrentNode.Name = childCurrentNode.Text = frm.Name;
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            #endregion

        #endregion


        #region 6. Help Ribbon Page
        #endregion

        #region 7. Navigation Pane (Tree List)

        private void ShowTreeList()
        {
            treeList1.Nodes.Clear();
            SetTreeListImageCollection();

            //Node fuzzy database
            parentFdbNode = new TreeNode();
            parentFdbNode.Text = DBValues.dbName.ToUpper();
            //NodeDB.ToolTipText = "Database " + Resource.dbShowName;
            parentFdbNode.ContextMenuStrip = ContextMenu_Database;
            parentFdbNode.ImageIndex = parentFdbImageTree.unselectedState;
            parentFdbNode.SelectedImageIndex = parentFdbImageTree.selectedState;
            treeList1.Nodes.Add(parentFdbNode);

            //Node scheme
            childSchemeNode = new TreeNode();
            childSchemeNode.Text = "Schemes";
            childSchemeNode.ToolTipText = "Schemes";
            childSchemeNode.ContextMenuStrip = ContextMenu_Schema;
            childSchemeNode.ImageIndex = folderImageTree.unselectedState;
            childSchemeNode.SelectedImageIndex = folderImageTree.selectedState;
            parentFdbNode.Nodes.Add(childSchemeNode);

            //Node Relation
            childRelationNode = new TreeNode();
            childRelationNode.Text = "Relations";
            childRelationNode.ToolTipText = "Relations";
            childRelationNode.ContextMenuStrip = ContextMenu_Relation;
            childRelationNode.ImageIndex = folderImageTree.unselectedState;
            childRelationNode.SelectedImageIndex = folderImageTree.selectedState;
            parentFdbNode.Nodes.Add(childRelationNode);

            //Node Query
            childQueryNode = new TreeNode();
            childQueryNode.Text = "Queries";
            childQueryNode.ToolTipText = "Queries";
            childQueryNode.ContextMenuStrip = ContextMenu_Query;
            childQueryNode.ImageIndex = folderImageTree.unselectedState;
            childQueryNode.SelectedImageIndex = folderImageTree.selectedState;
            parentFdbNode.Nodes.Add(childQueryNode);

            //Node DiscreteFN
            childDiscreteFNNode = new TreeNode();
            childDiscreteFNNode.Text = "Discrete Fuzzy Numbers";
            childDiscreteFNNode.ToolTipText = "Discrete Fuzzy Numbers";
            childDiscreteFNNode.ContextMenuStrip = ContextMenu_DiscreteFN;
            childDiscreteFNNode.ImageIndex = folderImageTree.unselectedState;
            childDiscreteFNNode.SelectedImageIndex = folderImageTree.selectedState;
            parentFdbNode.Nodes.Add(childDiscreteFNNode);

            //Node ContinuousFN
            childContinuousFNNode = new TreeNode();
            childContinuousFNNode.Text = "Continuous Fuzzy Numbers";
            childContinuousFNNode.ToolTipText = "Continuous Fuzzy Numbers";
            childContinuousFNNode.ContextMenuStrip = ContextMenu_ContinuousFN;
            childContinuousFNNode.ImageIndex = folderImageTree.unselectedState;
            childContinuousFNNode.SelectedImageIndex = folderImageTree.selectedState;
            parentFdbNode.Nodes.Add(childContinuousFNNode);

        }

        private void ShowTreeListNode()
        {
            foreach (FzSchemeEntity s in fdbEntity.Schemes)
            {
                childNewNode = new TreeNode();
                childNewNode.Text = s.SchemeName;
                childNewNode.Name = s.SchemeName;
                childNewNode.ToolTipText = "Scheme " + s.SchemeName;
                childNewNode.ContextMenuStrip = ContextMenu_SchemaNode;
                childNewNode.ImageIndex = schemeImageTree.unselectedState;
                childNewNode.SelectedImageIndex = schemeImageTree.selectedState;
                childSchemeNode.Nodes.Add(childNewNode);
            }

            foreach (FzRelationEntity r in fdbEntity.Relations)
            {
                childNewNode = new TreeNode();
                childNewNode.Text = r.RelationName;
                childNewNode.Name = r.RelationName;
                childNewNode.ToolTipText = "Relation " + r.RelationName;
                childNewNode.ContextMenuStrip = ContextMenu_RelationNode;
                childNewNode.ImageIndex = relationImageTree.unselectedState;
                childNewNode.SelectedImageIndex = relationImageTree.selectedState;
                childRelationNode.Nodes.Add(childNewNode);
            }

            foreach (FzQueryEntity q in fdbEntity.Queries)
            {
                childNewNode = new TreeNode();
                childNewNode.Text = q.QueryName;
                childNewNode.Name = q.QueryName;
                childNewNode.ToolTipText = "Queries " + q.QueryName;
                childNewNode.ContextMenuStrip = ContextMenu_QueryNode;
                childNewNode.ImageIndex = queryImageTree.unselectedState;
                childNewNode.SelectedImageIndex = queryImageTree.selectedState;
                childQueryNode.Nodes.Add(childNewNode);
            }
            foreach (FzDiscreteFuzzySetEntity d in fdbEntity.DiscreteFuzzyNumbers)
            {
                childNewNode = new TreeNode();
                childNewNode.Text = d.Name;
                childNewNode.Name = d.Name;
                childNewNode.ToolTipText = "Discrete Fuzzy Number " + d.Name;
                childNewNode.ContextMenuStrip = ContextMenu_DiscreteFNNode;
                childNewNode.ImageIndex = discreteFNImageTree.unselectedState;
                childNewNode.SelectedImageIndex = discreteFNImageTree.selectedState;
                childDiscreteFNNode.Nodes.Add(childNewNode);
            }
            foreach (FzContinuousFuzzySetEntity c in fdbEntity.ContinuousFuzzyNumbers)
            {
                childNewNode = new TreeNode();
                childNewNode.Text = c.Name;
                childNewNode.Name = c.Name;
                childNewNode.ToolTipText = "Continuous Fuzzy Number " + c.Name;
                childNewNode.ContextMenuStrip = ContextMenu_ContinuousFNNode;
                childNewNode.ImageIndex = continuousFNImageTree.unselectedState;
                childNewNode.SelectedImageIndex = continuousFNImageTree.selectedState;
                childContinuousFNNode.Nodes.Add(childNewNode);
            }
        }

        private void SetTreeListImageCollection()
        {
            //treeList1.SelectImageList = treeListImageCollection;
            treeList1.ImageList = treeListImageCollection;

            parentFdbImageTree.selectedState = parentFdbImageTree.unselectedState = 0;
            folderImageTree.selectedState = 1;//folder is opened
            folderImageTree.unselectedState = 2;// folder is closed
            schemeImageTree.selectedState = schemeImageTree.unselectedState = 3;
            relationImageTree.selectedState = relationImageTree.unselectedState = 3;
            queryImageTree.selectedState = queryImageTree.unselectedState = 4;
            discreteFNImageTree.selectedState = discreteFNImageTree.unselectedState = 4;
            continuousFNImageTree.selectedState = continuousFNImageTree.unselectedState = 4;

        }

        private void TreeList1_NodeMouseClick(Object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                childCurrentNode = e.Node;

                if (childCurrentNode.Parent == parentFdbNode && !childCurrentNode.IsExpanded)
                {
                    e.Node.ImageIndex = e.Node.SelectedImageIndex = folderImageTree.unselectedState;
                }

                if (e.Button == MouseButtons.Right)
                {
                    childCurrentNode.ContextMenuStrip.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show( ex.Message);
            }
        }

        private void TreeList1_AfterExpand(Object sender, TreeViewEventArgs e)
        {
            try
            {
                if (e.Node != parentFdbNode && e.Node.IsExpanded)
                {
                    e.Node.ImageIndex = e.Node.SelectedImageIndex = folderImageTree.selectedState;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void treeList1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            childCurrentNode = e.Node;
            String name = childCurrentNode.Name;

            if (childCurrentNode.Parent == childRelationNode)
            {
                currentRelation = FzRelationBLL.GetRelationByName(name, fdbEntity);
                ShowColumnsAttribute(currentRelation);
                ShowTuples(currentRelation);
            }
            if (childCurrentNode.Parent == childSchemeNode)
            {
                currentScheme = FzSchemeBLL.GetSchemeByName(name, fdbEntity);
                OpenScheme(currentScheme);
            }
            if (childCurrentNode.Parent == childQueryNode)
            {
                currentQuery = FzQueryBLL.GetQueryByName(name, fdbEntity);
                OpenQuery(name);
                txtQuery.Focus();
            }
            //if (childCurrentNode.Parent == childDiscreteFSNode)
            //{
            //    currentDis = 
            //    OpenQuery(name);
            //    txtQuery.Focus();
            //}

        }

        /// <summary>
        /// Delete tree node on treeList and also delete Object in db
        /// </summary>
        private void DeleteTreeNode(String deleteNodeName, FzSchemeEntity schemeEntity, FzRelationEntity relationEntity, FzQueryEntity queryEntity, FzDiscreteFuzzySetEntity disEntity,FzContinuousFuzzySetEntity conEntity)
        {
            if (schemeEntity != null)
            {
                TreeNode deletedNode = childSchemeNode.Nodes[deleteNodeName];
                deletedNode.Remove();
                fdbEntity.Schemes.Remove(schemeEntity);
                schemeEntity = null;

                if (childSchemeNode.Nodes.Count == 0)
                {
                    childSchemeNode.ImageIndex = childSchemeNode.SelectedImageIndex = 2;
                }
            }

            if (relationEntity != null)
            {
                TreeNode deletedNode = childRelationNode.Nodes[deleteNodeName];
                deletedNode.Remove();
                fdbEntity.Relations.Remove(relationEntity);
                relationEntity = null;

                if (childRelationNode.Nodes.Count == 0)
                {
                    childRelationNode.ImageIndex = childRelationNode.SelectedImageIndex = 2;
                }
            }

            if (queryEntity != null)
            {
                TreeNode deletedNode = childQueryNode.Nodes[deleteNodeName];
                deletedNode.Remove();
                fdbEntity.Queries.Remove(queryEntity);
                queryEntity = null;

                if (childQueryNode.Nodes.Count == 0)
                {
                    childQueryNode.ImageIndex = childQueryNode.SelectedImageIndex = 2;//folder close
                }
            }
            if (disEntity != null)
            {
                TreeNode deletedNode = childDiscreteFNNode.Nodes[deleteNodeName];
                deletedNode.Remove();
                fdbEntity.DiscreteFuzzyNumbers.Remove(disEntity);
                disEntity = null;

                if (childDiscreteFNNode.Nodes.Count == 0)
                {
                    childDiscreteFNNode.ImageIndex = childDiscreteFNNode.SelectedImageIndex = 2;//folder close
                }
            }
            if (conEntity != null)
            {
                TreeNode deletedNode = childContinuousFNNode.Nodes[deleteNodeName];
                deletedNode.Remove();
                fdbEntity.ContinuousFuzzyNumbers.Remove(conEntity);
                conEntity = null;

                if (childContinuousFNNode.Nodes.Count == 0)
                {
                    childContinuousFNNode.ImageIndex = childContinuousFNNode.SelectedImageIndex = 2;//folder close
                }
            }
        }

        private void AddTreeNode() { }

        //private void ShowTreeList()
        //{
        //    treeList1.Nodes.Clear();
        //    SetTreeListImageCollection();   

        //    //Node database
        //    parentFdbNode = null;
        //    parentFdbNode = treeList1.AppendNode(null, null);
        //    parentFdbNode.SetValue("name", DBValues.dbName.ToUpper());//"name" is the "FileName" in Control TreeList (run desig and add column...)
        //    //
        //    parentFdbNode.ImageIndex = parentFdbImageTree.selectedState;
        //    parentFdbNode.SelectImageIndex = parentFdbImageTree.selectedState;

        //    //Node Scheme
        //    childSchemeNode = null;
        //    childSchemeNode = treeList1.AppendNode(null, parentFdbNode);
        //    childSchemeNode.SetValue("name", "Schemes");
        //    //Add context menu
        //    childSchemeNode.ImageIndex = folderImageTree.unselectedState;
        //    childSchemeNode.SelectImageIndex = folderImageTree.selectedState;

        //    //Node Relation
        //    childRelationNode = null;
        //    childRelationNode = treeList1.AppendNode(null, parentFdbNode);
        //    childRelationNode.SetValue("name", "Relations");
        //    //Add context menu
        //    childRelationNode.ImageIndex = folderImageTree.unselectedState;
        //    childRelationNode.SelectImageIndex = folderImageTree.selectedState;

        //    //Node Query
        //    childQueryNode = null;
        //    childQueryNode = treeList1.AppendNode(null, parentFdbNode);
        //    childQueryNode.SetValue("name", "Queries");
        //    //Add context menu
        //    childQueryNode.ImageIndex = folderImageTree.unselectedState;
        //    childQueryNode.SelectImageIndex = folderImageTree.selectedState;


        //}

        //private void ShowTreeListNode()
        //{

        //    foreach (FzSchemeEntity s in fdbEntity.Schemes)
        //    {
        //        childNewNode = null;
        //        childNewNode = treeList1.AppendNode(null, parentFdbNode);
        //        childNewNode.SetValue("name", s.SchemeName);
        //        //Add context menu here
        //        childNewNode.ImageIndex = schemeImageTree.unselectedState;
        //        childNewNode.SelectImageIndex = schemeImageTree.unselectedState;
        //    }

        //    foreach (FzRelationEntity r in fdbEntity.Relations)
        //    {
        //        childNewNode = null;
        //        childNewNode = treeList1.AppendNode(null, parentFdbNode);
        //        childNewNode.SetValue("name", r.RelationName);
        //        childNewNode.ImageIndex = relationImageTree.unselectedState;
        //        childNewNode.SelectImageIndex = relationImageTree.unselectedState;
        //    }

        //    foreach (FzQueryEntity q in fdbEntity.Queries)
        //    {
        //        childNewNode = null;
        //        childNewNode = treeList1.AppendNode(null, parentFdbNode);
        //        childNewNode.SetValue("name", q.QueryName);
        //        childNewNode.ImageIndex = queryImageTree.unselectedState;
        //        childNewNode.SelectImageIndex = queryImageTree.unselectedState;
        //    }
        //}

        //private IDXMenuManager menuManager;
        //public IDXMenuManager MenuManager
        //{
        //    get { return menuManager; }
        //    set { menuManager = value; }    
        //}

        //private DXPopupMenu CreatePopupMenu()
        //{
        //    DXPopupMenu menu = new DXPopupMenu();
        //    menu.Items.Add(new DXMenuItem("Menu Item 1"));
        //    return menu;
        //}

        //private void treeList1_CustomDrawNodeCell(object sender, DevExpress.XtraTreeList.CustomDrawNodeCellEventArgs e)
        //{
        //    TreeList tl = sender as TreeList;
        //    if (e.Node == tl.FocusedNode)
        //    {
        //        e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
        //        Rectangle rect = new Rectangle(
        //        e.EditViewInfo.ContentRect.Left,
        //        e.EditViewInfo.ContentRect.Top,
        //        Convert.ToInt32(e.Graphics.MeasureString(e.CellText, treeList1.Font).Width + 1),
        //        Convert.ToInt32(e.Graphics.MeasureString(e.CellText, treeList1.Font).Height));
        //        if ((sender as Control).Focused)
        //            e.Graphics.FillRectangle(SystemBrushes.Highlight, rect);
        //        else
        //            e.Graphics.FillRectangle(SystemBrushes.InactiveCaption, rect);
        //        e.Graphics.DrawString(e.CellText, treeList1.Font, SystemBrushes.HighlightText, rect);
        //        e.Handled = true;
        //    }
        //}

        #endregion

        #region 8. Start up Form

        private void InitSkinGallery()
        {
            SkinHelper.InitSkinGallery(rgbiSkins, true);
        }

        public void ResetObject()
        {
            fdbEntity = null;
            currentScheme = newScheme = null;
            //currentRelation = NewRelation = RenamedRelation = null;
            //CurrentQuery = NewQuery = RenamedQuery = null;
            parentFdbNode = childSchemeNode = childRelationNode = childQueryNode = childCurrentNode = childDiscreteFNNode= childNewNode = null;
        }

        public void ResetSchemePage(Boolean state)
        {
            //CloseCurrentScheme();
            AddRowDefault();

            GridViewDesign.Enabled = state;
            Btn_Design_DeleteRow.Enabled = state;
            Btn_Design_ClearData.Enabled = state;
            Btn_Design_UpdateData.Enabled = state;
        }

        public void ResetRelationPage(bool state)
        {
            xtraTabDatabase.TabPages[1].Text = "Relation";

        }

        public void ResetQueryPage(bool state)
        {
            xtraTabDatabase.TabPages[2].Text = "Query";
            
        }
        public void ResetFSPage(bool state)
        {
            if (fdbEntity != null)
            {
                string name = xtraTabDatabase.TabPages[3].Text;
                DBValues.disFNName = DiscreteFuzzySetBLL.ListOfDiscreteFNName(fdbEntity);
                DBValues.conFNName = ContinuousFuzzySetBLL.ListOfContinuousFNName(fdbEntity);
                foreach (string n in DBValues.disFNName)
                {
                    if (n.Contains(name))
                    {
                        xtraTabDatabase.TabPages[3].Text = "Fuzzy Set";
                        break;
                    }
                }
                foreach (string c in DBValues.conFNName)
                {
                    if (c.Contains(name))
                    {
                        xtraTabDatabase.TabPages[3].Text = "Fuzzy Set";
                        break;
                    }
                }
            }
        }

        public void ResetInputValue(bool state)
        {
            
        }

        private void ResetRibbonPage(Boolean state)
        {
            iSave.Enabled = state;
            iSaveAs.Enabled = state;
            iCloseDatabase.Enabled = state;

            connectionRibbonPageGroup.Visible = state;

            schemeRibbonPage.Visible = state;
            relationRibbonPage.Visible= state;
            queryRibbonPage.Visible = state;
        }
        private void ActiveDatabase(Boolean state)
        {
            ResetSchemePage(state);
            ResetRelationPage(state);
            ResetQueryPage(state);
            ResetFSPage(state);
            ResetInputValue(state);
            ResetRibbonPage(state);
        }

        private void StartApp()
        {
            currentRow = currentCell = 0;
            validated = flag = true;
            rollbackCell = false;
            xtraTabDatabase.Show();
            xtraTabDatabase.SelectedTabPageIndex = 0;

            //SwitchValueState(true);
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += new ElapsedEventHandler(OnTimeEvent);
            ActiveDatabase(false);
        }

        #endregion

        #region 9. GridView Design Scheme 
        private void AddRowDefault()
        {
            UnsetReadOnlyGridView();
            xtraTabDatabase.TabPages[0].Text = "Scheme";
            int n = GridViewDesign.Rows.Count - 2;
            for (int i = n; i >= 0; i--)
                //if (!GridViewDesign.Rows[i].IsNewRow)
                    GridViewDesign.Rows.Remove(GridViewDesign.Rows[i]);
            Object[] _default = new Object[] { true, "Edit_PrimaryKey_Here", "Int32", "[-2147483648  ...  2147483647]", "The primary key of this relation" };
            GridViewDesign.Rows.Add(_default);
            toolStripLabel1.Text = lblDesignRowNumberIndicator.Text = "2 / 2";
        }

        private void GridViewDesign_Click(object sender, EventArgs e)
        {
            if (GridViewDesign.CurrentRow != null)
            {
                toolStripLabel1.Text = lblDesignRowNumberIndicator.Text = (GridViewDesign.CurrentRow.Index + 1) + " / " + GridViewDesign.Rows.Count;
            }
            else
            {
                toolStripLabel1.Text = lblDesignRowNumberIndicator.Text = "1 / " + GridViewDesign.Rows.Count;
            }
        }

        private void GridViewDesign_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            if (GridViewDesign.CurrentRow != null)
            {
                toolStripLabel1.Text = lblDesignRowNumberIndicator.Text = (GridViewDesign.CurrentRow.Index + 1) + " / " + GridViewDesign.Rows.Count;
            }
            else
            {
                toolStripLabel1.Text = lblDesignRowNumberIndicator.Text = "1 / " + GridViewDesign.Rows.Count;
            }
        }

        private void GridViewDesign_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (flag) // set flag to prevent selectionChanged repeat 2 times
                {
                    if (GridViewDesign.CurrentRow.Index != currentRow)
                    {
                        if (ValidateRow(currentRow) == false)
                        {
                            flag = false;
                            GridViewDesign.CurrentCell = GridViewDesign.Rows[currentRow].Cells[currentCell];
                        }
                        else
                        {
                           // GridViewDesign.CurrentCell = GridViewDesign.Rows[currentRow].Cells[currentCell];
                            currentRow = GridViewDesign.CurrentRow.Index;
                        }
                    }
                }

                flag = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR:\n" + ex.Message);
            }
        }

        private void GridViewDesign_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            try
            {
                if (currentScheme != null)
                {
                    if (e.ColumnIndex == 1)
                    {
                        if (FzSchemeBLL.IsInherited(currentScheme, fdbEntity.Relations))
                        {
                            MessageBox.Show("This scheme is read only!");
                            GridViewDesign.ClearSelection();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR:\n" + ex.Message);
            }
        }

        private void GridViewDesign_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (GridViewDesign.CurrentCell.Value != null)
            {
                if (e.ColumnIndex == 1)
                {
                    for (int i = 0; i < GridViewDesign.Rows.Count - 1; i++)
                    {
                        if (GridViewDesign.CurrentCell.Value.ToString().CompareTo(GridViewDesign.Rows[i].Cells[1].Value.ToString()) == 0 && GridViewDesign.CurrentCell.RowIndex != i)
                        {
                            MessageBox.Show("There is already an attribute with the same name!");
                            GridViewDesign.ClearSelection();
                            GridViewDesign.CurrentCell.Selected = true;
                            break;
                        }
                    }
                }

                String temp = GridViewDesign.CurrentCell.Value.ToString();
                GridViewDesign.CurrentCell.ToolTipText = temp;
            }
        }

        private void GridViewDesign_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 0)
                {
                    if (currentScheme != null)
                    {
                        if (FzSchemeBLL.IsInherited(currentScheme, fdbEntity.Relations))
                        {
                            MessageBox.Show("This scheme is being inherited!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR:\n" + ex.Message);
            }
        }

        private void GridViewDesign_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 2)
                {
                    if (currentScheme != null && FzSchemeBLL.IsInherited(currentScheme, fdbEntity.Relations))
                    {
                        MessageBox.Show("This scheme is being inherited!");
                    }
                    else
                    {
                        currentCell = e.ColumnIndex;

                        frmDataType frm = new frmDataType();
                        frm.ShowDialog();

                        if (frm.TypeName == "")
                        {
                            GridViewDesign.Rows[currentRow].Cells[currentCell].Value = frm.DataType;
                        }
                        else GridViewDesign.Rows[currentRow].Cells[currentCell].Value = frm.TypeName;

                        GridViewDesign.Rows[currentRow].Cells[currentCell + 1].Value = frm.Domain;
                    }
                }
                else if (e.ColumnIndex == 1)
                {
                    if (currentScheme != null && FzSchemeBLL.IsInherited(currentScheme, fdbEntity.Relations))
                    {
                        MessageBox.Show("This scheme is being inherited!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR:\n" + ex.Message);
            }
        }

        private bool ValidateRow(int rowIndex)
        {
            try
            {
                if (rowIndex >= 0 && rowIndex < GridViewDesign.Rows.Count)
                {
                    bool prKey = (GridViewDesign.Rows[rowIndex].Cells["PrimaryKey"].Value != null);  
                    bool attrName = (GridViewDesign.Rows[rowIndex].Cells["ColumnName"].Value != null); 
                    bool typeName = (GridViewDesign.Rows[rowIndex].Cells["ColumnType"].Value != null); 
                    bool description = (GridViewDesign.Rows[rowIndex].Cells["ColumnDescription"].Value != null);
                    if (attrName && typeName)
                        return true;
                    else if (prKey || attrName || typeName || description)
                    {
                        if (!attrName && !typeName)
                        {
                            MessageBox.Show("Input the attribute name and data type!");
                            currentCell = 1;
                            return false;
                        }
                        else if (!attrName)
                        {
                            MessageBox.Show("Input the attribute name!");
                            currentCell = 1;
                            return false;
                        }
                        else
                        {
                            MessageBox.Show("Select a data type!");
                            currentCell = 2;
                            return false;
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }

            return true;
        }

        private void btn_Design_Home_Click(object sender, EventArgs e)
        {
            if (GridViewDesign.Rows.Count > 1)
            {
                GridViewDesign.CurrentCell = GridViewDesign.Rows[0].Cells[0];
                toolStripLabel1.Text = lblDesignRowNumberIndicator.Text = "1 / " + GridViewDesign.Rows.Count.ToString();
            }
        }

        private void btn_Design_Pre_Click(object sender, EventArgs e)
        {
            if (GridViewDesign.Rows.Count > 1)
            {
                int PreRow = GridViewDesign.CurrentRow.Index - 1;
                PreRow = (PreRow > 0 ? PreRow : 0);
                GridViewDesign.CurrentCell = GridViewDesign.Rows[PreRow].Cells[0];
                toolStripLabel1.Text = lblDesignRowNumberIndicator.Text = (PreRow + 1).ToString() + " / " + GridViewDesign.Rows.Count.ToString();
            }
        }

        private void btn_Design_Next_Click(object sender, EventArgs e)
        {
            if (GridViewDesign.Rows.Count > 1)
            {
                int nRow = GridViewDesign.Rows.Count;
                int NextRow = GridViewDesign.CurrentRow.Index + 1;
                NextRow = (NextRow < nRow - 1 ? NextRow : nRow - 1);
                GridViewDesign.CurrentCell = GridViewDesign.Rows[NextRow].Cells[0];
                toolStripLabel1.Text = lblDesignRowNumberIndicator.Text = (NextRow + 1).ToString() + " / " + GridViewDesign.Rows.Count.ToString();
                
            }
        }

        private void btn_Design_End_Click(object sender, EventArgs e)
        {
            if (GridViewDesign.Rows.Count > 1)
            {
                int nRow = GridViewDesign.Rows.Count;
                GridViewDesign.CurrentCell = GridViewDesign.Rows[nRow - 1].Cells[0];
                toolStripLabel1.Text = lblDesignRowNumberIndicator.Text = nRow + " / " + nRow;
            }
        }

        private void Btn_Design_DeleteRow_Click(object sender, EventArgs e)
        {
            if (fdbEntity == null) return;
            if (GridViewDesign.Rows.Count > 1)
            {
                if (!GridViewDesign.Rows[currentRow].IsNewRow)
                {
                    GridViewDesign.Rows.Remove(GridViewDesign.CurrentRow);
                }

                toolStripLabel1.Text = lblDesignRowNumberIndicator.Text = GridViewDesign.CurrentRow.Index + 1 + " / " + GridViewDesign.Rows.Count;
            }
        }

        private void Btn_Design_ClearData_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = new DialogResult();
                result = MessageBox.Show("Clear all attributes data ?", "Clear All Data", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    //CloseCurrentScheme();
                    AddRowDefault();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR:\n" + ex.Message);
            }
        }

        private void Btn_Design_UpdateData_Click(object sender, EventArgs e)
        {
            SaveScheme();
        }
        
        #endregion

        #region 10. GridView Data Relation
        private void UpdateDataRowNumber()
        {
            try
            {
                if (GridViewData.Rows.Count == 0)
                {
                    lblDataRowNumberIndicator.Text = "0 / 0";
                }
                else if (GridViewData.CurrentRow != null)
                {
                    lblDataRowNumberIndicator.Text = (GridViewData.CurrentRow.Index + 1) + " / " + GridViewData.Rows.Count;
                }
                else lblDataRowNumberIndicator.Text = "1 / " + GridViewData.Rows.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR:\n" + ex.Message);
            }
        }

        private void btn_Data_Home_Click(object sender, EventArgs e)
        {
            if (GridViewData.Rows.Count > 1)// && !GridViewData.Rows[GridViewData.Rows.Count - 1].IsNewRow)
            {
                GridViewData.CurrentCell = GridViewData.Rows[0].Cells[0];
                lblDataRowNumberIndicator.Text = "1 / " + GridViewData.Rows.Count;
            }
        }

        private void btn_Data_Pre_Click(object sender, EventArgs e)
        {
            if (GridViewData.Rows.Count > 0 )//&& !GridViewData.Rows[GridViewData.Rows.Count - 1].IsNewRow)
            {
                int PreRow = GridViewData.CurrentRow.Index - 1;
                PreRow = (PreRow > 0 ? PreRow : 0);
                GridViewData.CurrentCell = GridViewData.Rows[PreRow].Cells[0];
                lblDataRowNumberIndicator.Text = (PreRow + 1) + " / " + GridViewData.Rows.Count;
            }
        }
        //edit----
        private void GridViewData_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            AutoCompleteStringCollection DataCollection = new AutoCompleteStringCollection();
            addItems(DataCollection);
            int column = GridViewData.CurrentCell.ColumnIndex;
            if (column == GridViewData.ColumnCount-1)
            {
                TextBox autoText = e.Control as TextBox;
                    autoText.AutoCompleteMode = AutoCompleteMode.Suggest;
                    autoText.AutoCompleteSource = AutoCompleteSource.CustomSource;
                    autoText.AutoCompleteCustomSource = DataCollection;
            }
            else
            {
                TextBox prodCode = e.Control as TextBox;
                    prodCode.AutoCompleteMode = AutoCompleteMode.None;
            }
        }
        public void addItems(AutoCompleteStringCollection col)
        {
            List<String> list = DiscreteFuzzySetBLL.ListOfDiscreteFNName(fdbEntity);
             list.AddRange ( ContinuousFuzzySetBLL.ListOfContinuousFNName(fdbEntity)); //edit this
            foreach (string n in list)
            {
                col.Add(n);
            }            
        }
        //------------------
        private void btn_Data_Next_Click(object sender, EventArgs e)
        {
            if (GridViewData.Rows.Count > 0)
            {
                int nRow = GridViewData.Rows.Count;
                int NextRow = GridViewData.CurrentRow.Index + 1;
                NextRow = (NextRow < nRow - 1 ? NextRow : nRow - 1);
                GridViewData.CurrentCell = GridViewData.Rows[NextRow].Cells[0];
                lblDataRowNumberIndicator.Text = (NextRow + 1) + " / " + GridViewData.Rows.Count;
            }
        }

        private void btn_Data_End_Click(object sender, EventArgs e)
        {
            if (GridViewData.Rows.Count > 0)
            {
                int nRow = GridViewData.Rows.Count;
                GridViewData.CurrentCell = GridViewData.Rows[nRow - 1].Cells[0];
                lblDataRowNumberIndicator.Text = nRow + " / " + nRow;
            }
        }

        private void Btn_Data_DeleteRow_Click(object sender, EventArgs e)
        {
            if (GridViewData.Rows.Count > 1 && !GridViewData.CurrentRow.IsNewRow)
            {
                GridViewData.Rows.Remove(GridViewData.CurrentRow);
                UpdateDataRowNumber();
            }
        }

        private void Btn_Data_ClearData_Click(object sender, EventArgs e)
        {
            DialogResult result = new DialogResult();
            result = MessageBox.Show("Are you sure want to clear all data?", "Clear All Data", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                int n = GridViewData.Rows.Count - 2;
                for (int i = n; i >= 0; i--)
                    if (!GridViewData.Rows[i].IsNewRow)
                        GridViewData.Rows.Remove(GridViewData.Rows[i]);
                UpdateDataRowNumber();
            }
        }

        private void Btn_Data_UpdateData_Click(object sender, EventArgs e)
        {
            SaveRelation();
        }

        private void GridViewData_Click(object sender, EventArgs e)
        {
            UpdateDataRowNumber();
        }

        private void GridViewData_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            UpdateDataRowNumber();
        }

        private void GridViewData_SelectionChanged(object sender, EventArgs e)
        {
            if (rollbackCell)
            {
                GridViewData.CurrentCell = GridViewData.Rows[currentRow].Cells[currentCell];
            }
            rollbackCell = false;
        }

        private void GridViewData_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            GridViewData.CommitEdit((DataGridViewDataErrorContexts.Commit));
        }

        private void GridViewData_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            currentCell = e.ColumnIndex;
            currentRow = e.RowIndex;
            try
            {
                if (GridViewData.CurrentCell.Value != null)
                {
                    var value = GridViewData.CurrentCell.Value.ToString();//.ToString();

                    ///Convert value of current cell to correct datatype of attribute
                    ///If cannot convert, focus to current cell and block focus to others cell
                    if (currentRelation != null && !FzDataTypeBLL.CheckDataType(value, currentRelation.Scheme.Attributes[currentCell].DataType)&& GridViewData.CurrentCell.ColumnIndex != GridViewData.ColumnCount - 1)
                    {
                            e.Cancel = true;
                            MessageBox.Show("Attribute value does not match with the data type!");
                            return;
                    }
                    if (!CheckPrimaryKey(e.RowIndex))
                    {
                        e.Cancel = true;
                        return;
                    }
                    if (!CheckMembership())
                    {
                        e.Cancel = true;

                        return;
                    }
                }
                else if (!GridViewData.Rows[GridViewData.Rows.Count - 1].IsNewRow)
                {
                    e.Cancel = true;
                    MessageBox.Show("Value can not be NULL!", "INFORM");
                }
            }
            catch (Exception ex) { }
        }

        private void GridViewData_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            //try
            //{
            //    currentCell = e.ColumnIndex;
            //    currentRow = e.RowIndex;

            //    if (GridViewData.CurrentCell.Value != null)
            //    {
            //        var value = GridViewData.CurrentCell.Value.ToString();//.ToString();

            //        ///Convert value of current cell to correct datatype of attribute
            //        ///If cannot convert, focus to current cell and block focus to others cell
            //        if (!FzDataTypeBLL.CheckDataType(value, currentRelation.Scheme.Attributes[currentCell].DataType))
            //        {
            //            MessageBox.Show("Attribute value does not match with the data type!");
            //            return;
            //        }
            //    }
            //    else
            //    {
            //        throw new Exception("NULL!");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("ERROR:\n" + ex.Message);
            //    rollbackCell = true;
            //}
        }
        private List<Double> SplitString(String str)
        {
            List<Double> result = new List<double>();

            ///Remove "{", "}" and ","
            String tmp = str.Replace("{", "");
            tmp = tmp.Replace("}", "");
            Char[] seperator = { ',' };
            String[] values = tmp.Split(seperator);

            ///Add value to list after remove unesessary
            foreach (var value in values)
            {
                result.Add(Convert.ToDouble(value));
            }

            return result;
        }
        public bool IsNumber(string pText)
        {
            Regex regex = new Regex(@"^[-+]?[0-9]*\.?[0-9]+$");
            return regex.IsMatch(pText);
        }
        private Boolean CheckMembership()
            {
            int n = GridViewData.Rows.Count - 1;
            for (int i = 0; i < n; i++)
            {
                //Datatype has been checked above
                string membership = GridViewData.Rows[i].Cells[GridViewData.ColumnCount - 1].Value.ToString();
                if (IsNumber(membership))
                {
                    Double value = Double.Parse(membership);

                    if (value > 1 || value <= 0)
                    {
                        MessageBox.Show("The membership value at row " + (i + 1) + " must be between (0-1]");
                        return false;
                    }

                }
                else
                {
                    List<String> list = DiscreteFuzzySetBLL.ListOfDiscreteFNName(fdbEntity);
                    list.AddRange(ContinuousFuzzySetBLL.ListOfContinuousFNName(fdbEntity));
                    if (!list.Any(item => item == membership))
                    {
                        frmMessBoxCreateFN frm = new frmMessBoxCreateFN();
                        frm.ShowDialog();
                        //After form close
                        string select = frm.select;
                        if (select != "cancel")
                        {
                            if (select == "dis")
                            {
                                frmDescreteEditor frmDis = new frmDescreteEditor(fdbEntity.DiscreteFuzzyNumbers, membership, true);
                                frmDis.ShowDialog();
                                fdbEntity.DiscreteFuzzyNumbers = frmDis.DiscreteFSs;
                            }
                            else
                            {
                                frmContinuousEditor frmCon = new frmContinuousEditor(fdbEntity.ContinuousFuzzyNumbers, membership, true);
                                frmCon.ShowDialog();
                                fdbEntity.ContinuousFuzzyNumbers = frmCon.ContinuousFSs;
                            }
                            DBValues.disFNName = DiscreteFuzzySetBLL.ListOfDiscreteFNName(fdbEntity);
                            DBValues.conFNName = ContinuousFuzzySetBLL.ListOfContinuousFNName(fdbEntity);
                            ShowTreeList();
                            ShowTreeListNode();
                            treeList1.ExpandAll();
                        }
                    }
                }
            }

            return true;
        }

        private Boolean CheckPrimaryKey(int row)//Current relation only allow one primarykey
        {
            List<int> indexPrm = FzRelationBLL.GetArrPrimaryKey(currentRelation);
            string value = GridViewData.Rows[row].Cells[indexPrm[0]].Value.ToString() ;
            for (int i = 0; i < GridViewData.Rows.Count - 1; i++)
            {
                if (value ==  GridViewData.Rows[i].Cells[indexPrm[0]].Value.ToString() && i!= row)
                {
                    MessageBox.Show("The primary value must be unique");
                    return false;
                }
            }

            return true;
        }
        
        #endregion

        #region 11. Help and About
        private void barButtonItem13_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            (new frmAbout(false)).Show(); 
        }

        private void iHelp_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            frmHelp frm = new frmHelp();
            frm.ShowDialog();
        }
        #endregion

    }
}