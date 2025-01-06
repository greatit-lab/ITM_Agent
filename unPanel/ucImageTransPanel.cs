using ITM_Agent.Services;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ITM_Agent.ucPanel
{
    public partial class ucImageTransPanel : UserControl
    {
        private readonly LogManager logManager; // LogManager 인스턴스 선언
        private readonly PdfMergeManager pdfMergeManager;
        private readonly SettingsManager settingsManager;
        private readonly ucConfigurationPanel configPanel;

        public ucImageTransPanel(SettingsManager settingsManager, ucConfigurationPanel configPanel)
        {
            this.settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            this.configPanel = configPanel ?? throw new ArgumentNullException(nameof(configPanel));
            InitializeComponent();

            logManager = new LogManager(AppDomain.CurrentDomain.BaseDirectory); // LogManager 초기화
            pdfMergeManager = new PdfMergeManager(AppDomain.CurrentDomain.BaseDirectory, logManager);

            logManager.LogEvent("[ucImageTransPanel] Initialized");

            // 이벤트 연결
            btn_SetFolder.Click += btn_SetFolder_Click;
            btn_FolderClear.Click += btn_FolderClear_Click;
            btn_SetTime.Click += btn_SetTime_Click;
            btn_TimeClear.Click += btn_TimeClear_Click;
            btn_SelectOutputFolder.Click += btn_SelectOutputFolder_Click;

            // UI 초기화
            LoadFolders();
            LoadRegexFolderPaths();
            LoadWaitTimes();
            LoadOutputFolder();
        }

        private void btn_SelectOutputFolder_Click(object sender, EventArgs e)
        {
            logManager.LogEvent("[ucImageTransPanel] Select output folder initiated");

            string baseFolder = configPanel.BaseFolderPath;

            if (string.IsNullOrEmpty(baseFolder) || !Directory.Exists(baseFolder))
            {
                logManager.LogError("[ucImageTransPanel] Base folder not set or invalid");
                MessageBox.Show("기준 폴더(Base Folder)가 설정되지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.SelectedPath = baseFolder;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFolder = folderDialog.SelectedPath;
                    lb_ImageSaveFolder.Text = selectedFolder;
                    settingsManager.SetValueToSection("ImageTrans", "SaveFolder", selectedFolder);

                    logManager.LogEvent($"[ucImageTransPanel] Output folder set: {selectedFolder}");
                    MessageBox.Show("출력 폴더가 설정되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btn_SetFolder_Click(object sender, EventArgs e)
        {
            logManager.LogEvent("[ucImageTransPanel] Set target folder initiated");

            if (cb_TargetImageFolder.SelectedItem is string selectedFolder)
            {
                settingsManager.SetValueToSection("ImageTrans", "Target", selectedFolder);
                logManager.LogEvent($"[ucImageTransPanel] Target folder set: {selectedFolder}");
                MessageBox.Show($"폴더가 설정되었습니다: {selectedFolder}", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                logManager.LogError("[ucImageTransPanel] No target folder selected");
                MessageBox.Show("폴더를 선택하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btn_FolderClear_Click(object sender, EventArgs e)
        {
            logManager.LogEvent("[ucImageTransPanel] Clearing target folder");

            if (cb_TargetImageFolder.SelectedItem != null)
            {
                cb_TargetImageFolder.SelectedIndex = -1;
                settingsManager.RemoveSection("ImageTrans");

                logManager.LogEvent("[ucImageTransPanel] Target folder cleared");
                MessageBox.Show("폴더 설정이 초기화되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                logManager.LogError("[ucImageTransPanel] No target folder selected to clear");
                MessageBox.Show("선택된 폴더가 없습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void LoadRegexFolderPaths()
        {
            cb_TargetImageFolder.Items.Clear();

            var regexFolders = configPanel.GetRegexList();
            cb_TargetImageFolder.Items.AddRange(regexFolders.ToArray());

            string selectedPath = settingsManager.GetValueFromSection("ImageTrans", "Target");
            if (!string.IsNullOrEmpty(selectedPath) && cb_TargetImageFolder.Items.Contains(selectedPath))
            {
                cb_TargetImageFolder.SelectedItem = selectedPath;
            }
            else
            {
                cb_TargetImageFolder.SelectedIndex = -1;
            }

            logManager.LogEvent("[ucImageTransPanel] Regex folder paths loaded");
        }

        private void LoadFolders()
        {
            cb_TargetImageFolder.Items.Clear();
            var folders = settingsManager.GetFoldersFromSection("[TargetFolders]");
            cb_TargetImageFolder.Items.AddRange(folders.ToArray());

            logManager.LogEvent("[ucImageTransPanel] Target folders loaded");
        }

        public void LoadWaitTimes()
        {
            cb_WaitTime.Items.Clear();
            cb_WaitTime.Items.AddRange(new object[] { "30", "60", "120", "180", "240", "300" });
            cb_WaitTime.SelectedIndex = -1;

            string savedWaitTime = settingsManager.GetValueFromSection("ImageTrans", "Wait");
            if (!string.IsNullOrEmpty(savedWaitTime) && cb_WaitTime.Items.Contains(savedWaitTime))
            {
                cb_WaitTime.SelectedItem = savedWaitTime;
            }

            logManager.LogEvent("[ucImageTransPanel] Wait times loaded");
        }

        private void btn_SetTime_Click(object sender, EventArgs e)
        {
            logManager.LogEvent("[ucImageTransPanel] Setting wait time");

            if (cb_WaitTime.SelectedItem is string selectedWaitTime && int.TryParse(selectedWaitTime, out int waitTime))
            {
                settingsManager.SetValueToSection("ImageTrans", "Wait", selectedWaitTime);
                logManager.LogEvent($"[ucImageTransPanel] Wait time set: {waitTime} seconds");
                MessageBox.Show($"대기 시간이 {waitTime}초로 설정되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                logManager.LogError("[ucImageTransPanel] Invalid wait time selected");
                MessageBox.Show("대기 시간을 선택하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btn_TimeClear_Click(object sender, EventArgs e)
        {
            logManager.LogEvent("[ucImageTransPanel] Clearing wait time");

            if (cb_WaitTime.SelectedItem != null)
            {
                cb_WaitTime.SelectedIndex = -1;
                settingsManager.SetValueToSection("ImageTrans", "Wait", string.Empty);

                logManager.LogEvent("[ucImageTransPanel] Wait time cleared");
                MessageBox.Show("대기 시간이 초기화되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                logManager.LogError("[ucImageTransPanel] No wait time selected to clear");
                MessageBox.Show("선택된 대기 시간이 없습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadOutputFolder()
        {
            string outputFolder = settingsManager.GetValueFromSection("ImageTrans", "SaveFolder");

            if (!string.IsNullOrEmpty(outputFolder) && Directory.Exists(outputFolder))
            {
                lb_ImageSaveFolder.Text = outputFolder;
            }
            else
            {
                lb_ImageSaveFolder.Text = "Output folder not set or does not exist.";
            }

            logManager.LogEvent("[ucImageTransPanel] Output folder loaded");
        }

        public void RefreshUI()
        {
            LoadRegexFolderPaths();
            LoadWaitTimes();
            logManager.LogEvent("[ucImageTransPanel] UI refreshed");
        }

        public void UpdateStatusOnRun(bool isRunning)
        {
            btn_SetFolder.Enabled = !isRunning;
            btn_FolderClear.Enabled = !isRunning;
            btn_SetTime.Enabled = !isRunning;
            btn_TimeClear.Enabled = !isRunning;
            btn_SelectOutputFolder.Enabled = !isRunning;
            cb_TargetImageFolder.Enabled = !isRunning;
            cb_WaitTime.Enabled = !isRunning;

            logManager.LogEvent($"[ucImageTransPanel] Status updated to {(isRunning ? "Running" : "Stopped")}");
        }
    }
}
