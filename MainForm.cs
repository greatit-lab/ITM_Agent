// MainForm.cs
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ITM_Agent.Services;
using ITM_Agent.ucPanel;

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
        
        private ucConfigurationPanel ucConfigPanel;
        private ucOverrideNamesPanel ucOverrideNamesPanel;
        private ucScreen3 ucImageTransPanel;
        private ucScreen4 ucUploadDataPanel;
        
        private bool isRunning = false; // 현재 상태 플래그

        public MainForm(SettingsManager settingsManager)
        {
            this.settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            InitializeComponent();
            
            InitializeUserControls();
            RegisterMenuEvents();
        
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        
            settingsManager = new SettingsManager(Path.Combine(baseDir, "Settings.ini"));
            
            // settingsManager 인스턴스를 생성자 인자로 전달
            ucSc1 = new ucPanel.ucConfigurationPanel(settingsManager);
            ucOverrideNamesPanel = new ucOverrideNamesPanel(settingsManager);
            
            logManager = new LogManager(baseDir);
            
            // fileWatcherManager 생성 시 isDebugMode 전달
            fileWatcherManager = new FileWatcherManager(settingsManager, logManager, isDebugMode);
            eqpidManager = new EqpidManager(settingsManager);
            
            // icon 설정
            SetFormIcon();
            
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
        
        private void SetFormIcon()
        {
            // 제목줄 아이콘 설정
            this.Icon = new Icon(@"Resources\Icons\icon.ico");
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
                Icon = new Icon(@"Resources\Icons\icon.ico"), // TrayIcon에 사용할 아이콘
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
            titleItem.Enabled = false;  // 트레이 메뉴 비활성화
        }

        private void UpdateTrayMenuStatus()
        {
            if (runItem != null) runItem.Enabled = btn_Run.Enabled;
            if (stopItem != null) stopItem.Enabled = btn_Stop.Enabled;
            if (quitItem != null) quitItem.Enabled = btn_Quit.Enabled;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing) // X 버튼 클릭 시
            {
                e.Cancel = true; // 종료 방지
                this.Hide(); // 폼을 숨김
                trayIcon.BalloonTipTitle = "ITM Agent";
                trayIcon.BalloonTipText = "ITM Agent가 백그라운드에서 실행 중입니다.";
                trayIcon.ShowBalloonTip(3000); // 3초 동안 풍선 도움말 표시
            }
            else
            {
                // 강제 종료 등 다른 이유로 닫힐 때 처리
                fileWatcherManager.StopWatchers();
                trayIcon?.Dispose();
                Environment.Exit(0);
            }
        }
        
        private void UpdateUIBasedOnSettings()
        {
            // SettingsManager의 IsReadyToRun 결과에 따라 상태 업데이트
            if (settingsManager.IsReadyToRun())
            {
                UpdateMainStatus("Ready to Run", Color.Green);
                btn_Run.Enabled = true; // Run 버튼 활성화
            }
            else
            {
                UpdateMainStatus("Stopped!", Color.Red);
                btn_Run.Enabled = false; // Run 버튼 비활성화
            }
            btn_Stop.Enabled = false; // 초기 상태에서 Stop 버튼 비활성화
            btn_Quit.Enabled = true;  // Quit 버튼 활성화
        }
        
        private void UpdateMainStatus(string status, Color color)
        {
            ts_Status.Text = status;
            ts_Status.ForeColor = color;
            
            bool isRunning = status == "Running...";
            // ucSc1.UpdateStatusOnRun(isRunning); // 상태를 UserControl에 전달
            ucOverrideNamesPanel?.UpdateStatus(status); // 상태 없데이트 반영
            ucConfigPanel?.UpdateStatusOnRun(isRunning);
            ucOverrideNamesPanel?.UpdateStatusOnRun(isRunning);
            
            // 디버그 모드 비활성화 처리
            cb_DebugMode.Enabled = !isRunning;
            
            logManager.LogEvent($"Status updated to: {status}");
            
            if (isDebugMode)
            {
                logManager.LogDebug($"Status updated to: {status}. Running state: {isRunning}");
            }
            
            btn_Run.Enabled = !isRunning;   // 'Run' 버튼: Stopped 상태에서 활성화
            btn_Stop.Enabled = isRunning;   // 'Stop' 버튼: Running 상태에서 활성화
            btn_Quit.Enabled = !isRunning;   // 'Quit' 버튼: Stopped 상태에서 활성화
            
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
        
            if (isDebugMode)
            {
                logManager.LogDebug("Run operation started. Initializing components.");
            }
        
            try
            {
                // Watcher 시작
                fileWatcherManager.StartWatching();
        
                if (isDebugMode)
                {
                    logManager.LogDebug("File watchers started successfully.");
                }
        
                // 상태 업데이트
                UpdateMainStatus("Running...", Color.Blue);
            }
            catch (Exception ex)
            {
                logManager.LogEvent($"Error starting monitoring: {ex.Message}");
                if (isDebugMode)
                {
                    logManager.LogDebug($"Run operation failed with error: {ex.Message}");
                }
                UpdateMainStatus("Stopped!", Color.Red);
            }
        }
        
        private void btn_Stop_Click(object sender, EventArgs e)
        {
            logManager.LogEvent("Stop button clicked.");
        
            if (isDebugMode)
            {
                logManager.LogDebug("Stop operation started. Shutting down components.");
            }
        
            // Watcher 중지
            fileWatcherManager.StopWatchers();
        
            if (isDebugMode)
            {
                logManager.LogDebug("File watchers stopped successfully.");
            }
        
            // 상태 업데이트
            UpdateMainStatus("Stopped!", Color.Red);
        }

        
        private void UpdateButtonsState()
        {
            btn_Run.Enabled = !isRunning;
            btn_Stop.Enabled = isRunning;
            btn_Quit.Enabled = !isRunning;
        
            UpdateTrayMenuStatus(); // Tray 아이콘 상태 업데이트
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
        
        private void MainForm_Load(object sender, EventArgs e)
        {
            // 폼 로드시 실행할 로직
            pMain.Controls.Add(ucSc1);
            UpdateMenusBasedOnType();   // 메뉴 상태 업데이트
            
            // 초기 패널 설정 및 상태 동기화
            ShowUserControl(ucConfigPanel); // 초기 패널 로드
            ucConfigPanel.UpdateStatusOnRun(isRunning); // 상태 동기화
        }
        
        private void RefreshUI()
        {
            // Eqpid 상태 갱신
            string eqpid = settingsManager.GetEqpid();
            lb_eqpid.Text = $"Eqpid: {eqpid}";
        
            // TargetFolders, Regex 리스트 갱신
            ucSc1.RefreshUI(); // UserControl의 UI 갱신 호출
            ucConfigPanel?.RefreshUI();
            ucOverrideNamesPanel?.RefreshUI();
        
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
        
        private void InitializeUserControls()
        {
            // UserControl 초기화
            ucConfigPanel = new ucConfigurationPanel(settingsManager);
            ucOverrideNamesPanel = new ucOverrideNamesPanel(settingsManager);
            ucImageTransPanel = new ucScreen3();     // ucScreen3.cs 구현
            ucUploadDataPanel = new ucScreen4();     // ucScreen4.cs 공유
            
            ucConfigPanel.InitializePanel(isRunning); // 초기화 시 상태 동기화
            ucOverrideNamesPanel.InitializePanel(isRunning); // 초기화 시 상태 동기화
        }

        private void RegisterMenuEvents()
        {
            // Common -> Categorize
            tsm_Categorize.Click += (s, e) => ShowUserControl(ucConfigPanel);

            // ONTO -> Override Names
            tsm_OverrideNames.Click += (s, e) => ShowUserControl(ucOverrideNamesPanel);

            // ONTO -> Image Trans
            tsm_ImageTrans.Click += (s, e) => ShowUserControl(ucImageTransPanel);

            // ONTO -> Upload Data
            tsm_UploadData.Click += (s, e) => ShowUserControl(ucUploadDataPanel);
        }

        private void ShowUserControl(UserControl control)
        {
            pMain.Controls.Clear();
            pMain.Controls.Add(control);
            control.Dock = DockStyle.Fill;
            
            // 상태 동기화
            if (control is ucConfigurationPanel configPanel)
            {
                configPanel.UpdateStatusOnRun(isRunning);
            }
            else if (control is ucOverrideNamesPanel overridePanel)
            {
                overridePanel.UpdateStatusOnRun(isRunning);
            }
        }
        
        // MainForm.cs
        private void UpdateMenusBasedOnType()
        {
            string type = settingsManager.GetType();
            if (type == "ONTO")
            {
                tsm_Nova.Visible = false;
                tsm_Onto.Visible = true;
            }
            else if (type == "NOVA")
            {
                tsm_Onto.Visible = false;
                tsm_Nova.Visible = true;
            }
            else
            {
                tsm_Onto.Visible = false;
                tsm_Nova.Visible = false;
                return;
            }
            
            // Type 값에 따라 메뉴 표시/숨김 처리
            tsm_Onto.Visible = type.Equals("ONTO", StringComparison.OrdinalIgnoreCase);
            tsm_Nova.Visible = type.Equals("NOVA", StringComparison.OrdinalIgnoreCase);
        }
        
        private void InitializeMainMenu()
        {
            // 기존 메뉴 초기화 코드...
            UpdateMenusBasedOnType();
        }
        // 기본 생성자 추가
        public MainForm() 
            : this(new SettingsManager(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.ini")))
        {
            // 추가 동작 없음
        }
        
        // Debug Mode 체크박스 변경 시 처리
        private void cb_DebugMode_CheckedChanged(object sender, EventArgs e)
        {
            isDebugMode = cb_DebugMode.Checked;
            if (isDebugMode)
            {
                logManager.LogDebug("Debug mode enabled.");
            }
            else
            {
                logManager.LogDebug("Debug mode disabled.");
            }
        }
    }
}
