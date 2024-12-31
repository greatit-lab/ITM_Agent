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
        public event Action RegexSettingsUpdated;

        private bool isDebugMode; // DebugMode 상태 저장

        public SettingsManager(string settingsFilePath)
        {
            this.settingsFilePath = settingsFilePath;
            EnsureSettingsFileExists();
        }

        // DebugMode 속성 추가
        public bool IsDebugMode
        {
            get => isDebugMode;
            set
            {
                isDebugMode = value;
                // 필요시 설정 파일에 저장하거나 관련 작업 수행 가능
            }
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

            // 기존 설정 파일 갱신 로직
            File.WriteAllLines(settingsFilePath, ConvertRegexListToLines(regexDict));

            // 변경 알림 이벤트 호출
            NotifyRegexSettingsUpdated();
        }

        private IEnumerable<string> ConvertRegexListToLines(Dictionary<string, string> regexDict)
        {
            var lines = new List<string> { "[Regex]" };
            lines.AddRange(regexDict.Select(kvp => $"{kvp.Key} -> {kvp.Value}"));
            return lines;
        }

        public void ResetExceptEqpid()
        {
            // 설정 파일의 모든 라인을 읽어옴
            var lines = File.ReadAllLines(settingsFilePath).ToList();

            // [Eqpid] 섹션 시작과 끝 라인 찾기
            int eqpidStartIndex = lines.FindIndex(line => line.Trim().Equals("[Eqpid]", StringComparison.OrdinalIgnoreCase));
            int eqpidEndIndex = lines.FindIndex(eqpidStartIndex + 1, line => line.StartsWith("[") || string.IsNullOrWhiteSpace(line));

            if (eqpidStartIndex == -1)
            {
                throw new InvalidOperationException("[Eqpid] 섹션이 설정 파일에 존재하지 않습니다.");
            }

            // [Eqpid] 섹션의 내용을 보존
            eqpidEndIndex = (eqpidEndIndex == -1) ? lines.Count : eqpidEndIndex;
            var eqpidSectionLines = lines.Skip(eqpidStartIndex).Take(eqpidEndIndex - eqpidStartIndex).ToList();

            // 설정 파일 초기화
            File.WriteAllText(settingsFilePath, string.Empty);

            // [Eqpid] 섹션 복원
            File.AppendAllLines(settingsFilePath, eqpidSectionLines);

            // 추가 공백 라인 추가
            File.AppendAllText(settingsFilePath, Environment.NewLine);
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

        /// <summary>
        /// 특정 섹션에서 키 값을 읽어옵니다.
        /// </summary>
        public string GetValueFromSection(string section, string key)
        {
            if (!File.Exists(settingsFilePath)) return null;

            var lines = File.ReadAllLines(settingsFilePath);
            bool inSection = false;

            foreach (string line in lines)
            {
                if (line.Trim() == $"[{section}]")
                {
                    inSection = true;
                    continue;
                }

                if (inSection)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("["))
                        break;

                    var keyValue = line.Split('=');
                    if (keyValue.Length == 2 && keyValue[0].Trim() == key)
                    {
                        return keyValue[1].Trim();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 특정 섹션에 키-값을 설정합니다.
        /// </summary>
        public void SetValueToSection(string section, string key, string value)
        {
            lock (fileLock)
            {
                var lines = File.Exists(settingsFilePath) ? File.ReadAllLines(settingsFilePath).ToList() : new List<string>();
                int sectionIndex = lines.FindIndex(l => l.Trim() == $"[{section}]");

                if (sectionIndex == -1)
                {
                    // 섹션 추가
                    if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines.Last()))
                    {
                        lines.Add(""); // 섹션 구분 공백
                    }
                    lines.Add($"[{section}]");
                    lines.Add($"{key} = {value}");
                }
                else
                {
                    // 섹션 내 키값 업데이트
                    int endIndex = lines.FindIndex(sectionIndex + 1, l => l.StartsWith("[") || string.IsNullOrWhiteSpace(l));
                    if (endIndex == -1) endIndex = lines.Count;

                    bool keyFound = false;
                    for (int i = sectionIndex + 1; i < endIndex; i++)
                    {
                        if (lines[i].StartsWith($"{key} ="))
                        {
                            lines[i] = $"{key} = {value}";
                            keyFound = true;
                            break;
                        }
                    }

                    if (!keyFound)
                    {
                        lines.Insert(endIndex, $"{key} = {value}");
                    }
                }

                File.WriteAllLines(settingsFilePath, lines);
            }
        }


        /// <summary>
        /// 섹션 전체를 삭제합니다.
        /// </summary>
        public void RemoveSection(string section)
        {
            var lines = File.Exists(settingsFilePath) ? File.ReadAllLines(settingsFilePath).ToList() : new List<string>();
            int sectionIndex = lines.FindIndex(l => l.Trim() == $"[{section}]");

            if (sectionIndex != -1)
            {
                int endIndex = lines.FindIndex(sectionIndex + 1, l => l.StartsWith("[") || string.IsNullOrWhiteSpace(l));
                if (endIndex == -1) endIndex = lines.Count;

                lines.RemoveRange(sectionIndex, endIndex - sectionIndex);
                File.WriteAllLines(settingsFilePath, lines);
            }
        }

        public List<string> GetRegexFolders()
        {
            var folders = new List<string>();
            if (!File.Exists(settingsFilePath))
                return folders;

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
                        folders.Add(parts[1].Trim());
                }
            }
            return folders;
        }

        public void NotifyRegexSettingsUpdated()
        {
            // 변경 알림 이벤트 호출
            RegexSettingsUpdated?.Invoke();

            // 변경된 내용을 강제로 다시 로드
            ReloadSettings();
        }

        private void ReloadSettings()
        {
            // 현재 설정 파일 다시 읽기
            if (File.Exists(settingsFilePath))
            {
                var lines = File.ReadAllLines(settingsFilePath);
                // 내부 데이터 구조 갱신
            }
        }
        
        public string GetBaseFolder()
        {
          var baseFolders = GetFoldersFromSection("[BaseFolder]");
          if (baseFolders.Count > 0)
          {
            return baseFolders[0];  // 첫 번째 BaseFolder 반환
          }
          
          return null; // BaseFolder가 없는 경우 null 반환
        }
    }
}
