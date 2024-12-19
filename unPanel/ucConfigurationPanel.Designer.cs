namespace ITM_Agent.ucPanel
{
    partial class ucConfigurationPanel
    {
        private System.ComponentModel.IContainer components = null;
        
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btn_TargetRemove;
        private System.Windows.Forms.Button btn_TargetFolder;
        private System.Windows.Forms.ListBox lb_TargetList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_ExcludeRemove;
        private System.Windows.Forms.Button btn_ExcludeFolder;
        private System.Windows.Forms.ListBox lb_ExcludeList;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btn_BaseFolder;
        private System.Windows.Forms.Label lb_BaseFolder;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btn_RegRemove;
        private System.Windows.Forms.Button btn_RegEdit;
        private System.Windows.Forms.Button btn_RegAdd;
        private ucOverrideNamesPanel ucOverrideNamesPanel;
        private System.Windows.Forms.ListBox lb_RegexList;
        
        #region 구성 요소 디자이너에서 생성한 코드

        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btn_TargetRemove = new System.Windows.Forms.Button();
            this.btn_TargetFolder = new System.Windows.Forms.Button();
            this.lb_TargetList = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_ExcludeRemove = new System.Windows.Forms.Button();
            this.btn_ExcludeFolder = new System.Windows.Forms.Button();
            this.lb_ExcludeList = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btn_BaseFolder = new System.Windows.Forms.Button();
            this.lb_BaseFolder = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btn_RegRemove = new System.Windows.Forms.Button();
            this.btn_RegEdit = new System.Windows.Forms.Button();
            this.btn_RegAdd = new System.Windows.Forms.Button();
            this.lb_RegexList = new System.Windows.Forms.ListBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            //
            // tabControl1
            //
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 6);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(653, 299);
            this.tabControl1.TabIndex = 14;
            //
            // tabPage1
            //
            this.tabPage1.Controls.Add(this.splitContainer2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(645, 273);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Categorize";
            this.tabPage1.UseVisualStyleBackColor = true;
            //
            // splitContainer2
            //
            this.splitContainer2.Location = new System.Drawing.Point(6, 6);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitContainer2.Panel1
            //
            this.splitContainer2.Panel1.Controls.Add(this.groupBox1);
            //
            // splitContainer2.Panel2
            //
            this.splitContainer2.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer2.Size = new System.Drawing.Size(632, 264);
            this.splitContainer2.SplitterDistance = 201;
            this.splitContainer2.TabIndex = 1;
            //
            // groupBox1
            //
            this.groupBox1.Controls.Add(this.splitContainer1);
            this.groupBox1.Location = new System.Drawing.Point(3, 9);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(624, 187);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "● Folders to Monitor";
            //
            // splitContainer1
            //
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Ponit(3, 17);
            this.splitContainer1.Name = "splitContainer1";
            //
            // splitContainer1.Panel1
            //
            this.splitContainer1.Panel1.Controls.Add(this.btn_TargetRemove);
            this.splitContainer1.Panel1.Controls.Add(this.btn_TargetFolder);
            this.splitContainer1.Panel1.Controls.Add(this.lb_TargetList);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            //
            // splitContainer1.Panel2
            //
            this.splitContainer1.Panel2.Controls.Add(this.btn_ExcludeRemove);
            this.splitContainer1.Panel2.Controls.Add(this.btn_ExcludeFolder);
            this.splitContainer1.Panel2.Controls.Add(this.lb_ExcludeList);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Size = new System.Drawing.Size(618, 167);
            this.splitContainer1.SplitterDistance = 307;
            this.splitContainer1.TabIndex = 0;
            
                   
            
              
            
            this.splitContainer1.SplitterDistance = 307;//
            // Exclude Folders GroupBox
            //
            this.gb_ExcludeFolders.Controls.Add(this.lb_ExcludeList);
            this.gb_ExcludeFolders.Controls.Add(this.btn_ExcludeFolder);
            this.gb_ExcludeFolders.Controls.Add(this.btn_ExcludeRemove);
            this.gb_ExcludeFolders.Location = new System.Drawing.Point(10, 170);
            this.gb_ExcludeFolders.Size = new System.Drawing.Size(400, 150);
            this.gb_ExcludeFolders.Text = "Exclude Folders";

            this.lb_ExcludeList.Location = new System.Drawing.Point(10, 20);
            this.lb_ExcludeList.Size = new System.Drawing.Size(270, 95);

            this.btn_ExcludeFolder.Location = new System.Drawing.Point(290, 20);
            this.btn_ExcludeFolder.Size = new System.Drawing.Size(90, 30);
            this.btn_ExcludeFolder.Text = "Add";

            this.btn_ExcludeRemove.Location = new System.Drawing.Point(290, 60);
            this.btn_ExcludeRemove.Size = new System.Drawing.Size(90, 30);
            this.btn_ExcludeRemove.Text = "Remove";

            // Base Folder GroupBox
            this.gb_BaseFolder.Controls.Add(this.lb_BaseFolder);
            this.gb_BaseFolder.Controls.Add(this.btn_BaseFolder);
            this.gb_BaseFolder.Location = new System.Drawing.Point(10, 330);
            this.gb_BaseFolder.Size = new System.Drawing.Size(400, 70);
            this.gb_BaseFolder.Text = "Base Folder";

            this.lb_BaseFolder.Location = new System.Drawing.Point(10, 30);
            this.lb_BaseFolder.Size = new System.Drawing.Size(270, 20);

            this.btn_BaseFolder.Location = new System.Drawing.Point(290, 30);
            this.btn_BaseFolder.Size = new System.Drawing.Size(90, 30);
            this.btn_BaseFolder.Text = "Select";

            // Regex GroupBox
            this.gb_Regex.Controls.Add(this.lb_RegexList);
            this.gb_Regex.Controls.Add(this.btn_RegAdd);
            this.gb_Regex.Controls.Add(this.btn_RegEdit);
            this.gb_Regex.Controls.Add(this.btn_RegRemove);
            this.gb_Regex.Location = new System.Drawing.Point(10, 410);
            this.gb_Regex.Size = new System.Drawing.Size(400, 150);
            this.gb_Regex.Text = "Regex Patterns";

            this.lb_RegexList.Location = new System.Drawing.Point(10, 20);
            this.lb_RegexList.Size = new System.Drawing.Size(270, 95);

            this.btn_RegAdd.Location = new System.Drawing.Point(290, 20);
            this.btn_RegAdd.Size = new System.Drawing.Size(90, 30);
            this.btn_RegAdd.Text = "Add";

            this.btn_RegEdit.Location = new System.Drawing.Point(290, 60);
            this.btn_RegEdit.Size = new System.Drawing.Size(90, 30);
            this.btn_RegEdit.Text = "Edit";

            this.btn_RegRemove.Location = new System.Drawing.Point(290, 100);
            this.btn_RegRemove.Size = new System.Drawing.Size(90, 30);
            this.btn_RegRemove.Text = "Remove";

            // ucConfigurationPanel
            this.Controls.Add(this.gb_TargetFolders);
            this.Controls.Add(this.gb_ExcludeFolders);
            this.Controls.Add(this.gb_BaseFolder);
            this.Controls.Add(this.gb_Regex);
            this.Size = new System.Drawing.Size(420, 580);
        }
    }
}
