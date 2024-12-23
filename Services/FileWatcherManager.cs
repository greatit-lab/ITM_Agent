// Services\FileWatcherManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

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
        private readonly HashSet<string> processedFiles = new HashSet<string>(); // 처리된 파일 추적
        private readonly HashSet<string> recentlyCreatedFiles = new HashSet<string>(); // 최근 생성된 파일 추적
        private bool isRunning = false;
        private readonly Dictionary<string, DateTime> lastModifiedFiles = new Dictionary<string, DateTime>(); // 수정 시간 추적

        public FileWatcherManager(SettingsManager settings, LogManager logger)
        {
            this.settingsManager = settings;
            this.logManager = logger;
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
        
            string eventType = e.ChangeType switch
            {
                WatcherChangeTypes.Created => "File Created:",
                WatcherChangeTypes.Changed => "File Modified:",
                WatcherChangeTypes.Deleted => "File Deleted:",
                _ => "Unknown Event:"
            };
        
            // Log 기본 이벤트
            logManager.LogEvent($"{eventType} {e.FullPath}");
        
            if (isDebugMode)
            {
                logManager.LogDebug($"Event detected: {eventType} for file: {e.FullPath}");
            }
        
            try
            {
                if (e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Changed)
                {
                    if (File.Exists(e.FullPath))
                    {
                        if (isDebugMode)
                        {
                            logManager.LogDebug($"Processing file: {e.FullPath} after {eventType}");
                        }
        
                        string destinationFolder = ProcessFile(e.FullPath);
        
                        if (!string.IsNullOrEmpty(destinationFolder))
                        {
                            logManager.LogEvent($"{eventType} {e.FullPath} -> copied to: {destinationFolder}");
                            if (isDebugMode)
                            {
                                logManager.LogDebug($"File successfully copied: {e.FullPath} to folder: {destinationFolder}");
                            }
                        }
                        else
                        {
                            if (isDebugMode)
                            {
                                logManager.LogDebug($"File processing skipped: {e.FullPath}. No matching regex or other conditions failed.");
                            }
                        }
                    }
                }
                else if (e.ChangeType == WatcherChangeTypes.Deleted)
                {
                    if (isDebugMode)
                    {
                        logManager.LogDebug($"File deleted: {e.FullPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                logManager.LogEvent($"Error processing file: {e.FullPath}");
                if (isDebugMode)
                {
                    logManager.LogDebug($"Error during file processing: {e.FullPath}. Exception: {ex.Message}");
                }
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
                        logManager.LogError($"Error copying file: {fileName}. Exception: {ex.Message}");
                    }
                }
            }
        
            logManager.LogEvent($"No matching regex for file: {fileName}");
            return null; // 복사 실패 시 null 반환
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
