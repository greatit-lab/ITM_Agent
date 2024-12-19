namespace ITM_Agent.ucPanel
{
    partial class ucConfigurationPanel
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.GroupBox gb_TargetFolders;
        private System.Windows.Forms.ListBox lb_TargetList;
        private System.Windows.Forms.Button btn_TargetFolder;
        private System.Windows.Forms.Button btn_TargetRemove;

        private System.Windows.Forms.GroupBox gb_ExcludeFolders;
        private System.Windows.Forms.ListBox lb_ExcludeList;
        private System.Windows.Forms.Button btn_ExcludeFolder;
        private System.Windows.Forms.Button btn_ExcludeRemove;

        private System.Windows.Forms.GroupBox gb_BaseFolder;
        private System.Windows.Forms.TextBox lb_BaseFolder;
        private System.Windows.Forms.Button btn_BaseFolder;

        private System.Windows.Forms.GroupBox gb_Regex;
        private System.Windows.Forms.ListBox lb_RegexList;
        private System.Windows.Forms.Button btn_RegAdd;
        private System.Windows.Forms.Button btn_RegEdit;
        private System.Windows.Forms.Button btn_RegRemove;

        private void InitializeComponent()
        {
            this.gb_TargetFolders = new System.Windows.Forms.GroupBox();
            this.lb_TargetList = new System.Windows.Forms.ListBox();
            this.btn_TargetFolder = new System.Windows.Forms.Button();
            this.btn_TargetRemove = new System.Windows.Forms.Button();

            this.gb_ExcludeFolders = new System.Windows.Forms.GroupBox();
            this.lb_ExcludeList = new System.Windows.Forms.ListBox();
            this.btn_ExcludeFolder = new System.Windows.Forms.Button();
            this.btn_ExcludeRemove = new System.Windows.Forms.Button();

            this.gb_BaseFolder = new System.Windows.Forms.GroupBox();
            this.lb_BaseFolder = new System.Windows.Forms.TextBox();
            this.btn_BaseFolder = new System.Windows.Forms.Button();

            this.gb_Regex = new System.Windows.Forms.GroupBox();
            this.lb_RegexList = new System.Windows.Forms.ListBox();
            this.btn_RegAdd = new System.Windows.Forms.Button();
            this.btn_RegEdit = new System.Windows.Forms.Button();
            this.btn_RegRemove = new System.Windows.Forms.Button();

            // Target Folders GroupBox
            this.gb_TargetFolders.Controls.Add(this.lb_TargetList);
            this.gb_TargetFolders.Controls.Add(this.btn_TargetFolder);
            this.gb_TargetFolders.Controls.Add(this.btn_TargetRemove);
            this.gb_TargetFolders.Location = new System.Drawing.Point(10, 10);
            this.gb_TargetFolders.Size = new System.Drawing.Size(400, 150);
            this.gb_TargetFolders.Text = "Target Folders";

            this.lb_TargetList.Location = new System.Drawing.Point(10, 20);
            this.lb_TargetList.Size = new System.Drawing.Size(270, 95);

            this.btn_TargetFolder.Location = new System.Drawing.Point(290, 20);
            this.btn_TargetFolder.Size = new System.Drawing.Size(90, 30);
            this.btn_TargetFolder.Text = "Add";

            this.btn_TargetRemove.Location = new System.Drawing.Point(290, 60);
            this.btn_TargetRemove.Size = new System.Drawing.Size(90, 30);
            this.btn_TargetRemove.Text = "Remove";

            // Exclude Folders GroupBox
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
