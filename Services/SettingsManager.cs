// Services\SettingsManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ITM_Agent.Services
{
    /// <summary>
    /// Settings.ini 파일을 관리하며, 특정 섹션([Eqpid], [BaseFolder], [TargetFolders], [ExcludeFolders], [Regex]) 값들을
    /// 읽고/쓰고/수정하는 기능을 제공하는 클래스입니다.
    /// </summary>
    public class SettingsManager
    {
        private readonly string settingsFilePath;
        private readonly object fileLock = new object();

        public SettingsManager(string settingsFilePath)
        {
            this.settingsFilePath = settingsFilePath;
            EnsureSettingsFileExists();
        }

        private void EnsureSettingsFileExists()
        {
            if (!File.Exists(settingsFilePath))
            {
                using (File.Create(settingsFilePath)) { }
            }
        }

        public string GetEqpid()
        {
            if (!File.Exists(settingsFilePath)) return null;

            var lines = File.ReadAllLines(settingsFilePath);
            bool eqpidSectionFound = false;
            foreach (string line in lines)
            {
                if (line.Trim() == "[Eqpid]")
                {
                    eqpidSectionFound = true;
                    continue;
                }
                if (eqpidSectionFound && line.StartsWith("Eqpid = "))
                {
                    return line.Substring("Eqpid =".Length).Trim();
                }
            }
            return null;
        }

        private void WriteToFileSafely(string[] lines)
        {
            lock (fileLock)
            {
                File.WriteAllLines(settingsFilePath, lines);
            }
        }
        
        public void SetEqpid(string eqpid)
        {
            var lines = File.Exists(settingsFilePath) ? File.ReadAllLines(settingsFilePath).ToList() : new List<string>();
            int eqpidIndex = lines.FindIndex(l => l.Trim() == "[Eqpid]");
        
            if (eqpidIndex == -1)
            {
                lines.Add("[Eqpid]");
                lines.Add("Eqpid = " + eqpid);
            }
            else
            {
                lines[eqpidIndex + 1] = "Eqpid = " + eqpid;
            }
        
            WriteToFileSafely(lines.ToArray());
        }

        public bool IsReadyToRun()
        {
            return HasValuesInSection("[BaseFolder]") &&
                   HasValuesInSection("[TargetFolders]") &&
                   HasValuesInSection("[Regex]");
        }

        private bool HasValuesInSection(string section)
        {
            if (!File.Exists(settingsFilePath)) return false;

            var lines = File.ReadAllLines(settingsFilePath).ToList();
            int sectionIndex = lines.FindIndex(line => line.Trim() == section);
            if (sectionIndex == -1) return false;

            int endIndex = lines.FindIndex(sectionIndex + 1, line => line.StartsWith("[") || string.IsNullOrWhiteSpace(line));
            if (endIndex == -1) endIndex = lines.Count;

            return lines.Skip(sectionIndex + 1).Take(endIndex - sectionIndex - 1)
                        .Any(line => !string.IsNullOrWhiteSpace(line));
        }

        public List<string> GetFoldersFromSection(string section)
        {
            var folders = new List<string>();
            if (!File.Exists(settingsFilePath))
                return folders;

            var lines = File.ReadAllLines(settingsFilePath);
            bool inSection = false;
            foreach (var line in lines)
            {
                if (line.Trim() == section)
                {
                    inSection = true;
                    continue;
                }
                if (inSection)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("["))
                        break;
                    folders.Add(line.Trim());
                }
            }
            return folders;
        }

        public Dictionary<string, string> GetRegexList()
        {
            var regexList = new Dictionary<string, string>();
            if (!File.Exists(settingsFilePath)) return regexList;

            var lines = File.ReadAllLines(settingsFilePath);
            bool inRegexSection = false;

            foreach (var line in lines)
            {
                if (line.Trim() == "[Regex]")
                {
                    inRegexSection = true;
                    continue;
                }

                if (inRegexSection)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("["))
                        break;

                    var parts = line.Split(new[] { "->" }, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        regexList[parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }
            return regexList;
        }

        /// <summary>
        /// 해당 section에 folders 목록을 반영하는 메서드.
        /// section이 이미 존재한다면 기존 내용을 삭제하고 folders를 기록.
        /// section이 없다면 새로 추가.
        /// </summary>
        public void SetFoldersToSection(string section, List<string> folders)
        {
            var lines = File.Exists(settingsFilePath) ? File.ReadAllLines(settingsFilePath).ToList() : new List<string>();

            int sectionIndex = lines.FindIndex(l => l.Trim() == section);
            if (sectionIndex == -1)
            {
                // 섹션이 없으면 추가
                if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines.Last()))
                {
                    lines.Add("");
                }
                lines.Add(section);
                foreach (var folder in folders)
                {
                    lines.Add(folder);
                }
                lines.Add(""); // 다음 섹션과 구분을 위해 빈 줄 추가(선택 사항)
            }
            else
            {
                // 섹션이 있을 경우 endIndex 찾기
                int endIndex = lines.FindIndex(sectionIndex + 1, line => line.StartsWith("[") || string.IsNullOrWhiteSpace(line));
                if (endIndex == -1) endIndex = lines.Count;

                // 기존 섹션 내용을 제거하고 새로운 목록 삽입
                lines.RemoveRange(sectionIndex + 1, endIndex - sectionIndex - 1);

                foreach (var folder in folders)
                {
                    lines.Insert(sectionIndex + 1, folder);
                    sectionIndex++;
                }

                // 마지막에 빈 줄 추가
                if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines.Last()))
                {
                    lines.Add("");
                }
            }
            File.WriteAllLines(settingsFilePath, lines);
        }

        /// <summary>
        /// BaseFolder를 설정하는 메서드
        /// </summary>
        public void SetBaseFolder(string folderPath)
        {
            var lines = File.Exists(settingsFilePath) ? File.ReadAllLines(settingsFilePath).ToList() : new List<string>();

            int sectionIndex = lines.FindIndex(l => l.Trim() == "[BaseFolder]");
            if (sectionIndex == -1)
            {
                // 섹션 없으면 추가
                if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines.Last()))
                {
                    lines.Add("");
                }
                lines.Add("[BaseFolder]");
                lines.Add(folderPath);
                lines.Add("");
            }
            else
            {
                int endIndex = lines.FindIndex(sectionIndex + 1, line => line.StartsWith("[") || string.IsNullOrWhiteSpace(line));
                if (endIndex == -1) endIndex = lines.Count;

                var updatedSection = new List<string> { "[BaseFolder]", folderPath, "" };
                lines = lines.Take(sectionIndex)
                             .Concat(updatedSection)
                             .Concat(lines.Skip(endIndex))
                             .ToList();
            }

            File.WriteAllLines(settingsFilePath, lines);
        }

        /// <summary>
        /// Regex 리스트를 설정하는 메서드.
        /// 주어진 Dictionary<string,string>를 [Regex] 섹션에 재작성.
        /// </summary>
        public void SetRegexList(Dictionary<string, string> regexDict)
        {
            var lines = File.Exists(settingsFilePath) ? File.ReadAllLines(settingsFilePath).ToList() : new List<string>();

            // [Regex] 섹션 초기화
            int sectionIndex = lines.FindIndex(l => l.Trim() == "[Regex]");
            if (sectionIndex != -1)
            {
                int endIndex = lines.FindIndex(sectionIndex + 1, line => line.StartsWith("[") || string.IsNullOrWhiteSpace(line));
                if (endIndex == -1) endIndex = lines.Count;
                lines.RemoveRange(sectionIndex, endIndex - sectionIndex);
            }

            // [Regex] 섹션 새로 추가
            if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines.Last()))
            {
                lines.Add("");
            }

            lines.Add("[Regex]");
            foreach (var kvp in regexDict)
            {
                lines.Add($"{kvp.Key} -> {kvp.Value}");
            }
            lines.Add("");

            File.WriteAllLines(settingsFilePath, lines);
        }
        
        public void ResetExceptEqpid()
        {
            var lines = File.Exists(settingsFilePath) ? File.ReadAllLines(settingsFilePath).ToList() : new List<string>();
            var eqpidLines = lines.Where(line => line.StartsWith("[Eqpid]") || line.StartsWith("Eqpid =")).ToList();
        
            // Settings 파일 초기화
            File.WriteAllText(settingsFilePath, string.Empty);
        
            // Eqpid 섹션 복원
            File.AppendAllLines(settingsFilePath, eqpidLines);
        }
        
        public void LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("File not found.", filePath);
        
            File.Copy(filePath, settingsFilePath, overwrite: true);
        }
        
        public void SaveToFile(string filePath)
        {
            File.Copy(settingsFilePath, filePath, overwrite: true);
        }
        
        public void SetType(string type)
        {
            var lines = File.Exists(settingsFilePath) ? File.ReadAllLines(settingsFilePath).ToList() : new List<string>();
            int sectionIndex = lines.FindIndex(l => l.Trim() == "[Eqpid]");
            if (sectionIndex == -1)
            {
                lines.Add("[Eqpid]");
                lines.Add($"Type = {type}");
            }
            else
            {
                int typeIndex = lines.FindIndex(sectionIndex + 1, l => l.StartsWith("Type ="));
                if (typeIndex != -1)
                    lines[typeIndex] = $"Type = {type}";
                else
                    lines.Insert(sectionIndex + 1, $"Type = {type}");
            }
            WriteToFileSafely(lines.ToArray());
        }
        
        public string GetType()
        {
            var lines = File.Exists(settingsFilePath) ? File.ReadAllLines(settingsFilePath).ToList() : new List<string>();
            int sectionIndex = lines.FindIndex(l => l.Trim() == "[Eqpid]");
            if (sectionIndex != -1)
            {
                var typeLine = lines.Skip(sectionIndex + 1).FirstOrDefault(l => l.StartsWith("Type ="));
                if (!string.IsNullOrEmpty(typeLine))
                    return typeLine.Split('=')[1].Trim();
            }
            return null;
        }

    }
}
