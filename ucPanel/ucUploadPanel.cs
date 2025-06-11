// ucPanel\ucUploadPanel.cs
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
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

        public ucUploadPanel(ucConfigurationPanel configPanel, ucPluginPanel pluginPanel, SettingsManager settingsManager)
        {
            InitializeComponent();
            this.configPanel = configPanel;
            this.pluginPanel = pluginPanel;
            this.settingsManager = settingsManager;
            logManager = new LogManager(AppDomain.CurrentDomain.BaseDirectory);

            LoadTargetFolderItems();
            LoadPluginItems();
            LoadUploadSettings();
        }
        
        private void LoadUploadSettings()
        {
            string savedFolder = settingsManager.GetValueFromSection("Upload", "WaferFlatFolder");
            if (!string.IsNullOrEmpty(savedFolder) && Directory.Exists(savedFolder))
            {
                cb_WaferFlat_Path.Text = savedFolder;
                StartUploadFolderWatcher(savedFolder);
            }
            else
            {
                // 설정이 없거나 폴더가 존재하지 않으면 기본값 처리(필요시)
                cb_WaferFlat_Path.Text = "";
            }
        }
        
        /// ucConfigurationPanel에서 대상 폴더 목록을 가져와 콤보박스에 로드
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
            if (uploadFolderWatcher != null)
            {
                uploadFolderWatcher.EnableRaisingEvents = false;
                uploadFolderWatcher.Dispose();
            }
            uploadFolderWatcher = new FileSystemWatcher(folderPath)
            {
                Filter = "*.*",
                IncludeSubdirectories = false,
                EnableRaisingEvents = true
            };
            uploadFolderWatcher.Created += UploadFolderWatcher_Event;
            UploadFolderWatcher.Changed += UploadFolderWatcher_Event;
            logManager.LogEvent($"[ucUploadPanel] Started watching folder: {folderPath}");
        }
        
        // FileSystemWatcher 이벤트 처리 - 파일 변화 감지 시 지정 플러그인 호출 (리플렉션 사용)
        private void UploadFolderWatcher_Event(object sender, FileSystemEventArgs e)
        {
            // 파일 접근이 준비되었는지 확인
            if (!IsFileReady(e.FullPath))
                return;
            
            // UI 스레드에서 실행
            this.Invoke(new Action(() =>
            {
                // Settings.ini "[UploadSettings]" 섹션에 저장된 플러그인명을 가져옴
                string savedPlugin = settingsManager.GetValueFromSection("UploadSettings", "FilePlugin");
                if (string.IsNullOrEmpty(savedPlugin))
                {
                    logManager.LogError("[ucUploadPanel] 설정에서 플러그인을 찾을 수 없습니다.");
                    return;
                }
                
                // pluginPanel에서 로드한 PluginListItem 목록 중 일치하는 항목 검색
                var pluginItem = pluginPanel.GetLoadedPlugins().FirstOnDefault(p => p.PluginName.Equals(savedPlugin, StringComparision.OrdinalIgnoreCase));
                
                if (pluginItem != null)
                {
                    try
                    {
                        // DLL 경로를 이용해 어셈블리 로드
                        Assembly asm = Assembly.LoadFrom(pluginItem.AssemblyPath);
                        // 외부 DLL 내에 "PluginUploader"라는 타입이 존잰한다고 가정
                        Type uploaderType = asm.GetType("PluginUploader");
                        if (uploaderType == null)
                        {
                            logManager.LogError("[ucUploadPanel] PluginUploader 타입을 찾을 수 없습니다.");
                            return;
                        }
                        // PluginUploader 인스턴스 생성
                        object uploaderInstance = Activator.CreateInstance(uploaderType);
                        // "PorcessAndUpload(string)" 메서드 검색
                        MethodInfo mi = uploaderType.GetMethod("ProcessAndUpload", new Type[] { typeof(string) });
                        if (mi == null)
                        {
                            logManager.LogError("[ucUploadPanel] ProcessAndUpload(string) 메서드가 구현되어 있지 않습니다.");
                            return;
                        }
                        // 메서드 호출 - 인자로 파일 변화가 발생한 폴더 경로 전달
                        mi.Invoke(uploaderInstance, new object[] { Path.GetDirectoryName(e.FullPath) });
                    }
                    catch (Exception ex)
                    {
                        logManager.LogError("[ucUploadPanel] 플러그인 호출 중 오류: " + ex.Massage);
                    }
                }
                else
                {
                    logManager.LogError("[ucUploadPanel] 선택한 플러그인 '{savedPlugin}'을(를) 찾을 수 없습니다.");
                }
            }));
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
            if (cb_WaferFlat_Path.SelectedItem == null || cb_FlatPlugin.SelectedItem == null)
            {
                MessageBox.Show("Please select both a folder and a plugin.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string selectedFolder = cb_WaferFlat_Path.SelectedItem.ToString();
            string selectedPlugin = cb_FlatPlugin.SelectedItem.ToString();

            settingsManager.SetValueToSection("UploadSettings", "WaferFlatFolder", selectedFolder);
            settingsManager.SetValueToSection("UploadSettings", "FlatPlugin", selectedPlugin);
            MessageBox.Show("Upload settings saved.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            StartUploadFolderWatcher(selectedFolder);
        }
    }
}
