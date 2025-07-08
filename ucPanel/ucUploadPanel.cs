// ucPanel\ucUploadPanel.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Threading.Tasks;
using ITM_Agent.Plugins;
using ITM_Agent.Services;

namespace ITM_Agent.ucPanel
{
    public partial class ucUploadPanel : UserControl
    {
        // 외부에서 주입받는 참조
        private ucConfigurationPanel configPanel;
        private ucPluginPanel pluginPanel;
        private SettingsManager settingsManager;
        private LogManager logManager;

        // 업로드 대상 폴더 감시용 FileSystemWatcher
        private FileSystemWatcher uploadFolderWatcher;

        private const string UploadSection = "UploadSetting";  // INI 섹션명
        private const string UploadKey = "DBItem";
        private const string KeyFolder = "WaferFlatFolder";  // 폴더 키
        private const string KeyPlugin = "FilePlugin";  // 플러그인 키
        
        public ucUploadPanel(ucConfigurationPanel configPanel, ucPluginPanel pluginPanel, SettingsManager settingsManager)
        {
            InitializeComponent();
            this.configPanel = configPanel;
            this.pluginPanel = pluginPanel;
            this.pluginPanel.PluginsChanged += PluginPanel_PluginsChanged;  // 구독
            this.settingsManager = settingsManager;
            logManager = new LogManager(AppDomain.CurrentDomain.BaseDirectory);

            // 반드시 이벤트 먼저 연결
            btn_FlatSet.Click += btn_FlatSet_Click;
            
            LoadTargetFolderItems();
            LoadPluginItems();
            
            LoadWaferFlatSettings();  // 방금 만든 복원 로직
            
            LoadUploadSettings();
        }
        
        private void LoadUploadSettings()
        {
            string iniLine = settingsManager.GetValueFromSection(UploadSection, UploadKey);
            if (string.IsNullOrWhiteSpace(iniLine)) return;
        
            // "WaferFlat, Folder : D:\ITM_Agent\wf, Plugin : Onto_WaferFlatData"
            string[] parts = iniLine.Split(',');
        
            if (parts.Length < 3) return;
        
            // Folder : 뒤의 **전체 문자열**을 잘라 온다
            string folderToken = parts[1];                       // " Folder : D:\ITM_Agent\wf"
            int    idxFolder   = folderToken.IndexOf(':');
            if (idxFolder <= 0) return;
            string folderPath  = folderToken.Substring(idxFolder + 1).Trim(); // "D:\ITM_Agent\wf"
        
            // Plugin : 뒤 문자열
            string pluginToken = parts[2];                       // " Plugin : Onto_WaferFlatData"
            int    idxPlugin   = pluginToken.IndexOf(':');
            if (idxPlugin <= 0) return;
            string pluginName  = pluginToken.Substring(idxPlugin + 1).Trim(); // "Onto_WaferFlatData"
        
            // 콤보박스 반영
            if (!cb_WaferFlat_Path.Items.Contains(folderPath))
                cb_WaferFlat_Path.Items.Add(folderPath);
            cb_WaferFlat_Path.Text = folderPath;
        
            if (!cb_FlatPlugin.Items.Contains(pluginName))
                cb_FlatPlugin.Items.Add(pluginName);
            cb_FlatPlugin.Text = pluginName;
        
            // 감시 재개
            StartUploadFolderWatcher(folderPath);
        }

        // ucConfigurationPanel에서 대상 폴더 목록을 가져와 콤보박스에 로드
        private void LoadTargetFolderItems()
        {
            cb_WaferFlat_Path.Items.Clear();
            if (configPanel != null)
            {
                var folders = configPanel.GetTargetFolders();
                cb_WaferFlat_Path.Items.AddRange(folders);
            }
            else
            {
                var folders = settingsManager.GetFoldersFromSection("[TargetFolders]");
                cb_WaferFlat_Path.Items.AddRange(folders.ToArray());
            }
        }
        
        // ucPluginPanel에서 로드한 플러그인(PluginListItem) 목록을 콤보박스에 로드
        private void LoadPluginItems()
        {
            cb_FlatPlugin.Items.Clear();
            if (pluginPanel != null)
            {
                var plugins = pluginPanel.GetLoadedPlugins(); // PluginListItem 목록
                foreach (var plugin in plugins)
                {
                  cb_FlatPlugin.Items.Add(plugin.PluginName);
                }
            }
        }
        
        // 선택된 업로드 폴더에서 파일 변화 감시 시작
        private void StartUploadFolderWatcher(string folderPath)
        {
            try
            {
                // ① 경로 문자열 정리(개행·앞뒤 공백 제거)
                folderPath = folderPath.Trim();
        
                // ② 유효성 검사
                if (string.IsNullOrEmpty(folderPath))
                    throw new ArgumentException("폴더 경로가 비어 있습니다.", nameof(folderPath));
        
                // ③ 폴더가 없으면 생성 (권한 없으면 예외 발생)
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    logManager.LogEvent($"[UploadPanel] 폴더가 없어 새로 생성: {folderPath}");
                }
        
                // ④ 이전 Watcher 해제
                uploadFolderWatcher?.Dispose();
        
                // ⑤ 새로운 Watcher 생성
                uploadFolderWatcher = new FileSystemWatcher(folderPath)
                {
                    Filter               = "*.*",
                    IncludeSubdirectories = false,
                    NotifyFilter          = NotifyFilters.FileName
                                            | NotifyFilters.Size
                                            | NotifyFilters.LastWrite,
                    EnableRaisingEvents   = true
                };
                uploadFolderWatcher.Created += UploadFolderWatcher_Created;
        
                logManager.LogEvent($"[UploadPanel] 폴더 감시 시작: {folderPath}");
            }
            catch (Exception ex)
            {
                // 실패 시 로그 + 사용자 알림
                logManager.LogError($"[UploadPanel] 폴더 감시 시작 실패: {ex.Message}");
                MessageBox.Show("폴더 감시를 시작할 수 없습니다.\n" + ex.Message,
                                "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // FileSystemWatcher 이벤트 처리 - 파일 변화 감지 시 지정 플러그인 호출 (리플렉션 사용)
        private void UploadFolderWatcher_Event(object sender, FileSystemEventArgs e)
        {
            try
            {
                /* 0) 파일 잠금 해제 대기 (200 ms) */
                System.Threading.Thread.Sleep(200);
                
                /* 1) 현재 UI에서 선택된 플러그인 이름을 가져옴 */
                string pluginName = cb_FlatPlugin.Text.Trim();
                if (string.IsNullOrEmpty(pluginName))
                {
                    logManager.LogError("[UploadPanel] 플러그인을 선택하지 않았습니다.");
                    return;
                }
                
                /* 2) pluginPanel 에 로드된 DLL 목록에서 매칭 */
                var pluginItem = pluginPanel
                    .GetLoadedPlugins()
                    .FirstOrDefault(p => p.PluginName
                                          .Equals(pluginName,
                                                  StringComparison.OrdinalIgnoreCase));
                
                if (pluginItem == null)
                {
                    logManager.LogError($"[UploadPanel] '{pluginName}' 플러그인을 로드하지 못했습니다.");
                    return;
                }
                
                /* 3) DLL 로드 & IOnto_WaferFlatData 구현 타입 검색 */
                Assembly asm = Assembly.LoadFrom(pluginItem.AssemblyPath);
                
                // 네임스페이스를 모른다면 Reflection 문자열 검색 방식 사용
                Type targetType = asm.GetTypes()
                    .FirstOrDefault(t => t.GetInterface("Onto_WaferFlatDataLib.IOnto_WaferFlatData") != null
                                      && !t.IsInterface && !t.IsAbstract);
                
                if (targetType == null)
                {
                    logManager.LogError($"[UploadPanel] IOnto_WaferFlatData 구현을 '{pluginName}' 에서 찾지 못했습니다.");
                    return;
                }
                
                /* 4) 인스턴스 생성 & 전처리+업로드 실행 */
                object processor = Activator.CreateInstance(targetType);
                
                // MethodInfo 미리 가져오기(c# 7.3 호환)
                MethodInfo mi = targetType.GetMethod("ProcessAndUpload",
                                                      new[] { typeof(string) });
                if (mi == null)
                {
                    logManager.LogError($"[UploadPanel] ProcessAndUpload 메서드를 '{pluginName}' 에서 찾지 못했습니다.");
                    return;
                }
                
                string watchFolder = cb_WaferFlat_Path.Text.Trim();
                logManager.LogEvent($"[UploadPanel] 플러그인 실행 시작 > {pluginName}");
                mi.Invoke(processor, new object[] { watchFolder });
                logManager.LogEvent($"[UploadPanel] 플러그인 실행 완료 > {pluginName}");
            }
            catch (Exception ex)
            {
                logManager.LogError("[UploadPanel] UploadFolderWatcher_Event 예외 : " + ex.Message);
            }
        }
        
        // 파일 접근 준비 여부 확인 헬퍼 메서드
        private bool IsFileReady(string filePath)
        {
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        
        // btn_FlatSet 클릭 시: 선택된 폴더와 플러그인 정보를 Settings.ini에 저장하고 감시 시작
        private void btn_FlatSet_Click(object sender, EventArgs e)
        {
            // (1) 콤보박스 텍스트 취득
            string folderPath = cb_WaferFlat_Path.Text.Trim();
            string pluginName = cb_FlatPlugin.Text.Trim();
        
            // (2) 필수값 검사
            if (string.IsNullOrEmpty(folderPath) || string.IsNullOrEmpty(pluginName))
            {
                MessageBox.Show("Wafer-Flat 폴더와 플러그인을 모두 선택(입력)하세요.",
                                "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        
            // (3) INI 한 줄 포맷
            string iniValue = $"WaferFlat, Folder : {folderPath}, Plugin : {pluginName}";
        
            // ▼▼▼ 기존 코드 : "Line" 키 사용  (오류 원인) ▼▼▼
            // settingsManager.SetValueToSection(UploadSection, "Line", iniValue);
        
            // ===== 개선 코드 : "DBItem" 키 사용 =====
            settingsManager.SetValueToSection(UploadSection, "DBItem", iniValue);
        
            // (4) 콤보박스 목록 관리
            if (!cb_WaferFlat_Path.Items.Contains(folderPath))
                cb_WaferFlat_Path.Items.Add(folderPath);
            if (!cb_FlatPlugin.Items.Contains(pluginName))
                cb_FlatPlugin.Items.Add(pluginName);
        
            // (5) 로그 & 안내
            logManager.LogEvent($"[ucUploadPanel] 저장 ➜ {iniValue}");
            MessageBox.Show("설정이 저장되었습니다.", "완료",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
        
            // (6) 폴더 감시 시작
            StartUploadFolderWatcher(folderPath);
        }

        /// <summary>
        /// [WaferFlat] 섹션에 폴더/플러그인 정보를 저장
        /// </summary>
        private void SaveWaferFlatSettings()
        {
            string folderPath = cb_WaferFlat_Path.Text.Trim();
            string pluginName = cb_FlatPlugin.Text.Trim();
        
            string iniValue = $"WaferFlat, Folder : {folderPath}, Plugin : {pluginName}";
            settingsManager.SetValueToSection(UploadSection, UploadKey, iniValue);
        
            logManager.LogEvent($"[ucUploadPanel] 저장 ➜ {iniValue}");
        }
        
        /// <summary>
        /// Settings.ini 의 [UploadSetting] 섹션에서
        /// Wafer-Flat 폴더·플러그인 정보를 읽어 콤보박스에 반영
        /// </summary>
        private void LoadWaferFlatSettings()
        {
            string valueLine = settingsManager.GetValueFromSection("UploadSetting", "Line1");
            if (string.IsNullOrWhiteSpace(valueLine)) return;
        
            // 예: "WaferFlat, Folder : D:\ITM_Agent\wf, Plugin : Onto_WaferFlatData"
            string[] parts = valueLine.Split(',');
            if (parts.Length < 3) return;
        
            // Folder 부분 추출
            string folderToken = parts[1];                       // " Folder : D:\ITM_Agent\wf"
            int idxFolder      = folderToken.IndexOf(':');
            if (idxFolder <= 0) return;
            string folderPath  = folderToken.Substring(idxFolder + 1).Trim();
        
            // Plugin 부분 추출
            string pluginToken = parts[2];                       // " Plugin : Onto_WaferFlatData"
            int idxPlugin      = pluginToken.IndexOf(':');
            if (idxPlugin <= 0) return;
            string pluginName  = pluginToken.Substring(idxPlugin + 1).Trim();
        
            // (1) 폴더 콤보박스
            if (!cb_WaferFlat_Path.Items.Contains(folderPath))
                cb_WaferFlat_Path.Items.Add(folderPath);
            cb_WaferFlat_Path.SelectedItem = folderPath;
        
            // (2) 플러그인 콤보박스
            if (cb_FlatPlugin.Items.Contains(pluginName))
                cb_FlatPlugin.SelectedItem = pluginName;
        }

        /// <summary>
        /// FileSystemWatcher 가 감시 폴더에서 새 파일을 감지했을 때 호출됩니다.
        /// </summary>
        private void UploadFolderWatcher_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                // (1) 전체 경로 얻기
                string filePath = e.FullPath;

                // (2) 로그 기록
                logManager.LogEvent("[UploadPanel] 새 파일 감지 : " + filePath);

                // (3) 워크로드가 무거울 수 있으므로 비동기로 처리
                Task.Run(() => ProcessWaferFlatFile(filePath));
            }
            catch (Exception ex)
            {
                logManager.LogError("[UploadPanel] 파일 처리 실패 : " + ex.Message);
            }
        }
        
        /// <summary>
        /// 플러그인을 이용해 Wafer-Flat 데이터 파일을 처리합니다.
        /// 실제 로직은 프로젝트 상황에 맞게 구현하세요.
        /// </summary>
        private void ProcessWaferFlatFile(string filePath)
        {
            // TODO: 플러그인 호출 및 DB 업로드 로직
            // ✔️ 여기서는 예시로 2초 Sleep 후 완료 로그만 남깁니다.
            System.Threading.Thread.Sleep(2000);
            logManager.LogEvent("[UploadPanel] 파일 처리 완료 : " + filePath);
        }
        
        private void PluginPanel_PluginsChanged(object sender, EventArgs e)
        {
            RefreshPluginCombo();
        }
        
        private void RefreshPluginCombo()
        {
            // (1) 현재 선택 상태 보존
            string prevSelection = cb_FlatPlugin.SelectedItem as string;
        
            cb_FlatPlugin.BeginUpdate();
            cb_FlatPlugin.Items.Clear();
        
            // (2) 플러그인 이름 다시 채우기
            foreach (var p in pluginPanel.GetLoadedPlugins())
                cb_FlatPlugin.Items.Add(p.PluginName);
        
            // ▼▼ 기존 코드 : 새 목록의 “마지막 항목”을 강제로 선택 -------------------
            //if (cb_FlatPlugin.Items.Count > 0)
            //    cb_FlatPlugin.SelectedIndex = cb_FlatPlugin.Items.Count - 1;
            // ▲▲ 삭제(주석처리) -----------------------------------------------------
        
            // (3) 이전에 선택돼 있던 값이 아직도 존재하면 그대로 유지
            if (!string.IsNullOrEmpty(prevSelection) &&
                cb_FlatPlugin.Items.Contains(prevSelection))
            {
                cb_FlatPlugin.SelectedItem = prevSelection;
            }
            else
            {
                // 아니면 아무 것도 선택하지 않음
                cb_FlatPlugin.SelectedIndex = -1;
            }
        
            cb_FlatPlugin.EndUpdate();
        }
    }
}
