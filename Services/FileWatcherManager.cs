// Services\FileWatcherManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ITM_Agent.Services
{
    /// <summary>
    /// FileSystemWatcher를 사용하여 지정된 타겟 폴더(TargetFolders)를 감시하고,
    /// 발생하는 파일 변경 이벤트에 따라 정규표현식(Regex) 패턴 매칭 후 파일을 지정된 폴더로 복사하는 로직을 담당합니다.
    /// </summary>
    public class FileWatcherManager
    {
        private readonly List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();
        private readonly SettingsManager settingsManager;
        private readonly LogManager logManager;
        private readonly Dictionary<string, DateTime> lastModifiedFiles = new Dictionary<string, DateTime>(); // 수정 시간 추적
        private readonly HashSet<string> recentlyCreatedFiles = new HashSet<string>(); // 최근 생성된 파일 추적
        private readonly HashSet<string> deletedFiles = new HashSet<string>(); // 삭제된 파일 추적
        private readonly Dictionary<string, DateTime> fileProcessTracker = new Dictionary<string, DateTime>(); // 파일 처리 추적
        private readonly TimeSpan duplicateEventThreshold = TimeSpan.FromSeconds(5); // 중복 이벤트 방지 시간

        private bool isDebugMode;

        private bool isRunning = false;
        // private readonly bool isDebugMode;

        // Debug Mode 상태 속성
        public bool IsDebugMode { get; set; } = false;

        public FileWatcherManager(SettingsManager settingsManager, LogManager logManager, bool isDebugMode)
        {
            this.settingsManager = settingsManager;
            this.logManager = logManager;
            this.isDebugMode = isDebugMode; // MainForm 에서 전달받은 디버그 모드 상태
        }

        public void UpdateDebugMode(bool isDebug)
        {
            this.isDebugMode = isDebug; // 디버그 모드 상태 업데이트
        }

        public void InitializeWatchers()
        {
            StopWatchers(); // 기존 Watcher 중지
            var targetFolders = settingsManager.GetFoldersFromSection("[TargetFolders]");
            if (targetFolders.Count == 0)
            {
                logManager.LogEvent("No target folders configured for monitoring.");
                return;
            }

            foreach (var folder in targetFolders)
            {
                if (!Directory.Exists(folder))
                {
                    logManager.LogEvent($"Folder does not exist: {folder}", true);
                    continue;
                }

                var watcher = new FileSystemWatcher
                {
                    Path = folder,
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
                };

                watcher.Created += OnFileChanged;
                watcher.Changed += OnFileChanged;
                watcher.Deleted += OnFileChanged;

                watchers.Add(watcher);

                if (isDebugMode)
                {
                    logManager.LogDebug($"Initialized watcher for folder: {folder}");
                }
            }

            logManager.LogEvent($"{watchers.Count} watcher(s) initialized.");
        }


        public void StartWatching()
        {
            if (isRunning)
            {
                logManager.LogEvent("File monitoring is already running.");
                return;
            }

            InitializeWatchers(); // 새로 초기화

            foreach (var watcher in watchers)
            {
                watcher.EnableRaisingEvents = true; // 이벤트 활성화
            }

            isRunning = true; // 상태 업데이트
            logManager.LogEvent("File monitoring started.");
            logManager.LogDebug($"Monitoring {watchers.Count} folder(s): {string.Join(", ", watchers.Select(w => w.Path))}");
        }


        public void StopWatchers()
        {
            foreach (var w in watchers)
            {
                w.EnableRaisingEvents = false;
                w.Created -= OnFileChanged;
                w.Changed -= OnFileChanged;
                w.Deleted -= OnFileChanged;
                w.Dispose();
            }
            watchers.Clear(); // 리스트 비우기
            isRunning = false; // 상태 업데이트
            logManager.LogEvent("File monitoring stopped.");
        }

        private async Task<bool> WaitForFileReadyAsync(string filePath, int maxRetries, int delayMilliseconds)
        {
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                if (IsFileReady(filePath))
                {
                    logManager.LogEvent($"File is ready: {filePath}");
                    return true;
                }

                logManager.LogEvent($"Retrying... File not ready: {filePath}, Attempt: {attempt + 1}");
                await Task.Delay(delayMilliseconds);
            }

            logManager.LogEvent($"File not ready after {maxRetries} attempts: {filePath}");
            return false;
        }

        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (!isRunning)
            {
                if (isDebugMode)
                    logManager.LogDebug($"File event ignored (not running): {e.FullPath}");
                return;
            }

            // 중복 이벤트 방지
            if (IsDuplicateEvent(e.FullPath))
            {
                if (isDebugMode)
                    logManager.LogDebug($"Duplicate event ignored: {e.ChangeType} - {e.FullPath}");
                return;
            }

            try
            {
                if (e.ChangeType == WatcherChangeTypes.Created)
                {
                    if (File.Exists(e.FullPath))
                    {
                        await Task.Run(() => ProcessFile(e.FullPath));
                    }
                }
                else if (e.ChangeType == WatcherChangeTypes.Deleted)
                {
                    logManager.LogEvent($"File Deleted: {e.FullPath}");
                }
            }
            catch (Exception ex)
            {
                logManager.LogEvent($"Error processing file: {e.FullPath}. Exception: {ex.Message}");
                if (isDebugMode)
                {
                    logManager.LogDebug($"Exception details: {ex.Message}");
                }
            }
        }

        private async Task<string> ProcessFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            var regexList = settingsManager.GetRegexList();

            foreach (var kvp in regexList)
            {
                if (Regex.IsMatch(fileName, kvp.Key))
                {
                    string destinationFolder = kvp.Value;
                    string destinationFile = Path.Combine(destinationFolder, fileName);

                    try
                    {
                        Directory.CreateDirectory(destinationFolder);

                        // 파일 준비 상태 확인
                        if (!await WaitForFileReady(filePath))
                        {
                            logManager.LogEvent($"File skipped (not ready): {filePath}");
                            return null;
                        }

                        // 파일 복사
                        File.Copy(filePath, destinationFile, true);
                        logManager.LogEvent($"File Created: {filePath} -> copied {destinationFolder}");
                        return destinationFolder;
                    }
                    catch (Exception ex)
                    {
                        logManager.LogEvent($"Error copying file: {filePath}. Exception: {ex.Message}");
                        if (isDebugMode)
                        {
                            logManager.LogDebug($"Error details: {ex.Message}");
                        }
                    }
                }
            }

            logManager.LogEvent($"No matching regex for file: {fileName}");
            return null;
        }

        private async Task<bool> WaitForFileReady(string filePath, int maxRetries = 10, int delayMilliseconds = 500)
        {
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                if (IsFileReady(filePath))
                {
                    return true;
                }

                // 디버그 모드일 경우 재시도 로그 기록
                if (isDebugMode)
                {
                    logManager.LogDebug($"Retrying access to file: {filePath}, Attempt: {attempt + 1}/{maxRetries}");
                }

                await Task.Delay(delayMilliseconds); // 대기
            }

            // 파일 준비되지 않음
            logManager.LogEvent($"File not ready after {maxRetries} retries: {filePath}");
            return false;
        }

        private void ScheduleDeletedFileCleanup(string filePath)
        {
            Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(_ =>
            {
                deletedFiles.Remove(filePath);
                if (isDebugMode)
                {
                    logManager.LogDebug($"Deleted file removed from tracking: {filePath}");
                }
            });
        }

        private bool IsDuplicateEvent(string filePath)
        {
            DateTime now = DateTime.Now;

            lock (fileProcessTracker)
            {
                if (fileProcessTracker.TryGetValue(filePath, out var lastProcessed))
                {
                    if ((now - lastProcessed).TotalMilliseconds < 500) // 500ms 이내 중복 처리 방지
                    {
                        return true;
                    }
                }

                // 이벤트 처리 시간 갱신
                fileProcessTracker[filePath] = now;
                return false;
            }
        }

        private bool IsFileReady(string filePath)
        {
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return true; // 파일이 준비됨
                }
            }
            catch (IOException)
            {
                return false; // 파일이 잠겨 있음
            }
        }

        private void ScheduleFileRemoval(string filePath, HashSet<string> fileSet)
        {
            Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(_ =>
            {
                fileSet.Remove(filePath);
            });
        }
    }
}
