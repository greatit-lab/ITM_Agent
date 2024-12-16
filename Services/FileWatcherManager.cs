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
        private List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();
        private readonly SettingsManager settingsManager;
        private readonly LogManager logManager;
        private bool isRunning = false;

        public FileWatcherManager(SettingsManager settings, LogManager logger)
        {
            this.settingsManager = settings;
            this.logManager = logger;
        }

        public void InitializeWatchers()
        {
            StopWatchers(); // 기존 감시자 정리
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
                logManager.LogEvent("File event ignored because the status is not Running.", true);
                return;
            }

            logManager.LogEvent($"File event detected: {e.ChangeType} - {e.FullPath}");
            var regexList = settingsManager.GetRegexList();
            bool copied = false;

            foreach (var kvp in regexList)
            {
                if (Regex.IsMatch(e.Name, kvp.Key))
                {
                    string destination = Path.Combine(kvp.Value, e.Name);
                    try
                    {
                        Directory.CreateDirectory(kvp.Value);
                        File.Copy(e.FullPath, destination, true);
                        logManager.LogEvent($"File copied: {e.Name} -> {destination}");
                        copied = true;
                    }
                    catch (Exception ex)
                    {
                        logManager.LogEvent($"Error copying file: {e.FullPath}. {ex.Message}", true);
                    }
                    break;
                }
            }

            if (!copied)
            {
                logManager.LogEvent($"No matching regex for file: {e.FullPath}");
            }
        }
    }
}
