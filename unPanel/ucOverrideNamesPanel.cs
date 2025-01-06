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

            // Debug Log 예시
            logManager.LogDebug("[ucOverrideNamesPanel] 생성자 호출 - 초기화 시작");

            InitializeBaselineWatcher();
            InitializeCustomEvents();

            // 데이터 로드
            LoadDataFromSettings();
            LoadRegexFolderPaths(); // 초기화 시 목록 로드
            LoadSelectedBaseDatePath(); // 저장된 선택 값 불러오기

            // Debug Log 예시
            logManager.LogDebug("[ucOverrideNamesPanel] 생성자 호출 - 초기화 완료");
        }

        private void InitializeCustomEvents()
        {
            // Event Log 예시
            logManager.LogEvent("[ucOverrideNamesPanel] InitializeCustomEvents() 호출됨");

            cb_BaseDatePath.SelectedIndexChanged += cb_BaseDatePath_SelectedIndexChanged;
            btn_BaseClear.Click += btn_BaseClear_Click;
            btn_SelectFolder.Click += Btn_SelectFolder_Click;
            btn_Remove.Click += Btn_Remove_Click;
        }

        private void LoadRegexFolderPaths()
        {
            // Debug Log 예시
            logManager.LogDebug("[ucOverrideNamesPanel] LoadRegexFolderPaths() 시작");

            cb_BaseDatePath.Items.Clear();
            var regexList = settingsManager.GetRegexList();
            var folderPaths = regexList.Values.ToList();
            cb_BaseDatePath.Items.AddRange(folderPaths.ToArray());
            cb_BaseDatePath.SelectedIndex = -1; // 초기화

            // Event Log 예시
            logManager.LogEvent("[ucOverrideNamesPanel] 정규식 경로 목록 로드 완료");
        }

        private void LoadSelectedBaseDatePath()
        {
            // Debug Log
            logManager.LogDebug("[ucOverrideNamesPanel] LoadSelectedBaseDatePath() 시작");

            string selectedPath = settingsManager.GetValueFromSection("SelectedBaseDatePath", "Path");
            if (!string.IsNullOrEmpty(selectedPath) && cb_BaseDatePath.Items.Contains(selectedPath))
            {
                cb_BaseDatePath.SelectedItem = selectedPath;
                StartFolderWatcher(selectedPath);
            }

            // Event Log
            logManager.LogEvent("[ucOverrideNamesPanel] 저장된 BaseDatePath 로드 및 감시 시작");
        }

        private void cb_BaseDatePath_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_BaseDatePath.SelectedItem is string selectedPath)
            {
                settingsManager.SetValueToSection("SelectedBaseDatePath", "Path", selectedPath);
                StartFolderWatcher(selectedPath);

                // Debug Log
                logManager.LogDebug($"[ucOverrideNamesPanel] cb_BaseDatePath_SelectedIndexChanged -> {selectedPath} 설정");
            }
        }

        private void StartFolderWatcher(string path)
        {
            // 기존 감시 중지
            folderWatcher?.Dispose();

            // Event Log
            logManager.LogEvent($"[ucOverrideNamesPanel] StartFolderWatcher() 호출 - 감시 경로: {path}");

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
            else
            {
                // Error Log
                logManager.LogError($"[ucOverrideNamesPanel] 지정된 경로가 존재하지 않습니다: {path}");
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (!IsFileReady(e.FullPath))
                {
                    logManager.LogError($"[ucOverrideNamesPanel] 파일이 사용 중이어서 액세스할 수 없습니다: {e.FullPath}");
                    return;
                }
        
                if (File.Exists(e.FullPath))
                {
                    DateTime? dateTimeInfo = ExtractDateTimeFromFile(e.FullPath);
                    if (dateTimeInfo.HasValue)
                    {
                        // "Baseline 대상 파일 감지" 로그로 변경
                        logManager.LogEvent($"[ucOverrideNamesPanel] Baseline 대상 파일 감지: {e.FullPath}");
        
                        // 실제 .info 파일 생성
                        CreateBaselineInfoFile(e.FullPath, dateTimeInfo.Value);
                    }
                }
            }
            catch (IOException ioEx)
            {
                logManager.LogError($"[ucOverrideNamesPanel] 파일 처리 중 IO 오류 발생: {ioEx.Message}\n파일: {e.FullPath}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                logManager.LogError($"[ucOverrideNamesPanel] 파일 접근 권한 오류: {uaEx.Message}\n파일: {e.FullPath}");
            }
            catch (Exception ex)
            {
                logManager.LogError($"[ucOverrideNamesPanel] 예기치 않은 오류 발생: {ex.Message}\n파일: {e.FullPath}");
            }
        }
        
        private void CreateBaselineInfoFile(string filePath, DateTime dateTime)
        {
            // Debug Log
            logManager.LogDebug($"[ucOverrideNamesPanel] CreateBaselineInfoFile() 호출 - 대상 파일: {filePath}");
        
            string baseFolder = configPanel.BaseFolderPath; 
            if (string.IsNullOrEmpty(baseFolder) || !Directory.Exists(baseFolder))
            {
                logManager.LogError("[ucOverrideNamesPanel] 기준 폴더가 설정되지 않았거나 존재하지 않습니다.");
                return;
            }
        
            string baselineFolder = Path.Combine(baseFolder, "Baseline");
            if (!Directory.Exists(baselineFolder))
            {
                Directory.CreateDirectory(baselineFolder);
                logManager.LogEvent($"[ucOverrideNamesPanel] Baseline 폴더가 없어서 생성했습니다: {baselineFolder}");
            }
        
            string originalFileName = Path.GetFileNameWithoutExtension(filePath);
            string newFileName = $"{dateTime:yyyyMMdd_HHmmss}_{originalFileName}.info";
            string newFilePath = Path.Combine(baselineFolder, newFileName);
        
            try
            {
                // 파일 접근 테스트
                using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // 단순 읽기 후 닫기
                }
        
                // 빈 파일 생성
                using (File.Create(newFilePath))
                {
                    // **실제 .info 파일이 만들어진 순간**에만 "Baseline 파일 생성 성공" 로그 출력
                    logManager.LogEvent($"[ucOverrideNamesPanel] Baseline 파일 생성 성공: {newFilePath}");
                }
            }
            catch (Exception ex)
            {
                logManager.LogError($"[ucOverrideNamesPanel] 파일 처리 중 오류가 발생했습니다: {ex.Message}\n파일: {filePath}");
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
            // Debug Log
            logManager.LogDebug($"[ucOverrideNamesPanel] WaitForFileReady() 호출 - 파일 준비 대기: {filePath}");

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                if (IsFileReady(filePath))
                {
                    return true; // 파일에 접근 가능
                }
        
                // 파일 잠금 해제를 기다림
                Thread.Sleep(delayMilliseconds);
            }
        
            // Error Log
            logManager.LogError($"[ucOverrideNamesPanel] 파일이 잠겨 있어 접근할 수 없습니다: {filePath}");
            return false; // 재시도 초과
        }
        
        private DateTime? ExtractDateTimeFromFile(string filePath)
        {
            string datePattern = @"Date and Time:\s*(\d{2}/\d{2}/\d{4} \d{2}:\d{2}:\d{2} (AM|PM))";
            const int maxRetries = 5;
            const int delayMs = 1000;

            // Debug Log
            logManager.LogDebug($"[ucOverrideNamesPanel] ExtractDateTimeFromFile() - 파일: {filePath}");

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
                        logManager.LogError($"[ucOverrideNamesPanel] 파일 읽기 중 오류 발생: {ex.Message}\n파일: {filePath}");
                        return null;
                    }
                }
                else
                {
                    Thread.Sleep(delayMs); // 파일이 잠겨있으면 대기
                }
            }

            logManager.LogError($"[ucOverrideNamesPanel] 파일이 사용 중이어서 처리할 수 없습니다.\n파일: {filePath}");
            return null;
        }


        private void RenameFileWithDate(string filePath, DateTime dateTime)
        {
            // Debug Log
            logManager.LogDebug($"[ucOverrideNamesPanel] RenameFileWithDate() 호출 - 대상 파일: {filePath}");

            string directory = Path.GetDirectoryName(filePath);
            string originalFileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);

            string newFileName = $"{dateTime:yyyyMMdd_HHmmss}_{originalFileName}{extension}";
            string newFilePath = Path.Combine(directory, newFileName);

            File.Move(filePath, newFilePath);

            // Event Log
            logManager.LogEvent($"[ucOverrideNamesPanel] 파일 이름 변경 성공: {filePath} -> {newFilePath}");
        }

        private void btn_BaseClear_Click(object sender, EventArgs e)
        {
            // Debug Log
            logManager.LogDebug("[ucOverrideNamesPanel] btn_BaseClear_Click() - BaseDatePath 초기화");

            cb_BaseDatePath.SelectedIndex = -1;
            settingsManager.RemoveSection("SelectedBaseDatePath"); // 저장된 값 삭제
            folderWatcher?.Dispose();

            // Event Log
            logManager.LogEvent("[ucOverrideNamesPanel] BaseDatePath 해제 및 감시 중지");
        }

        private void Btn_SelectFolder_Click(object sender, EventArgs e)
        {
            // Debug Log
            logManager.LogDebug("[ucOverrideNamesPanel] Btn_SelectFolder_Click() 호출");

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

                        // Event Log
                        logManager.LogEvent($"[ucOverrideNamesPanel] 새로운 비교 경로 추가: {folderDialog.SelectedPath}");
                    }
                    else
                    {
                        MessageBox.Show("해당 폴더는 이미 추가되어 있습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        logManager.LogDebug("[ucOverrideNamesPanel] 이미 추가된 폴더 선택됨");
                    }
                }
            }
        }

        private void Btn_Remove_Click(object sender, EventArgs e)
        {
            // Debug Log
            logManager.LogDebug("[ucOverrideNamesPanel] Btn_Remove_Click() 호출");

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

                    // Event Log
                    logManager.LogEvent("[ucOverrideNamesPanel] 선택한 비교 경로 삭제 완료");
                }
            }
            else
            {
                MessageBox.Show("삭제할 항목을 선택하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                logManager.LogDebug("[ucOverrideNamesPanel] 삭제할 항목 미선택");
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

            // Event Log
            logManager.LogEvent($"[ucOverrideNamesPanel] 상태 업데이트 - {status}");
        }

        public void InitializePanel(bool isRunning)
        {
            UpdateStatusOnRun(isRunning);
        }

        public void LoadDataFromSettings()
        {
            // Debug Log
            logManager.LogDebug("[ucOverrideNamesPanel] LoadDataFromSettings() 호출");

            var baseFolders = settingsManager.GetFoldersFromSection("[BaseFolder]");
            cb_BaseDatePath.Items.Clear();
            cb_BaseDatePath.Items.AddRange(baseFolders.ToArray());

            var comparePaths = settingsManager.GetFoldersFromSection("[TargetComparePath]");
            lb_TargetComparePath.Items.Clear();
            foreach (var path in comparePaths)
            {
                lb_TargetComparePath.Items.Add(path);
            }

            // Event Log
            logManager.LogEvent("[ucOverrideNamesPanel] 설정에서 BaseFolder 및 TargetComparePath 로드 완료");
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

            // Debug Log
            logManager.LogDebug($"[ucOverrideNamesPanel] UpdateStatus() - 현재 상태: {status}");
        }

        public void CompareAndRenameFiles()
        {
            // Debug Log
            logManager.LogDebug("[ucOverrideNamesPanel] CompareAndRenameFiles() 호출");

            try
            {
                string baselineFolder = Path.Combine(settingsManager.GetBaseFolder(), "Baseline");
                if (!Directory.Exists(baselineFolder))
                {
                    logManager.LogError("[ucOverrideNamesPanel] Baseline 폴더가 존재하지 않습니다.");
                    return;
                }
        
                // Baseline 파일 데이터 추출
                var baselineFiles = Directory.GetFiles(baselineFolder, "*.info");
                var baselineData = ExtractBaselineData(baselineFiles);
        
                if (baselineData.Count == 0)
                {
                    logManager.LogEvent("[ucOverrideNamesPanel] Baseline 폴더에 유효한 .info 파일이 없습니다.");
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
        
                                // 성공 로그 기록 (Event Log)
                                logManager.LogEvent($"[ucOverrideNamesPanel] 파일 이름 변경 성공: {targetFile} -> {newFilePath}");
                            }
                        }
                        catch (IOException ioEx)
                        {
                            // 파일 작업 중 오류를 이벤트 로그에 기록 (Error Log)
                            logManager.LogError($"[ucOverrideNamesPanel] 파일 작업 중 오류 발생: {ioEx.Message}\n파일: {targetFile}");
        
                            // 작업 진행 상황 (Event Log)
                            logManager.LogEvent($"[ucOverrideNamesPanel] 파일 처리 중 오류 발생, 다음 파일로 진행: {targetFile}");
                        }
                        catch (UnauthorizedAccessException uaEx)
                        {
                            logManager.LogError($"[ucOverrideNamesPanel] 파일 접근 권한 오류: {uaEx.Message}\n파일: {targetFile}");
                            logManager.LogEvent($"[ucOverrideNamesPanel] 권한 오류 발생, 다음 파일로 진행: {targetFile}");
                        }
                        catch (Exception ex)
                        {
                            logManager.LogError($"[ucOverrideNamesPanel] 예기치 않은 오류 발생: {ex.Message}\n파일: {targetFile}");
                            logManager.LogEvent($"[ucOverrideNamesPanel] 예기치 않은 오류 발생, 다음 파일로 진행: {targetFile}");
                        }
                    }
                }
        
                logManager.LogEvent("[ucOverrideNamesPanel] 파일 이름 변환 작업이 완료되었습니다.");
            }
            catch (Exception ex)
            {
                // 최상위 예외 처리 (예외를 놓치지 않도록 최종 방어선)
                logManager.LogError($"[ucOverrideNamesPanel] 전체 작업 중 예기치 않은 오류 발생: {ex.Message}");
            }
        }

        public void StartProcessing()
        {
            // Debug Log
            logManager.LogDebug("[ucOverrideNamesPanel] StartProcessing() 호출 - 상시 가동 루프 시작");

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
            // Debug Log
            logManager.LogDebug("[ucOverrideNamesPanel] InitializeBaselineWatcher() 호출");

            var baseFolder = settingsManager.GetFoldersFromSection("[BaseFolder]").FirstOrDefault();
            if (string.IsNullOrEmpty(baseFolder) || !Directory.Exists(baseFolder))
            {
                MessageBox.Show("BaseFolder를 선택하거나 유효한 경로를 설정하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                logManager.LogError("[ucOverrideNamesPanel] 유효하지 않은 BaseFolder로 인해 BaselineWatcher 초기화 실패");
                return;
            }

            var baselineFolder = Path.Combine(baseFolder, "Baseline");
            if (!Directory.Exists(baselineFolder))
            {
                MessageBox.Show("Baseline 폴더가 존재하지 않습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                logManager.LogError("[ucOverrideNamesPanel] Baseline 폴더가 없어 BaselineWatcher 초기화 실패");
                return;
            }

            baselineWatcher = new FileSystemWatcher(baselineFolder, "*.info")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            baselineWatcher.Created += OnBaselineFileChanged;
            baselineWatcher.Changed += OnBaselineFileChanged;
            baselineWatcher.EnableRaisingEvents = true;

            // Event Log
            logManager.LogEvent($"[ucOverrideNamesPanel] BaselineWatcher 초기화 완료 - 경로: {baselineFolder}");
        }

        private void OnBaselineFileChanged(object sender, FileSystemEventArgs e)
        {
            // Debug Log
            logManager.LogDebug($"[ucOverrideNamesPanel] OnBaselineFileChanged() - Baseline 파일 변경 감지: {e.FullPath}");

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
                                        logManager.LogDebug($"[ucOverrideNamesPanel] 원본 파일을 찾을 수 없어 건너뜀: {targetFile}");
                                    }
                                    continue;
                                }

                                File.Move(targetFile, newFilePath);

                                // 변경 내용 로그 기록
                                LogFileRename(targetFile, newFilePath);
                            }
                            catch (IOException ioEx)
                            {
                                logManager.LogError($"[ucOverrideNamesPanel] 파일 이동 중 오류 발생: {ioEx.Message}\n파일: {targetFile}");
                            }
                            catch (Exception ex)
                            {
                                logManager.LogError($"[ucOverrideNamesPanel] 예기치 않은 오류 발생: {ex.Message}\n파일: {targetFile}");
                            }
                        }
                    }
                }
            }
        }

        private void LogFileRename(string oldPath, string newPath)
        {
            // 변경된 파일 이름만 추출
            string changedFileName = Path.GetFileName(newPath);
        
            // Event Log
            string logMessage = $"[ucOverrideNamesPanel] 파일 이름 변경: {oldPath} -> {changedFileName}";
            logManager.LogEvent(logMessage);
        
            // Debug Log
            if (settingsManager.IsDebugMode)
            {
                logManager.LogDebug($"[ucOverrideNamesPanel] 파일 변경 상세 로그 기록: {logMessage}");
            }
        }

        private Dictionary<string, (string TimeInfo, string Prefix, string CInfo)> ExtractBaselineData(string[] files)
        {
            // Debug Log
            logManager.LogDebug("[ucOverrideNamesPanel] ExtractBaselineData() 호출");

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

            // Event Log
            logManager.LogEvent("[ucOverrideNamesPanel] Baseline 파일 분석 완료");
            return baselineData;
        }

        private string ProcessTargetFile(string targetFile, Dictionary<string, (string TimeInfo, string Prefix, string CInfo)> baselineData)
        {
            // 파일이 준비될 때까지 대기
            if (!WaitForFileReady(targetFile))
            {
                logManager.LogError($"[ucOverrideNamesPanel] 파일을 처리할 수 없습니다(잠김 상태): {targetFile}");
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
