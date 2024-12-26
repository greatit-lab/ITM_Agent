using ITM_Agent.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace ITM_Agent.ucPanel
{
    public partial class ucOverrideNamesPanel : UserControl
    {
        private readonly SettingsManager settingsManager;
        private FileSystemWatcher folderWatcher; // 폴더 감시기
        private List<string> regexFolders; // 정규표현식과 폴더 정보 저장
        private string baseFolder; // BaseFolder 저장

        public event Action<string, Color> StatusUpdated;

        private ucConfigurationPanel configPanel;

        public ucOverrideNamesPanel(SettingsManager settingsManager, ucConfigurationPanel configPanel)
        {
            this.settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            this.configPanel = configPanel ?? throw new ArgumentNullException(nameof(configPanel));

            InitializeComponent();

            InitializeCustomEvents();
            //PopulateComboBox();

            // 데이터 로드
            LoadDataFromSettings();
            LoadRegexFolderPaths(); // 초기화 시 목록 로드
            LoadSelectedBaseDatePath(); // 저장된 선택 값 불러오기
        }

        private void InitializeCustomEvents()
        {
            cb_BaseDatePath.SelectedIndexChanged += cb_BaseDatePath_SelectedIndexChanged;
            btn_BaseClear.Click += btn_BaseClear_Click;
            btn_SelectFolder.Click += Btn_SelectFolder_Click;
            btn_Remove.Click += Btn_Remove_Click;
        }

        private void LoadRegexFolderPaths()
        {
            cb_BaseDatePath.Items.Clear();
            var regexList = settingsManager.GetRegexList();
            var folderPaths = regexList.Values.ToList();
            cb_BaseDatePath.Items.AddRange(folderPaths.ToArray());
            cb_BaseDatePath.SelectedIndex = -1; // 초기화
        }

        private void LoadSelectedBaseDatePath()
        {
            string selectedPath = settingsManager.GetValueFromSection("SelectedBaseDatePath", "Path");
            if (!string.IsNullOrEmpty(selectedPath) && cb_BaseDatePath.Items.Contains(selectedPath))
            {
                cb_BaseDatePath.SelectedItem = selectedPath;
                StartFolderWatcher(selectedPath);
            }
        }

        private void cb_BaseDatePath_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_BaseDatePath.SelectedItem is string selectedPath)
            {
                settingsManager.SetValueToSection("SelectedBaseDatePath", "Path", selectedPath);
                StartFolderWatcher(selectedPath);
            }
        }

        private void StartFolderWatcher(string path)
        {
            // 기존 감시 중지
            folderWatcher?.Dispose();

            if (Directory.Exists(path))
            {
                folderWatcher = new FileSystemWatcher
                {
                    Path = path,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                    Filter = "*.*",
                    EnableRaisingEvents = true
                };

                folderWatcher.Created += OnFileChanged;
                folderWatcher.Changed += OnFileChanged;
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (File.Exists(e.FullPath))
                {
                    DateTime? dateTimeInfo = ExtractDateTimeFromFile(e.FullPath);
                    if (dateTimeInfo.HasValue)
                    {
                        CreateBaselineInfoFile(e.FullPath, dateTimeInfo.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일 처리 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateBaselineInfoFile(string filePath, DateTime dateTime)
        {
            string baseFolder = configPanel.BaseFolderPath; // ucConfigurationPanel에서 기준 폴더 경로 가져오기
            if (string.IsNullOrEmpty(baseFolder) || !Directory.Exists(baseFolder))
            {
                MessageBox.Show("기준 폴더가 설정되지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string baselineFolder = Path.Combine(baseFolder, "Baseline");
            if (!Directory.Exists(baselineFolder))
            {
                Directory.CreateDirectory(baselineFolder);
            }

            string originalFileName = Path.GetFileNameWithoutExtension(filePath);
            string newFileName = $"{dateTime:yyyyMMdd_HHmmss}_{originalFileName}.info";
            string newFilePath = Path.Combine(baselineFolder, newFileName);

            try
            {
                // 파일을 읽기 후 닫기
                using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // 파일을 단순히 읽고 아무 작업 없이 닫음
                }

                // 빈 파일 생성
                using (File.Create(newFilePath))
                {
                    // 파일 생성 성공
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일 처리 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsFileReady(string filePath)
        {
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return true;
                }
            }
            catch (IOException)
            {
                return false;
            }
        }

        private DateTime? ExtractDateTimeFromFile(string filePath)
        {
            string datePattern = @"Date and Time:\s*(\d{2}/\d{2}/\d{4} \d{2}:\d{2}:\d{2} (AM|PM))";
            const int maxRetries = 5;
            const int delayMs = 1000;

            for (int i = 0; i < maxRetries; i++)
            {
                if (IsFileReady(filePath))
                {
                    try
                    {
                        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var reader = new StreamReader(fileStream))
                        {
                            string fileContent = reader.ReadToEnd();
                            Match match = Regex.Match(fileContent, datePattern);
                            if (match.Success && DateTime.TryParse(match.Groups[1].Value, out DateTime result))
                            {
                                return result;
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show($"파일 읽기 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                }
                else
                {
                    Thread.Sleep(delayMs); // 파일이 잠겨있으면 대기
                }
            }

            MessageBox.Show("파일이 사용 중이어서 처리할 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }


        private void RenameFileWithDate(string filePath, DateTime dateTime)
        {
            string directory = Path.GetDirectoryName(filePath);
            string originalFileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);

            string newFileName = $"{dateTime:yyyyMMdd_HHmmss}_{originalFileName}{extension}";
            string newFilePath = Path.Combine(directory, newFileName);

            File.Move(filePath, newFilePath);
        }

        private void btn_BaseClear_Click(object sender, EventArgs e)
        {
            cb_BaseDatePath.SelectedIndex = -1;
            settingsManager.RemoveSection("SelectedBaseDatePath"); // 저장된 값 삭제
            folderWatcher?.Dispose();
        }

        private void Btn_SelectFolder_Click(object sender, EventArgs e)
        {
            var baseFolder = settingsManager.GetFoldersFromSection("[BaseFolder]").FirstOrDefault() ?? AppDomain.CurrentDomain.BaseDirectory;

            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.SelectedPath = baseFolder;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    if (!lb_TargetComparePath.Items.Contains(folderDialog.SelectedPath))
                    {
                        lb_TargetComparePath.Items.Add(folderDialog.SelectedPath);
                        UpdateTargetComparePathInSettings();
                    }
                    else
                    {
                        MessageBox.Show("해당 폴더는 이미 추가되어 있습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void Btn_Remove_Click(object sender, EventArgs e)
        {
            if (lb_TargetComparePath.SelectedItems.Count > 0)
            {
                var confirmResult = MessageBox.Show("선택한 항목을 삭제하시겠습니까?", "삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirmResult == DialogResult.Yes)
                {
                    var selectedItems = lb_TargetComparePath.SelectedItems.Cast<string>().ToList();
                    foreach (var item in selectedItems)
                    {
                        lb_TargetComparePath.Items.Remove(item);
                    }

                    UpdateTargetComparePathInSettings();
                }
            }
            else
            {
                MessageBox.Show("삭제할 항목을 선택하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateTargetComparePathInSettings()
        {
            var folders = lb_TargetComparePath.Items.Cast<string>().ToList();
            settingsManager.SetFoldersToSection("[TargetComparePath]", folders);
        }

        public void UpdateStatusOnRun(bool isRunning)
        {
            string status = isRunning ? "Running" : "Stopped";
            Color statusColor = isRunning ? Color.Green : Color.Red;

            StatusUpdated?.Invoke($"Status: {status}", statusColor);
        }

        public void InitializePanel(bool isRunning)
        {
            UpdateStatusOnRun(isRunning);
        }

        public void LoadDataFromSettings()
        {
            var baseFolders = settingsManager.GetFoldersFromSection("[BaseFolder]");
            cb_BaseDatePath.Items.Clear();
            cb_BaseDatePath.Items.AddRange(baseFolders.ToArray());

            var comparePaths = settingsManager.GetFoldersFromSection("[TargetComparePath]");
            lb_TargetComparePath.Items.Clear();
            foreach (var path in comparePaths)
            {
                lb_TargetComparePath.Items.Add(path);
            }
        }

        public void RefreshUI()
        {
            LoadDataFromSettings();
        }

        public void SetControlEnabled(bool isEnabled)
        {
            btn_BaseClear.Enabled = isEnabled;
            btn_SelectFolder.Enabled = isEnabled;
            btn_Remove.Enabled = isEnabled;
            cb_BaseDatePath.Enabled = isEnabled;
            lb_TargetComparePath.Enabled = isEnabled;
        }

        public void UpdateStatus(string status)
        {
            bool isRunning = status == "Running...";
            SetControlEnabled(!isRunning);
        }
    }
}
