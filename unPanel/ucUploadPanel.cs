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

        // 플러그인 정보(PluginListItem)는 ucPluginPanel에서 관리하므로 별도 로컬 리스트는 사용하지 않음

        public ucUploadPanel(ucConfigurationPanel configPanel, ucPluginPanel pluginPanel, SettingsManager settingsManager)
        {
            InitializeComponent();
            this.configPanel = configPanel;
            this.pluginPanel = pluginPanel;
            this.settingsManager = settingsManager;
            logManager = new LogManager(AppDomain.CurrentDomain.BaseDirectory);

            // 대상 폴더와 플러그인 목록을 각각 로드
            LoadTargetFolderItems();
            LoadPluginItems();
            LoadUploadSettings();
        }
        
        /// <summary>
        /// settings.ini의 [Upload] 섹션에 저장된 업로드 폴더를 로드하고,
        /// 해당 폴더에서 FileSystemWatcher를 시작합니다.
        /// </summary>
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
                cb_WaferFlat_Path.Text = "";
            }
        }
        
        /// <summary>
        /// ucConfigurationPanel 또는 settings.ini의 [TargetFolders] 섹션에서 대상 폴더 목록을 가져와 콤보박스에 로드합니다.
        /// </summary>
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
        
        /// <summary>
        /// ucPluginPanel에서 로드한 플러그인 목록(PluginListItem)을 콤보박스에 로드합니다.
        /// </summary>
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
        
        /// <summary>
        /// 지정된 폴더에서 FileSystemWatcher를 시작합니다.
        /// </summary>
        /// <param name="folderPath"></param>
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
            uploadFolderWatcher.Changed += UploadFolderWatcher_Event;
            logManager.LogEvent($"[ucUploadPanel] Started watching folder: {folderPath}");
        }
        
        /// <summary>
        /// FileSystemWatcher 이벤트 처리 – 파일이 생성되거나 변경되면,
        /// settings.ini의 [UploadSettings] 섹션에 저장된 플러그인명을 기준으로,
        /// ucPluginPanel에서 로드한 플러그인 DLL을 리플렉션을 통해 불러와
        /// 해당 DLL의 PluginUploader 타입의 ProcessAndUpload(string) 메서드를 호출합니다.
        /// </summary>
        private void UploadFolderWatcher_Event(object sender, FileSystemEventArgs e)
        {
            if (!IsFileReady(e.FullPath))
                return;
            
            this.Invoke(new Action(() =>
            {
                // settings.ini의 [UploadSettings] 섹션에 저장된 플러그인명을 읽음
                string savedPlugin = settingsManager.GetValueFromSection("UploadSettings", "FlatPlugin");
                // pluginPanel에서 해당 플러그인 정보를 검색
                var pluginItem = pluginPanel.GetLoadedPlugins()
                                        .FirstOrDefault(p => p.PluginName.Equals(savedPlugin, StringComparison.OrdinalIgnoreCase));
                if (pluginItem != null)
                {
                    try 
                    {
                        // 복사된 DLL 파일(예: Library 폴더 내 경로)을 이용하여 어셈블리 로드
                        Assembly asm = Assembly.LoadFrom(pluginItem.AssemblyPath);
                        // 외부 DLL 내 "PluginUploader" 타입 검색 (타입 이름은 약속된 이름으로 가정)
                        Type uploaderType = asm.GetType("PluginUploader");
                        if (uploaderType == null)
                        {
                            logManager.LogError("[ucUploadPanel] PluginUploader 타입을 찾을 수 없습니다.");
                            return;
                        }
                        // PluginUploader 인스턴스 생성
                        object uploaderInstance = Activator.CreateInstance(uploaderType);
                        // "ProcessAndUpload(string)" 메서드 검색
                        MethodInfo mi = uploaderType.GetMethod("ProcessAndUpload", new Type[] { typeof(string) });
                        if (mi == null)
                        {
                            logManager.LogError("[ucUploadPanel] ProcessAndUpload(string) 메서드가 구현되어 있지 않습니다.");
                            return;
                        }
                        // 메서드 호출 – 파일 변화가 발생한 폴더 경로 전달
                        mi.Invoke(uploaderInstance, new object[] { Path.GetDirectoryName(e.FullPath) });
                    }
                    catch (Exception ex)
                    {
                        logManager.LogError("[ucUploadPanel] 플러그인 호출 중 오류: " + ex.Message);
                    }
                }
                else
                {
                    logManager.LogError("[ucUploadPanel] 선택한 플러그인을 찾을 수 없습니다.");
                }
            }));
        }
        
        /// <summary>
        /// 파일이 열릴 수 있는지 여부를 확인합니다.
        /// </summary>
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
        
        /// <summary>
        /// btn_FlatSet 버튼 클릭 시: 선택된 업로드 폴더와 플러그인 정보를 settings.ini의 [UploadSettings] 섹션에 저장하고,
        /// 해당 폴더 감시(FileSystemWatcher)를 시작합니다.
        /// </summary>
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
