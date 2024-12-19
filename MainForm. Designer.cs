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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem overrideNamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem imageTransToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uploadDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 파일ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
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
        private System.Windows.Forms.ListBox lb_RegexList;
        private System.Windows.Forms.Button btn_TargetFolder;
        private System.Windows.Forms.Button btn_TargetRemove;
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
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
        
        #region Windows Form 디자이너에서 생성한 코드
        
        private void InitializeComponent()
        {
            this.cb_DebugMode = new System.Windows.Forms.CheckBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.파일ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_Categorize = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.tsm_Option = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_Onto = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_ImageTrans = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_UploadData = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_OverrideNames = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_Nova = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.도움말ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.내용ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.newConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.overrideNamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageTransToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uploadDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.lb_eqpid = new System.Windows.Forms.Label();
            this.pMain = new System.Windows.Forms.Panel();
            this.btn_Quit = new System.Windows.Forms.Button();
            this.btn_Stop = new System.Windows.Forms.Button();
            this.btn_Run = new System.Windows.Forms.Button();
            this.ts_Status = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lb_regexPatterns = new.System.Windows.Forms.ListBox();
            this.lb_BaseFolder = new.System.Windows.Forms.ListBox();
            this.lb_TargetList = new.System.Windows.Forms.ListBox();
            this.lb_ExcludeList = new.System.Windows.Forms.ListBox();
            this.lb_RegexList = new.System.Windows.Forms.ListBox();
            this.lb_TargetFolders = new.System.Windows.Forms.ListBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            //
            // cb_DebugMode
            //
            this.cb_DebugMode.AutoSize = true;
            this.cb_DebugMode.Location = new System.Drawing.Point(569, 12);
            this.cb_DebugMode.Name = "cb_DebugMode";
            this.cb_DebugMode.Size = new System.Drawing.Size(96, 16);
            this.cb_DebugMode.TabIndex = 0;
            this.cb_DebugMode.Text = "Debug Mode";
            this.cb_DebugMode.UseVisualStyleBackColor = true;
            //
            // menuStrip11
            //
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                this.파일ToolStripMenuItem1,
                this.toolStripMenuItem8,
                this.tsm_Onto,
                this.tsm_Nova,
                this.도움말ToolStripMenuItem1
            });
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(676, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            //
            // 파일ToolStripMenuItem1
            //
            this.파일ToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                this.newToolStripMenuItem,
                this.openToolStripMenuItem,
                this.toolStripSeparator7,
                this.saveAsToolStripMenuItem,
                this.toolStripSeparator8,
                this.quitToolStripMenuItem
            });
            this.파일ToolStripMenuItem1.Name = "파일ToolStripMenuItem1";
            this.파일ToolStripMenuItem1.Size = new System.Drawing.Size(37, 20);
            this.파일ToolStripMenuItem1.Text = "File";
            //
            // newToolStripMenuItem
            //
            this.newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Megenta;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.NewMenuItem_Click);
            //
            // openToolStripMenuItem
            //
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Megenta;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenMenuItem_Click);
            //
            // toolStripSeparator7
            //
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(177, 6);
            //
            // Onto Menu
            //
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
