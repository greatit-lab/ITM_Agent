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
        private ucImageTransPanel ucImageTransPanel;
        //private ucScreen4 ucUploadDataPanel;

        private bool isRunning = false; // 현재 상태 플래그
        private bool isDebugMode = false;   // 디버그 모드 상태
        
        // MainForm.cs 상단 (다른 user control 변수들과 함께)
        private ucUploadPanel ucUploadPanel;
        private ucPluginPanel ucPluginPanel;
        
        
        public MainForm(SettingsManager settingsManager)
        {
           this.settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
           this.settingsManager = settingsManager;
        
           InitializeComponent();
        
           // 폼 핸들이 생성된 직후 상태 표시(Stopped, 빨간색)
           this.HandleCreated += (sender, e) => UpdateMainStatus("Stopped", Color.Red);
        
           string baseDir = AppDomain.CurrentDomain.BaseDirectory;
           logManager = new LogManager(baseDir);
        
           InitializeUserControls();
           RegisterMenuEvents();
        
           // 설정 패널
           ucSc1 = new ucPanel.ucConfigurationPanel(settingsManager);
           // Override Names 패널 (Designer에서 배치된 ucConfigPanel 컨트롤 인스턴스 전달)
           ucOverrideNamesPanel = new ucOverrideNamesPanel(settingsManager,this.ucConfigPanel,this.logManager,this.settingsManager.IsDebugMode);
        
           // FileWatcherManager 생성 (SettingsManager, LogManager, 디버그 모드 플래그)
           fileWatcherManager = new FileWatcherManager(settingsManager, logManager, isDebugMode);
           eqpidManager = new EqpidManager(settingsManager, logManager, AppVersion);
        
           // 아이콘 설정
           SetFormIcon();
        
           this.Text = $"ITM Agent - {AppVersion}";
           this.MaximizeBox = false;
        
           InitializeTrayIcon();
           this.FormClosing += MainForm_FormClosing;
        
           // EQPID 초기화 및 메인 기능 진행
           eqpidManager.InitializeEqpid();
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

            // --- UserControl 상태 동기화 ---
            ucOverrideNamesPanel?.UpdateStatus(status);
            bool isRunning = (status == "Running...");
            ucConfigPanel?.UpdateStatusOnRun(isRunning);
            ucOverrideNamesPanel?.UpdateStatusOnRun(isRunning);
            ucImageTransPanel?.UpdateStatusOnRun(isRunning);

            // 디버그 모드
            cb_DebugMode.Enabled = !isRunning;

            logManager.LogEvent($"Status updated to: {status}");
            if (isDebugMode)
                logManager.LogDebug($"Status updated to: {status}. Running state: {isRunning}");

            // --- 여기부터 상태 문자열별로 버튼 활성화 설정 ---
            if (status == "Stopped!")
            {
                // Stopped 상태: Run/Stop 비활성, Quit 활성
                btn_Run.Enabled = false;
                btn_Stop.Enabled = false;
                btn_Quit.Enabled = true;
            }
            else if (status == "Ready to Run")
            {
                // Ready 상태: Run, Quit 활성 / Stop 비활성
                btn_Run.Enabled = true;
                btn_Stop.Enabled = false;
                btn_Quit.Enabled = true;
            }
            else if (status == "Running...")
            {
                // Running 상태: Stop 활성 / Run, Quit 비활성
                btn_Run.Enabled = false;
                btn_Stop.Enabled = true;
                btn_Quit.Enabled = false;
            }
            else
            {
                // 혹시 다른 문자열 상태가 들어왔을 때
                btn_Run.Enabled = false;
                btn_Stop.Enabled = false;
                btn_Quit.Enabled = false;
            }

            // ★ 기존에 아래처럼 한 줄로 처리하던 'btn_Run.Enabled = !isRunning;' 등은 지워주세요.
            //    btn_Run.Enabled = !isRunning;   
            //    btn_Stop.Enabled = isRunning;   
            //    btn_Quit.Enabled = !isRunning;

            UpdateTrayMenuStatus();
            UpdateMenuItemsState(isRunning);
            UpdateButtonsState();
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
                fileWatcherManager.StartWatching(); // FileWatcher 시작
                isRunning = true; // 상태 업데이트
                UpdateMainStatus("Running...", Color.Blue);

                if (isDebugMode)
                {
                    logManager.LogDebug("FileWatcherManager started successfully.");
                }
            }
            catch (Exception ex)
            {
                logManager.LogError($"Error starting monitoring: {ex.Message}");
                UpdateMainStatus("Stopped!", Color.Red);
            }
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            logManager.LogEvent("Stop button clicked.");

            fileWatcherManager.StopWatchers(); // 모니터 중지
            isRunning = false; // 현재 Running 상태 false

            // 이제 Ready 상태인지 확인
            bool isReady = ucConfigPanel.IsReadyToRun();
            if (isReady)
            {
                // 모든 조건이 충족되었다면 "Ready to Run"
                UpdateMainStatus("Ready to Run", Color.Green);
            }
            else
            {
                // 아니면 "Stopped!"
                UpdateMainStatus("Stopped!", Color.Red);
            }

            // ucConfigPanel, ucOverrideNamesPanel 등 동기화
            ucConfigPanel.InitializePanel(isRunning);
            ucOverrideNamesPanel.InitializePanel(isRunning);
        }

        private void UpdateButtonsState()
        {
            //btn_Run.Enabled = !isRunning;
            //btn_Stop.Enabled = isRunning;
            //btn_Quit.Enabled = !isRunning;

            UpdateTrayMenuStatus(); // Tray 아이콘 상태 업데이트
        }

        private void btn_Quit_Click(object sender, EventArgs e)
        {
            if (ts_Status.Text == "Running...")
            {
                // 실행 중인 작업 강제 중지
                btn_Stop.PerformClick(); // Stop 버튼 동작 호출
            }

            PerformQuit(); // 종료 실행
        }

        private void PerformQuit()
        {
            fileWatcherManager.StopWatchers();
            trayIcon?.Dispose();
            Environment.Exit(0);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // (1) 폼 로드시 실행할 로직
            pMain.Controls.Add(ucSc1);
            UpdateMenusBasedOnType();   // 메뉴 상태 업데이트

            // (2) 초기 패널 설정 및 UserControl 상태 동기화
            ShowUserControl(ucConfigPanel); // 가장 먼저 ucConfigPanel 보여줌

            // 만약 isRunning 플래그를 기본값으로 false로 두고 있다면,
            // 우선 "멈춤" 상태를 패널들에 반영
            ucConfigPanel.UpdateStatusOnRun(isRunning);
            ucImageTransPanel.UpdateStatusOnRun(isRunning);

            // (3) ucConfigurationPanel 에서 현재 Target/Folder/Regex 등이 모두 세팅되었는지 확인
            bool isReady = ucConfigPanel.IsReadyToRun();

            // (4) 준비되었으면 "Ready to Run" 상태로,
            //     아니면 "Stopped!" 상태로 업데이트
            if (isReady)
            {
                UpdateMainStatus("Ready to Run", Color.Green);
            }
            else
            {
                UpdateMainStatus("Stopped!", Color.Red);
            }
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
            // 1) 가장 먼저 ucConfigPanel 생성
            ucConfigPanel = new ucConfigurationPanel(settingsManager);
        
            // 2) 그 다음에 ucImageTransPanel 생성
            ucImageTransPanel = new ucImageTransPanel(settingsManager,ucConfigPanel);     // ← null이 아닌, 방금 생성된 인스턴스를 전달
        
            // 3) 나머지 패널들 생성
            ucPluginPanel = new ucPluginPanel(settingsManager);
            ucUploadPanel = new ucUploadPanel(ucConfigPanel,ucPluginPanel,settingsManager);
            ucOverrideNamesPanel = new ucOverrideNamesPanel(settingsManager,ucConfigPanel,logManager,settingsManager.IsDebugMode);
        
            ucConfigurationPanel cfgPanel = this.ucConfigPanel;
        
            // 이 라인에서 ucOverrideNamesPanel을 생성합니다.
        
            ucImageTransPanel = new ucImageTransPanel(settingsManager, ucConfigPanel);
        
        
            // UserControl 초기화
            ucConfigPanel = new ucConfigurationPanel(settingsManager);
        
            // ucPluginPanel을 ucUploadPanel보다 먼저 생성하여 NullReferenceException을 방지합니다.
            ucPluginPanel = new ucPluginPanel(settingsManager);
            ucUploadPanel = new ucUploadPanel(ucConfigPanel, ucPluginPanel, settingsManager);
            ucOverrideNamesPanel = new ucOverrideNamesPanel(settingsManager,cfgPanel,logManager,settingsManager.IsDebugMode);
        
            this.Controls.Add(ucOverrideNamesPanel);
        
            ucImageTransPanel = new ucImageTransPanel(settingsManager, ucConfigPanel);
            //ucUploadDataPanel = new ucScreen4();     // ucScreen4.cs 공유
        
            // 신규 플러그인, 업로드 패널 생성
            //ucUploadPanel = new ucUploadPanel(ucConfigPanel, ucPluginPanel, settingsManager);
            //ucPluginPanel = new ucPluginPanel(settingsManager);
        
            ucConfigPanel.InitializePanel(isRunning); // 초기화 시 상태 동기화
            ucOverrideNamesPanel.InitializePanel(isRunning);
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
            //tsm_UploadData.Click += (s, e) => ShowUserControl(ucUploadDataPanel);
            
            // 신규 메뉴 - Upload Data 메뉴 클릭 시 ucUploadPanel 표시
            tsm_UploadData.Click += (s, e) => ShowUserControl(ucUploadPanel);
            
            // 신규 메뉴 - Plugin 메뉴 클릭 시 ucPluginPanel 표시
            tsm_PluginList.Click += (s, e) => ShowUserControl(ucPluginPanel);
        }

        private void ShowUserControl(UserControl control)
        {
            pMain.Controls.Clear();
            pMain.Controls.Add(control);
            control.Dock = DockStyle.Fill;

            // 상태 동기화
            if (control is ucConfigurationPanel configPanel)
            {
                configPanel.InitializePanel(isRunning); // Running 상태 동기화
            }
            else if (control is ucOverrideNamesPanel overridePanel)
            {
                overridePanel.InitializePanel(isRunning); // Running 상태 동기화
            }
        }

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

        private void cb_DebugMode_CheckedChanged(object sender, EventArgs e)
        {
            isDebugMode = cb_DebugMode.Checked;
            settingsManager.IsDebugMode = isDebugMode; // SettingsManager에 상태 업데이트

            fileWatcherManager.UpdateDebugMode(isDebugMode);

            if (isDebugMode)
            {
                logManager.LogEvent("Debug Mode: Enabled");
                logManager.LogDebug("Debug mode enabled.");
            }
            else
            {
                logManager.LogEvent("Debug Mode: Disabled");
                logManager.LogDebug("Debug mode disabled.");
            }
        }
    }
}
