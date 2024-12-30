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
        private FileSystemWatcher baselineWatcher;
        
        private readonly LogManager logManager;
        
        public ucOverrideNamesPanel(SettingsManager settingsManager, ucConfigurationPanel configPanel)
        {
            this.settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            this.configPanel = configPanel ?? throw new ArgumentNullException(nameof(configPanel));

            InitializeComponent();
            
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            logManager = new LogManager(baseDir);
            
            InitializeBaselineWatcher();

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
                // 파일 준비 상태 확인
                if (!IsFileReady(e.FullPath))
                {
                    logManager.LogError($"파일이 사용 중이어서 액세스할 수 없습니다: {e.FullPath}");
                    return;
                }
        
                if (File.Exists(e.FullPath))
                {
                    DateTime? dateTimeInfo = ExtractDateTimeFromFile(e.FullPath);
                    if (dateTimeInfo.HasValue)
                    {
                        CreateBaselineInfoFile(e.FullPath, dateTimeInfo.Value);
                        logManager.LogEvent($"Baseline 파일 생성 성공: {e.FullPath}");
                    }
                }
            }
            catch (IOException ioEx)
            {
                logManager.LogError($"파일 처리 중 IO 오류 발생: {ioEx.Message}\n파일: {e.FullPath}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                logManager.LogError($"파일 접근 권한 오류: {uaEx.Message}\n파일: {e.FullPath}");
            }
            catch (Exception ex)
            {
                logManager.LogError($"예기치 않은 오류 발생: {ex.Message}\n파일: {e.FullPath}");
            }
        }
        
        private void CreateBaselineInfoFile(string filePath, DateTime dateTime)
        {
            string baseFolder = configPanel.BaseFolderPath; // ucConfigurationPanel에서 기준 폴더 경로 가져오기
            if (string.IsNullOrEmpty(baseFolder) || !Directory.Exists(baseFolder))
            {
                logManager.LogError("기준 폴더가 설정되지 않았습니다.");
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
                    logManager.LogEvent($"Baseline 파일 생성 성공: {newFilePath}");
                }
            }
            catch (Exception ex)
            {
                logManager.LogError($"파일 처리 중 오류가 발생했습니다: {ex.Message}\n파일: {filePath}");
            }
        }

        private bool IsFileReady(string filePath)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    return true; // 파일에 액세스 가능
                }
            }
            catch (IOException)
            {
                return false; // 파일이 잠겨 있음
            }
        }
        
        private bool WaitForFileReady(string filePath, int maxRetries = 10, int delayMilliseconds = 500)
        {
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                if (IsFileReady(filePath))
                {
                    return true; // 파일에 접근 가능
                }
        
                // 파일 잠금 해제를 기다림
                Thread.Sleep(delayMilliseconds);
            }
        
            logManager.LogError($"파일이 잠겨 있어 접근할 수 없습니다: {filePath}");
            return false; // 재시도 초과
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
                        logManager.LogError($"파일 읽기 중 오류 발생: {ex.Message}\n파일: {filePath}");
                        return null;
                    }
                }
                else
                {
                    Thread.Sleep(delayMs); // 파일이 잠겨있으면 대기
                }
            }

            logManager.LogError($"파일이 사용 중이어서 처리할 수 없습니다.\n파일: {filePath}");
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

        public void CompareAndRenameFiles()
        {
            try
            {
                string baselineFolder = Path.Combine(settingsManager.GetBaseFolder(), "Baseline");
                if (!Directory.Exists(baselineFolder))
                {
                    logManager.LogError("Baseline 폴더가 존재하지 않습니다.");
                    return;
                }
        
                // Baseline 파일 데이터 추출
                var baselineFiles = Directory.GetFiles(baselineFolder, "*.info");
                var baselineData = ExtractBaselineData(baselineFiles);
        
                if (baselineData.Count == 0)
                {
                    logManager.LogEvent("Baseline 폴더에 유효한 .info 파일이 없습니다.");
                    return;
                }
        
                // Target Compare Path에서 파일 처리
                foreach (string targetFolder in lb_TargetComparePath.Items)
                {
                    if (!Directory.Exists(targetFolder)) continue;
        
                    var targetFiles = Directory.GetFiles(targetFolder);
                    foreach (var targetFile in targetFiles)
                    {
                        try
                        {
                            string newFileName = ProcessTargetFile(targetFile, baselineData);
                            if (!string.IsNullOrEmpty(newFileName))
                            {
                                string newFilePath = Path.Combine(targetFolder, newFileName);
                                File.Move(targetFile, newFilePath);
        
                                // 성공 로그 기록
                                logManager.LogEvent($"파일 이름 변경 성공: {targetFile} -> {newFilePath}");
                            }
                        }
                        catch (IOException ioEx)
                        {
                            // 파일 작업 중 오류를 이벤트 로그에 기록
                            logManager.LogError($"파일 작업 중 오류 발생: {ioEx.Message}\n파일: {targetFile}");
        
                            // 작업 중단 없이 진행 상황 기록
                            logManager.LogEvent($"작업 진행: {targetFile} 처리 중 오류 발생, 다음 파일로 진행.");
                        }
                        catch (UnauthorizedAccessException uaEx)
                        {
                            logManager.LogError($"파일 접근 권한 오류: {uaEx.Message}\n파일: {targetFile}");
                            logManager.LogEvent($"작업 진행: {targetFile} 처리 중 권한 오류 발생, 다음 파일로 진행.");
                        }
                        catch (Exception ex)
                        {
                            logManager.LogError($"예기치 않은 오류 발생: {ex.Message}\n파일: {targetFile}");
                            logManager.LogEvent($"작업 진행: {targetFile} 처리 중 예기치 않은 오류 발생, 다음 파일로 진행.");
                        }
                    }
                }
        
                logManager.LogEvent("파일 이름 변환 작업이 완료되었습니다.");
            }
            catch (Exception ex)
            {
                // 최상위 예외 처리 (예외를 놓치지 않도록 최종 방어선)
                logManager.LogError($"전체 작업 중 예기치 않은 오류 발생: {ex.Message}");
            }
        }

        
        public void StartProcessing()
        {
            while (true) // 상시 가동 상태
            {
                if (IsRunning())
                {
                    CompareAndRenameFiles();
                    System.Threading.Thread.Sleep(1000); // 작업 주기 조정
                }
            }
        }

        private bool IsRunning()
        {
            // Running 상태 확인 로직 구현
            return true; // 임시로 항상 true 반환
        }

        private void InitializeBaselineWatcher()
        {
            var baseFolder = settingsManager.GetFoldersFromSection("[BaseFolder]").FirstOrDefault();
            if (string.IsNullOrEmpty(baseFolder) || !Directory.Exists(baseFolder))
            {
                MessageBox.Show("BaseFolder를 선택하거나 유효한 경로를 설정하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var baselineFolder = Path.Combine(baseFolder, "Baseline");
            if (!Directory.Exists(baselineFolder))
            {
                MessageBox.Show("Baseline 폴더가 존재하지 않습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            baselineWatcher = new FileSystemWatcher(baselineFolder, "*.info")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            baselineWatcher.Created += OnBaselineFileChanged;
            baselineWatcher.Changed += OnBaselineFileChanged;
            baselineWatcher.EnableRaisingEvents = true;
        }

        private void OnBaselineFileChanged(object sender, FileSystemEventArgs e)
        {
            if (File.Exists(e.FullPath))
            {
                var baselineData = ExtractBaselineData(new[] { e.FullPath });

                foreach (string targetFolder in lb_TargetComparePath.Items)
                {
                    if (!Directory.Exists(targetFolder)) continue;

                    var targetFiles = Directory.GetFiles(targetFolder);
                    foreach (var targetFile in targetFiles)
                    {
                        string newFileName = ProcessTargetFile(targetFile, baselineData);
                        if (!string.IsNullOrEmpty(newFileName))
                        {
                            string newFilePath = Path.Combine(targetFolder, newFileName);

                            try
                            {
                                // 파일 경로 존재 여부 확인
                                if (!File.Exists(targetFile))
                                {
                                    if (settingsManager.IsDebugMode)
                                    {
                                        Console.WriteLine($"[DEBUG] 원본 파일을 찾을 수 없습니다: {targetFile}");
                                    }
                                    continue;
                                }

                                File.Move(targetFile, newFilePath);

                                // 변경 내용 로그 기록
                                LogFileRename(targetFile, newFilePath);
                            }
                            catch (IOException ioEx)
                            {
                                Console.WriteLine($"파일 이동 중 오류 발생: {ioEx.Message}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"예기치 않은 오류가 발생했습니다: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }

        private void LogFileRename(string oldPath, string newPath)
        {
            string logMessage = $"[INFO] 파일 이름 변경: {oldPath} -> {newPath}";
            Console.WriteLine(logMessage);

            if (settingsManager.IsDebugMode) // Debug Mode 상태 확인
            {
                Console.WriteLine($"[DEBUG] 파일 변경 상세 로그 기록 완료: {logMessage}");
            }
        }


        private Dictionary<string, (string TimeInfo, string Prefix, string CInfo)> ExtractBaselineData(string[] files)
        {
            var baselineData = new Dictionary<string, (string, string, string)>();
            var regex = new Regex(@"(\d{8}_\d{6})_([^_]+?)_(C\dW\d+)");

            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                var match = regex.Match(fileName);
                if (match.Success)
                {
                    string timeInfo = match.Groups[1].Value;
                    string prefix = match.Groups[2].Value;
                    string cInfo = match.Groups[3].Value;

                    baselineData[fileName] = (timeInfo, prefix, cInfo);
                }
            }

            return baselineData;
        }

        private string ProcessTargetFile(string targetFile, Dictionary<string, (string TimeInfo, string Prefix, string CInfo)> baselineData)
        {
            // 파일이 준비될 때까지 대기
            if (!WaitForFileReady(targetFile))
            {
                logManager.LogError($"파일을 처리할 수 없습니다. 잠긴 상태로 남아 있습니다: {targetFile}");
                return null;
            }
        
            string fileName = Path.GetFileName(targetFile);
            foreach (var data in baselineData.Values)
            {
                if (fileName.Contains(data.TimeInfo) && fileName.Contains(data.Prefix))
                {
                    var regex = new Regex(@"_#1_");
                    return regex.Replace(fileName, $"_{data.CInfo}_");
                }
            }
        
            return null;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                baselineWatcher?.Dispose();
            }
            base.Dispose(disposing);
        }
        
        private bool IsFileAccessible(string filePath)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    return true; // 파일 접근 가능
                }
            }
            catch (IOException)
            {
                return false; // 파일이 잠겨 있음
            }
        }
    }
}
