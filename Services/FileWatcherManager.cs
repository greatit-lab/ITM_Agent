// Services\FileWatcherManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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
        private readonly TimeSpan duplicateEventThreshold = TimeSpan.FromSeconds(2); // 중복 이벤트 방지 시간
        
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
            StopWatchers();
            var targetFolders = settingsManager.GetFoldersFromSection("[TargetFolders]");
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
            }
            logManager.LogEvent($"{watchers.Count} watchers initialized.");
        }

        public void StartWatching()
        {
            foreach (var w in watchers)
            {
                w.EnableRaisingEvents = true;
            }
            isRunning = true;
            logManager.LogEvent("File monitoring started.");
        }

        public void StopWatchers()
        {
            foreach (var w in watchers)
            {
                w.EnableRaisingEvents = false;
                w.Dispose();
            }
            watchers.Clear();
            isRunning = false;
            logManager.LogEvent("File monitoring stopped.");
        }
        
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (!isRunning)
            {
                logManager.LogEvent("File event ignored because the status is not Running.");
                if (isDebugMode)
                {
                    logManager.LogDebug($"Ignored event: {e.ChangeType} for file: {e.FullPath} because monitoring is not running.");
                }
                return;
            }
        
            // 삭제된 파일에 대한 중복 이벤트 방지
            if (deletedFiles.Contains(e.FullPath) && e.ChangeType == WatcherChangeTypes.Changed)
            {
                if (isDebugMode)
                {
                    logManager.LogDebug($"Ignored File Modified: for deleted file: {e.FullPath}");
                }
                return;
            }
        
            string eventType = e.ChangeType switch
            {
                WatcherChangeTypes.Created => "File Created:",
                WatcherChangeTypes.Changed => "File Modified:",
                WatcherChangeTypes.Deleted => "File Deleted:",
                _ => "Unknown Event:"
            };
        
            // 중복 이벤트 방지
            if (IsDuplicateEvent(e.FullPath))
            {
                if (isDebugMode)
                {
                    logManager.LogDebug($"Duplicate event ignored: {eventType} for file: {e.FullPath}");
                }
                return;
            }
        
            try
            {
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                    case WatcherChangeTypes.Changed:
                        if (File.Exists(e.FullPath))
                        {
                            string destinationFolder = ProcessFile(e.FullPath);
                            logManager.LogEvent($"{eventType} {e.FullPath} -> copied to: {destinationFolder}");
                            if (isDebugMode)
                            {
                                logManager.LogDebug($"Processed file: {e.FullPath} -> Destination: {destinationFolder}");
                            }
                        }
                        break;
        
                    case WatcherChangeTypes.Deleted:
                        logManager.LogEvent($"File Deleted: {e.FullPath}");
                        if (isDebugMode)
                        {
                            logManager.LogDebug($"Deleted file tracked: {e.FullPath}");
                        }
                        deletedFiles.Add(e.FullPath); // 삭제된 파일을 목록에 추가
                        ScheduleDeletedFileCleanup(e.FullPath);
                        break;
        
                    default:
                        logManager.LogEvent($"Unhandled event: {eventType} {e.FullPath}");
                        break;
                }
            }
            catch (Exception ex)
            {
                logManager.LogEvent($"Error processing file: {e.FullPath}. Exception: {ex.Message}");
                if (isDebugMode)
                {
                    logManager.LogDebug($"Error: {ex.Message} for file: {e.FullPath}");
                }
            }
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
                    if (now - lastProcessed < duplicateEventThreshold)
                    {
                        // 중복 이벤트
                        return true;
                    }
                }
        
                // 마지막 처리 시간 갱신
                fileProcessTracker[filePath] = now;
                return false;
            }
        }
        
        private string ProcessFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            var regexList = settingsManager.GetRegexList();
        
            foreach (var kvp in regexList)
            {
                if (Regex.IsMatch(fileName, kvp.Key))
                {
                    string destinationFolder = kvp.Value; // 지정된 폴더 경로만 사용
                    string destinationFile = Path.Combine(destinationFolder, fileName);
                    
                    try
                    {
                        Directory.CreateDirectory(destinationFolder);
                        File.Copy(filePath, destinationFile, true);
                        return destinationFolder; // 복사된 폴더 경로 반환
                    }
                    catch (Exception ex)
                    {
                        logManager.LogEvent($"Error copying file: {fileName}. Exception: {ex.Message}");
                    }
                }
            }
            logManager.LogEvent($"No matching regex for file: {fileName}");
            return null;
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
