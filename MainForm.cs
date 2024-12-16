// MainForm.cs
using ITM_Agent.Services;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ITM_Agent
{
    public partial class MainForm : Form
    {
        private SettingsManager settingsManager;
        private LogManager logManager;
        private FileWatcherManager fileWatcherManager;
        private EqpidManager eqpidManager;

        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private ToolStripMenuItem titleItem;
        private ToolStripMenuItem runItem;
        private ToolStripMenuItem stopItem;
        private ToolStripMenuItem quitItem;

        private const string AppVersion = "v1.0.0";
        
        ucPanel.ucConfigurationPanel ucSc1;

        public MainForm(SettingsManager settingsManager)
        {
            this.settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            InitializeComponent();
        
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        
            settingsManager = new SettingsManager(Path.Combine(baseDir, "Settings.ini"));
            
            // settingsManager 인스턴스를 생성자 인자로 전달
            ucSc1 = new ucPanel.ucConfigurationPanel(settingsManager);
            
            logManager = new LogManager(baseDir);
            fileWatcherManager = new FileWatcherManager(settingsManager, logManager);
            eqpidManager = new EqpidManager(settingsManager);
        
            this.Text = $"ITM Agent - {AppVersion}";
            this.MaximizeBox = false;
        
            InitializeTrayIcon();
            this.FormClosing += MainForm_FormClosing;
        
            eqpidManager.InitializeEqpid(); 
            // Eqpid 초기화 완료 후 eqpid 값 가져오기
            string eqpid = settingsManager.GetEqpid();
            if (!string.IsNullOrEmpty(eqpid))
            {
                ProceedWithMainFunctionality(eqpid);
            }
        
            fileWatcherManager.InitializeWatchers();
        
            btn_Run.Click += btn_Run_Click;
            btn_Stop.Click += btn_Stop_Click;
            btn_Quit.Click += btn_Quit_Click;
        
            UpdateUIBasedOnSettings();
        }
        
        private void ProceedWithMainFunctionality(string eqpid)
        {
            lb_eqpid.Text = $"Eqpid: {eqpid}";
        }
        
        private void InitializeTrayIcon()
        {
            trayMenu = new ContextMenuStrip();

            titleItem = new ToolStripMenuItem(this.Text);
            titleItem.Click += (sender, e) => RestoreMainForm();
            trayMenu.Items.Add(titleItem);

            trayMenu.Items.Add(new ToolStripSeparator());

            runItem = new ToolStripMenuItem("Run", null, (sender, e) => btn_Run.PerformClick());
            trayMenu.Items.Add(runItem);

            stopItem = new ToolStripMenuItem("Stop", null, (sender, e) => btn_Stop.PerformClick());
            trayMenu.Items.Add(stopItem);

            quitItem = new ToolStripMenuItem("Quit", null, (sender, e) => PerformQuit());
            trayMenu.Items.Add(quitItem);

            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                ContextMenuStrip = trayMenu,
                Visible = true,
                Text = this.Text
            };
            trayIcon.DoubleClick += (sender, e) => RestoreMainForm();
        }

        private void RestoreMainForm()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
            titleItem.Enabled = false;
        }

        private void UpdateTrayMenuStatus()
        {
            if (runItem != null) runItem.Enabled = btn_Run.Enabled;
            if (stopItem != null) stopItem.Enabled = btn_Stop.Enabled;
            if (quitItem != null) quitItem.Enabled = btn_Quit.Enabled;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            fileWatcherManager.StopWatchers();
            trayIcon?.Dispose();
            Environment.Exit(0);
        }

        private void UpdateUIBasedOnSettings()
        {
            if (settingsManager.IsReadyToRun())
            {
                UpdateMainStatus("Ready to Run", Color.Green);
            }
            else
            {
                UpdateMainStatus("Stopped!", Color.Red);
            }
        }

        private void UpdateMainStatus(string status, Color color)
        {
            ts_Status.Text = status;
            ts_Status.ForeColor = color;
            
            bool isRunning = status == "Running...";
            
            switch (status)
            {
                case "Ready to Run":
                    btn_Run.Enabled = true;
                    btn_Stop.Enabled = false;
                    btn_Quit.Enabled = true;
                    break;
                case "Stopped!":
                    btn_Run.Enabled = true;
                    btn_Stop.Enabled = false;
                    btn_Quit.Enabled = true;
                    break;
                case "Running...":
                    btn_Run.Enabled = false;
                    btn_Stop.Enabled = true;
                    btn_Quit.Enabled = false;
                    break;
            }
            UpdateTrayMenuStatus();
            UpdateMenuItemsState(isRunning); // 메뉴 활성/비활성화
        }
        
        private void UpdateMenuItemsState(bool isRunning)
        {
            if (menuStrip1 != null)
            {
                foreach (ToolStripMenuItem item in menuStrip1.Items)
                {
                    if (item.Text == "File")
                    {
                        foreach (ToolStripItem subItem in item.DropDownItems)
                        {
                            if (subItem.Text == "New" || subItem.Text == "Open" || subItem.Text == "Quit")
                            {
                                subItem.Enabled = !isRunning; // Running 상태에서 비활성화
                            }
                        }
                    }
                }
            }
        }
        
        private void btn_Run_Click(object sender, EventArgs e)
        {
            logManager.LogEvent("Run button clicked.");
            try
            {
                UpdateMainStatus("Running...", Color.Blue);
                fileWatcherManager.StartWatching();
            }
            catch (Exception ex)
            {
                logManager.LogEvent($"Error starting monitoring: {ex.Message}", true);
                UpdateMainStatus("Stopped!", Color.Red);
            }
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            logManager.LogEvent("Stop button clicked.");
            fileWatcherManager.StopWatchers();
            UpdateMainStatus("Stopped!", Color.Red);
        }

        private void btn_Quit_Click(object sender, EventArgs e)
        {
            if (ts_Status.Text == "Ready to Run" || ts_Status.Text == "Stopped!")
            {
                PerformQuit();
            }
            else
            {
                MessageBox.Show("실행 중에는 종료할 수 없습니다. 먼저 작업을 중지하세요.",
                                "종료 불가",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }

        private void PerformQuit()
        {
            fileWatcherManager.StopWatchers();
            trayIcon?.Dispose();
            Environment.Exit(0);
        }
        
        private void categorizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 이벤트 발생 시 처리할 코드 작성
            MessageBox.Show("Categorize 메뉴 클릭!");
        }
        
        private void cb_DebugMode_CheckedChanged(object sender, EventArgs e)
        {
            // Debug 모드 체크박스 상태 변경 시 처리할 로직을 추가
            if (cb_DebugMode.Checked)
            {
                // Debug 모드 활성화 로직
            }
            else
            {
                // Debug 모드 비활성화 로직
            }
        }
        
        private void imageTransToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // 이 메뉴 클릭 시 처리할 로직 추가
            // 예: MessageBox.Show("Image Trans 메뉴 클릭됨!");
        }
        
        private void optionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // option 메뉴 클릭 시 처리할 로직
            // 예: MessageBox.Show("Option 메뉴를 클릭했습니다!");
        }
        
        private void uploadDataToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // uploadData 메뉴 클릭 시 처리할 로직
            // 예: MessageBox.Show("Upload Data 메뉴를 클릭했습니다!");
        }
        
        private void overrideNamesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // overrideNames 메뉴 클릭 시 처리할 로직
            // 예: MessageBox.Show("Override Names 메뉴를 클릭했습니다!");
        }
        
        private void MainForm_Load(object sender, EventArgs e)
        {
            // 폼 로드시 실행할 로직
            pMain.Controls.Add(ucSc1);
        }
        
        private void InitializeMenu()
        {
            menuStrip1 = new MenuStrip();
        
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
        
            ToolStripMenuItem newMenuItem = new ToolStripMenuItem("New");
            newMenuItem.Click += NewMenuItem_Click;
            fileMenu.DropDownItems.Add(newMenuItem);
        
            ToolStripMenuItem openMenuItem = new ToolStripMenuItem("Open");
            openMenuItem.Click += OpenMenuItem_Click;
            fileMenu.DropDownItems.Add(openMenuItem);
        
            ToolStripMenuItem saveAsMenuItem = new ToolStripMenuItem("Save As");
            saveAsMenuItem.Click += SaveAsMenuItem_Click;
            fileMenu.DropDownItems.Add(saveAsMenuItem);
        
            ToolStripMenuItem quitMenuItem = new ToolStripMenuItem("Quit");
            quitMenuItem.Click += QuitMenuItem_Click;
            fileMenu.DropDownItems.Add(quitMenuItem);
        
            menuStrip1.Items.Add(fileMenu);
        
            this.Controls.Add(menuStrip1);
            this.MainMenuStrip = menuStrip1;
        }
        
        private void RefreshUI()
        {
            // Eqpid 상태 갱신
            string eqpid = settingsManager.GetEqpid();
            lb_eqpid.Text = $"Eqpid: {eqpid}";
        
            // TargetFolders, Regex 리스트 갱신
            ucSc1.RefreshUI(); // UserControl의 UI 갱신 호출
        
            // MainForm 상태 업데이트
            UpdateUIBasedOnSettings();
        }
        
        private void NewMenuItem_Click(object sender, EventArgs e)
        {
            // Settings 초기화 (Eqpid 제외)
            settingsManager.ResetExceptEqpid();
            MessageBox.Show("Settings 초기화 완료 (Eqpid 제외)", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
        
            RefreshUI(); // 초기화 후 UI 갱신
        }
        
        private void OpenMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "INI files (*.ini)|*.ini|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        settingsManager.LoadFromFile(openFileDialog.FileName);
                        MessageBox.Show("새로운 Settings.ini 파일이 로드되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
        
                        RefreshUI(); // 파일 로드 후 UI 갱신
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"파일 로드 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        private void SaveAsMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "INI files (*.ini)|*.ini|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        settingsManager.SaveToFile(saveFileDialog.FileName);
                        MessageBox.Show("Settings.ini가 저장되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"파일 저장 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        private void QuitMenuItem_Click(object sender, EventArgs e)
        {
            if (ts_Status.Text == "Running...")
            {
                MessageBox.Show("실행 중에는 종료할 수 없습니다. 작업을 중지하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        
            Application.Exit();
        }
    }
}
