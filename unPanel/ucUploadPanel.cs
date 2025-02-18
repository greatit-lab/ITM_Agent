using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ITM_Agent.Plugins;

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
        }

        // ucConfigurationPanel에서 lb_RegexList에 있는 target 폴더 목록을 가져와 콤보박스에 로드
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
                // fallback: SettingsManager에서 [TargetFolders] 섹션 읽기
                var folders = settingsManager.GetFoldersFromSection("[TargetFolders]");
                cb_WaferFlat_Path.Items.AddRange(folders.ToArray());
            }
        }

        // ucPluginPanel에서 로드한 플러그인(IWaferFlatDataUploader) 목록을 콤보박스에 로드
        private void LoadPluginItems()
        {
            cb_FlatPlugin.Items.Clear();
            if (pluginPanel != null)
            {
                // pluginPanel.GetLoadedPlugins()가 IWaferFlatDataUploader[]를 반환한다고 가정
                var plugins = pluginPanel.GetLoadedPlugins();
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
            uploadFolderWatcher.Changed += UploadFolderWatcher_Event;
            logManager.LogEvent($"[ucUploadPanel] Started watching folder: {folderPath}");
        }

        // FileSystemWatcher 이벤트 처리 – 파일 변화 감지 시 지정 플러그인 호출
        private void UploadFolderWatcher_Event(object sender, FileSystemEventArgs e)
        {
            // 파일 접근이 준비되었는지 확인
            if (!IsFileReady(e.FullPath))
                return;
            
            // UI 스레드에서 실행
            this.Invoke(new Action(() =>
            {
                // Settings.ini "[UploadSettings]" 섹션에 저장된 플러그인명 가져오기
                string savedPlugin = settingsManager.GetValueFromSection("UploadSettings", "FlatPlugin");
                // pluginPanel에서 로드한 IWaferFlatDataUploader 목록 중 일치하는 플러그인 검색
                IWaferFlatDataUploader plugin = pluginPanel.GetLoadedPlugins()
                    .FirstOrDefault(p => p.PluginName == savedPlugin);
                if (plugin != null)
                {
                    // 파일 변화가 발생한 폴더(또는 상위 폴더)를 인자로 전달하여 업로드 처리
                    plugin.ProcessAndUpload(Path.GetDirectoryName(e.FullPath));
                }
                else
                {
                    logManager.LogError("[ucUploadPanel] Selected plugin not found.");
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
