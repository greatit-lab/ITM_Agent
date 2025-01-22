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
        private FileSystemWatcher folderWatcher;   // 폴더 감시기
        private List<string> regexFolders;         // 정규표현식과 폴더 정보 저장
        private string baseFolder;                 // BaseFolder 저장

        public event Action<string, Color> StatusUpdated;

        private ucConfigurationPanel configPanel;
        private FileSystemWatcher baselineWatcher;
        private readonly LogManager logManager;

        // ----------------------------
        // (1) 안정화 감지를 위한 필드
        // ----------------------------
        private readonly Dictionary<string, FileTrackingInfo> trackedFiles = new Dictionary<string, FileTrackingInfo>();
        private System.Threading.Timer stabilityTimer;
        private readonly object trackingLock = new object();
        private const double StabilitySeconds = 2.0;   // "안정화" 판단까지 대기시간 (초)

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

        #region 안정화 감지용 내부 클래스/메서드

        /// <summary>
        /// 파일 추적 정보를 저장하기 위한 클래스
        /// </summary>
        private class FileTrackingInfo
        {
            public DateTime LastEventTime { get; set; }     // 마지막 이벤트가 감지된 시각
            public long LastSize { get; set; }              // 마지막으로 확인된 파일 크기
            public DateTime LastWriteTime { get; set; }     // 마지막으로 확인된 파일 수정 시간
        }

        /// <summary>
        /// 파일 변경 이벤트 이후, "파일이 안정화되었는지"를 주기적으로 체크하는 메서드
        /// </summary>
        private void CheckFileStability()
        {
            var now = DateTime.Now;
            List<string> stableFiles = new List<string>();

            // 1) 현재 딕셔너리 상태 스냅샷 확보
            lock (trackingLock)
            {
                var snapshot = trackedFiles.ToList(); // KeyValuePair<string, FileTrackingInfo>
                foreach (var kv in snapshot)
                {
                    string filePath = kv.Key;
                    var info = kv.Value;

                    // 파일 크기/수정시각 재확인
                    long currentSize = GetFileSizeSafe(filePath);
                    DateTime currentWriteTime = GetLastWriteTimeSafe(filePath);

                    // 크기나 수정시각이 달라졌다면, 아직 안정화되지 않음
                    if (currentSize != info.LastSize || currentWriteTime != info.LastWriteTime)
                    {
                        info.LastEventTime = now;
                        info.LastSize = currentSize;
                        info.LastWriteTime = currentWriteTime;
                        continue;
                    }

                    // (변경 없음) => 마지막 이벤트 시각 이후 경과 시간 확인
                    double diffSec = (now - info.LastEventTime).TotalSeconds;
                    if (diffSec >= StabilitySeconds)
                    {
                        // 일정 시간동안 변경이 없으면 "안정화"로 간주
                        stableFiles.Add(filePath);
                    }
                }
            }

            // 2) 안정화된 파일 처리
            foreach (var filePath in stableFiles)
            {
                ProcessStableFile(filePath);
            }

            // 3) 처리 완료된 파일은 Dictionary에서 제거
            lock (trackingLock)
            {
                foreach (var filePath in stableFiles)
                {
                    if (trackedFiles.ContainsKey(filePath))
                    {
                        trackedFiles.Remove(filePath);
                    }
                }

                // 더 이상 추적 중인 파일이 없으면 타이머 해제
                if (trackedFiles.Count == 0 && stabilityTimer != null)
                {
                    stabilityTimer.Dispose();
                    stabilityTimer = null;
                }
            }
        }

        /// <summary>
        /// 안정화된 파일을 실제로 처리하는 메서드 (기존 OnFileChanged로직 대체)
        /// </summary>
        private void ProcessStableFile(string filePath)
        {
            try
            {
                // 충분히 긴 대기(파일 잠금 해제) 로직
                if (!WaitForFileReady(filePath, maxRetries: 30, delayMilliseconds: 1000))
                {
                    logManager.LogError($"[ucOverrideNamesPanel] 파일을 처리할 수 없습니다.(장기 잠김): {filePath}");
                    return;
                }

                if (File.Exists(filePath))
                {
                    // 기존에 OnFileChanged에서 하던 처리를 그대로 진행
                    DateTime? dateTimeInfo = ExtractDateTimeFromFile(filePath);
                    if (dateTimeInfo.HasValue)
                    {
                        logManager.LogEvent($"[ucOverrideNamesPanel] Baseline 대상 파일 감지: {filePath}");
                        CreateBaselineInfoFile(filePath, dateTimeInfo.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                logManager.LogError($"[ucOverrideNamesPanel] ProcessStableFile() 중 오류: {ex.Message}\n파일: {filePath}");
            }
        }

        /// <summary>
        /// 안전하게 파일 크기를 구하는 헬퍼
        /// </summary>
        private long GetFileSizeSafe(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var fi = new FileInfo(filePath);
                    return fi.Length;
                }
            }
            catch { /* 무시 */ }
            return 0;
        }

        /// <summary>
        /// 안전하게 LastWriteTime을 구하는 헬퍼
        /// </summary>
        private DateTime GetLastWriteTimeSafe(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    return File.GetLastWriteTime(filePath);
                }
            }
            catch { /* 무시 */ }
            return DateTime.MinValue;
        }

        #endregion


        #region 기존 로직 + FileSystemWatcher 이벤트 처리 수정

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

                // (중요) "이벤트가 들어오면 Dictionary에 기록"만 수행
                folderWatcher.Created += OnFileSystemEvent;
                folderWatcher.Changed += OnFileSystemEvent;
            }
            else
            {
                // Error Log
                logManager.LogError($"[ucOverrideNamesPanel] 지정된 경로가 존재하지 않습니다: {path}");
            }
        }

        /// <summary>
        /// FileSystemWatcher 이벤트 핸들러 (Created / Changed)
        /// 파일을 바로 처리하지 않고, Dictionary에 추적 정보를 기록해둠
        /// </summary>
        private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
        {
            // 잠시 기록만 해두고, 처리 로직은 Timer에서 진행
            lock (trackingLock)
            {
                if (!trackedFiles.TryGetValue(e.FullPath, out FileTrackingInfo info))
                {
                    info = new FileTrackingInfo
                    {
                        LastEventTime = DateTime.Now,
                        LastSize = GetFileSizeSafe(e.FullPath),
                        LastWriteTime = GetLastWriteTimeSafe(e.FullPath)
                    };
                    trackedFiles[e.FullPath] = info;
                }
                else
                {
                    // 이미 추적 중인 파일이면 정보 갱신
                    info.LastEventTime = DateTime.Now;
                    info.LastSize = GetFileSizeSafe(e.FullPath);
                    info.LastWriteTime = GetLastWriteTimeSafe(e.FullPath);
                }
            }

            // 타이머가 없으면 생성 (2초 간격으로 CheckFileStability 실행)
            if (stabilityTimer == null)
            {
                stabilityTimer = new System.Threading.Timer(_ => CheckFileStability(), null, 2000, 2000);
            }
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

            var baseFolder = settingsManager.GetFoldersFromSection("[BaseFolder]").FirstOrDefault()
                             ?? AppDomain.CurrentDomain.BaseDirectory;

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
                var confirmResult = MessageBox.Show("선택한 항목을 삭제하시겠습니까?", "삭제 확인", 
                                                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

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

        #endregion

        #region 기존 메서드(읽기/처리 로직) 변경 없이 재사용

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
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, 
                                                   FileShare.ReadWrite | FileShare.Delete))
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

        #endregion

        #region BaselineWatcher (기존 그대로)

        private void InitializeBaselineWatcher()
        {
            if (baselineWatcher != null)
            {
                // 혹은 baselineWatcher.Dispose() 후 null 할당
                baselineWatcher.EnableRaisingEvents = false;
                baselineWatcher.Dispose();
                baselineWatcher = null;
            }
        
            logManager.LogDebug("[ucOverrideNamesPanel] InitializeBaselineWatcher() 호출");
        
            var baseFolder = settingsManager.GetFoldersFromSection("[BaseFolder]").FirstOrDefault();
            if (string.IsNullOrEmpty(baseFolder) || !Directory.Exists(baseFolder))
            {
                logManager.LogError("[ucOverrideNamesPanel] 유효하지 않은 BaseFolder로 인해 BaselineWatcher 초기화 불가");
                return;
            }
        
            var baselineFolder = Path.Combine(baseFolder, "Baseline");
            if (!Directory.Exists(baselineFolder))
            {
                logManager.LogError("[ucOverrideNamesPanel] Baseline 폴더가 존재하지 않아 BaselineWatcher 초기화 불가");
                return;
            }
        
            baselineWatcher = new FileSystemWatcher(baselineFolder, "*.info")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };
        
            baselineWatcher.Created += OnBaselineFileChanged;
            baselineWatcher.Changed += OnBaselineFileChanged;
            baselineWatcher.EnableRaisingEvents = true;
        
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

        #endregion

        #region 기타 기존 메서드들 (상태 갱신, CompareAndRenameFiles 등) 그대로

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                baselineWatcher?.Dispose();
                folderWatcher?.Dispose();
                stabilityTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
