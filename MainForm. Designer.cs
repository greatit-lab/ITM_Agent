using ITM_Agent.Services;

namespace ITM_Agent
{
    partial class MainForm
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
        
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsm_Onto;
        private System.Windows.Forms.ToolStripMenuItem newConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator ToolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem overrideNamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem imageTransToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uploadDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 파일ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator ToolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem
        private System.Windows.Forms.ToolStripSeparator ToolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem
        private System.Windows.Forms.ToolStripMenuItem tsm_OverrideNames;
        private System.Windows.Forms.ToolStripMenuItem tsm_ImageTrans;
        private System.Windows.Forms.ToolStripMenuItem tsm_UploadData;
        private System.Windows.Forms.ToolStripMenuItem 도움말ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem 내용ToolStripMenuItem1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.CheckBox cb_DebugMode;
        private System.Windows.Forms.Button btn_Quit;
        private System.Windows.Forms.Button btn_Stop;
        private System.Windows.Forms.Button btn_Run;
        private System.Windows.Forms.Panel pMain;
        private System.Windows.Forms.Label lb_eqpid;
        private System.Windows.Forms.ToolStripStatusLabel ts_Status;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ListBox lb_TargetFolders;
        private System.Windows.Forms.ListBox lb_regexPatterns;
        private System.Windows.Forms.ListBox lb_TargetList;
        private System.Windows.Forms.ListBox lb_ExcludeList;
        private System.Windows.Forms.Label lb_BaseFolder;
        private System.Windows.Forms.Button btn_TargetFolder;
        private System.Windows.Forms.Button btn_TargerRemove;
        private System.Windows.Forms.Button btn_ExcludeFolder;
        private System.Windows.Forms.Button btn_ExcludeRemove;
        private System.Windows.Forms.Button btn_RegAdd;
        private System.Windows.Forms.Button btn_RegEdit;
        private System.Windows.Forms.Button btn_RegRemove;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem8;
        private System.Windows.Forms.ToolStripMenuItem tsm_Categorize;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem tsm_Option;
        private System.Windows.Forms.ToolStripMenuItem tsm_Nova;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem6;
        
        #region Windows From 디자이너에서 생성한 코드
        
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.tsm_Onto = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_Nova = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_Categorize = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_OverrideNames = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_ImageTrans = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_UploadData = new System.Windows.Forms.ToolStripMenuItem();

            this.pMain = new System.Windows.Forms.Panel();
            this.lb_eqpid = new System.Windows.Forms.Label();
            this.btn_Run = new System.Windows.Forms.Button();
            this.btn_Stop = new System.Windows.Forms.Button();
            this.btn_Quit = new System.Windows.Forms.Button();

            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.ts_Status = new System.Windows.Forms.ToolStripStatusLabel();

            // MenuStrip
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                this.fileMenuItem,
                this.tsm_Onto,
                this.tsm_Nova
            });
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);

            // File Menu
            this.fileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                this.newMenuItem,
                this.openMenuItem,
                this.saveAsMenuItem,
                this.quitMenuItem
            });
            this.fileMenuItem.Name = "fileMenuItem";
            this.fileMenuItem.Text = "File";

            this.newMenuItem.Name = "newMenuItem";
            this.newMenuItem.Text = "New";

            this.openMenuItem.Name = "openMenuItem";
            this.openMenuItem.Text = "Open";

            this.saveAsMenuItem.Name = "saveAsMenuItem";
            this.saveAsMenuItem.Text = "Save As";

            this.quitMenuItem.Name = "quitMenuItem";
            this.quitMenuItem.Text = "Quit";

            // Onto Menu
            this.tsm_Onto.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                this.tsm_Categorize,
                this.tsm_OverrideNames,
                this.tsm_ImageTrans,
                this.tsm_UploadData
            });
            this.tsm_Onto.Name = "tsm_Onto";
            this.tsm_Onto.Text = "ONTO";

            this.tsm_Categorize.Name = "tsm_Categorize";
            this.tsm_Categorize.Text = "Categorize";

            this.tsm_OverrideNames.Name = "tsm_OverrideNames";
            this.tsm_OverrideNames.Text = "Override Names";

            this.tsm_ImageTrans.Name = "tsm_ImageTrans";
            this.tsm_ImageTrans.Text = "Image Trans";

            this.tsm_UploadData.Name = "tsm_UploadData";
            this.tsm_UploadData.Text = "Upload Data";

            // Nova Menu
            this.tsm_Nova.Name = "tsm_Nova";
            this.tsm_Nova.Text = "NOVA";

            // Main Panel
            this.pMain.Location = new System.Drawing.Point(10, 50);
            this.pMain.Name = "pMain";
            this.pMain.Size = new System.Drawing.Size(780, 400);

            // Eqpid Label
            this.lb_eqpid.AutoSize = true;
            this.lb_eqpid.Location = new System.Drawing.Point(10, 30);
            this.lb_eqpid.Name = "lb_eqpid";
            this.lb_eqpid.Text = "Eqpid: ";

            // Run Button
            this.btn_Run.Location = new System.Drawing.Point(10, 470);
            this.btn_Run.Name = "btn_Run";
            this.btn_Run.Size = new System.Drawing.Size(100, 30);
            this.btn_Run.Text = "Run";

            // Stop Button
            this.btn_Stop.Location = new System.Drawing.Point(120, 470);
            this.btn_Stop.Name = "btn_Stop";
            this.btn_Stop.Size = new System.Drawing.Size(100, 30);
            this.btn_Stop.Text = "Stop";

            // Quit Button
            this.btn_Quit.Location = new System.Drawing.Point(230, 470);
            this.btn_Quit.Name = "btn_Quit";
            this.btn_Quit.Size = new System.Drawing.Size(100, 30);
            this.btn_Quit.Text = "Quit";

            // StatusStrip
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                this.ts_Status
            });
            this.statusStrip.Location = new System.Drawing.Point(0, 510);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(800, 22);

            this.ts_Status.Name = "ts_Status";
            this.ts_Status.Size = new System.Drawing.Size(39, 17);
            this.ts_Status.Text = "Ready";

            // MainForm
            this.ClientSize = new System.Drawing.Size(800, 550);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.pMain);
            this.Controls.Add(this.lb_eqpid);
            this.Controls.Add(this.btn_Run);
            this.Controls.Add(this.btn_Stop);
            this.Controls.Add(this.btn_Quit);
            this.Controls.Add(this.statusStrip);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "ITM Agent";
        }
    }
}
