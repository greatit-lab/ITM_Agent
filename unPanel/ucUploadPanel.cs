// ucPanel\ucUploadPanel.cs
using System;
using System.Collections.Generic;
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
            LoadLibraryLinkSettings();  // 이 시점에 콤보박스에 설정값이 반영되어야 합니다.
        }
        
        public void StartMonitoring()
        {
            if (cb_WaferFlat_Path.SelectedItem == null || string.IsNullOrEmpty(cb_WaferFlat_Path.Text))
            {
                logManager.LogError("[ucUploadPanel] 대상 폴더가 선택되어 있지 않습니다.");
                return;
            }
            string folder = cb_WaferFlat_Path.SelectedItem.ToString();
            StartUploadFolderWatcher(folder);
            logManager.LogEvent("[ucUploadPanel] Monitoring started for folder: " + folder);
        }
        
        public void StopMonitoring()
        {
            if (uploadFolderWatcher != null)
            {
                uploadFolderWatcher.EnableRaisingEvents = false;
                uploadFolderWatcher.Dispose();
                uploadFolderWatcher = null;
                logManager.LogEvent("[ucUploadPanel] Monitoring stopped.");
            }
        }
        
        // 플러그인 목록 변경 이벤트 핸들러
        private void PluginPanel_PluginListUpdated(object sender, EventArgs e)
        {
            // Settings.ini 파일의 [RegPlugins] 섹션도 이미 업데이트되었으므로
            // LoadPluginItems()를 호출하여 cb_FlatPlugin 콤보박스 항목을 새로 고칩니다.
            LoadPluginItems();
            logManager.LogEvent("[ucUploadPanel] 플러그인 목록 갱신됨.");
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
            regPlugins.Clear();
            var pluginLines = settingsManager.GetFoldersFromSection("RegPlugins");
            if (pluginLines != null)
            {
                foreach (var line in pluginLines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    string[] parts = line.Split(new char[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        string pluginName = parts[0].Trim();
                        string dllPath = parts[1].Trim();
                        regPlugins[pluginName] = dllPath;
                        if (!cb_FlatPlugin.Items.Contains(pluginName))
                        {
                            cb_FlatPlugin.Items.Add(pluginName);
                        }
                    }
                }
            }
        }
        
        private void SetComboBoxSelected(ComboBox combo, string savedValue)
        {
            if (!string.IsNullOrEmpty(savedValue))
            {
                int index = combo.Items.IndexOf(savedValue);
                if (index < 0)
                {
                    combo.Items.Add(savedValue);
                    index = combo.Items.IndexOf(savedValue);
                }
                combo.SelectedIndex = index;
            }
        }
        
        private void LoadLibraryLinkSettings()
        {
            var lines = settingsManager.GetFoldersFromSection("LibraryLink");
            if (lines != null && lines.Count > 0)
            {
                string line = lines[0];
                int eqIndex = line.IndexOf('=');
                if (eqIndex >= 0)
                {
                    string rightPart = line.Substring(eqIndex + 1).Trim();
                    string[] tokens = rightPart.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length >= 2)
                    {
                        string folderValue = tokens[0].Trim();
                        string pluginValue = tokens[1].Trim();

                        // 폴더 콤보박스 처리
                        if (!string.IsNullOrEmpty(folderValue))
                        {
                            if (cb_WaferFlat_Path.Items.IndexOf(folderValue) < 0)
                                cb_WaferFlat_Path.Items.Add(folderValue);
                            int idx = cb_WaferFlat_Path.Items.IndexOf(folderValue);
                            if (idx >= 0)
                            {
                                cb_WaferFlat_Path.SelectedIndex = idx;
                                cb_WaferFlat_Path.Text = folderValue;
                            }
                        }

                        // 플러그인 콤보박스 처리
                        if (!string.IsNullOrEmpty(pluginValue))
                        {
                            if (cb_FlatPlugin.Items.IndexOf(pluginValue) < 0)
                                cb_FlatPlugin.Items.Add(pluginValue);
                            int idx = cb_FlatPlugin.Items.IndexOf(pluginValue);
                            if (idx >= 0)
                            {
                                cb_FlatPlugin.SelectedIndex = idx;
                                cb_FlatPlugin.Text = pluginValue;
                            }
                        }
                    }
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
                // Settings.ini의 [LibraryLink] 섹션에서 플러그인명을 읽어옵니다.
                string savedPlugin = settingsManager.GetValueFromSection("LibraryLink", "WaferFlatplugin");
                if (string.IsNullOrEmpty(savedPlugin))
                {
                    logManager.LogError("[ucUploadPanel] LibraryLink 섹션에 플러그인 정보가 설정되어 있지 않습니다.");
                    return;
                }
                if (!regPlugins.TryGetValue(savedPlugin, out string dllPath) || !File.Exists(dllPath))
                {
                    logManager.LogError($"[ucUploadPanel] 선택한 플러그인 '{savedPlugin}'의 DLL 경로를 찾을 수 없습니다.");
                    return;
                }
                try
                {
                    Assembly asm = Assembly.LoadFrom(dllPath);
                    Type uploaderType = asm.GetType("PluginUploader");
                    if (uploaderType == null)
                    {
                        logManager.LogError("[ucUploadPanel] PluginUploader 타입을 찾을 수 없습니다.");
                        return;
                    }
                    object uploaderInstance = Activator.CreateInstance(uploaderType);
                    MethodInfo mi = uploaderType.GetMethod("ProcessAndUpload", new Type[] { typeof(string) });
                    if (mi == null)
                    {
                        logManager.LogError("[ucUploadPanel] ProcessAndUpload(string) 메서드가 구현되어 있지 않습니다.");
                        return;
                    }
                    // 감지된 파일의 폴더 경로를 인자로 전달합니다.
                    mi.Invoke(uploaderInstance, new object[] { Path.GetDirectoryName(e.FullPath) });
                    logManager.LogEvent("[ucUploadPanel] ProcessAndUpload 메서드 호출 완료.");
                }
                catch (Exception ex)
                {
                    logManager.LogError("[ucUploadPanel] 플러그인 호출 중 오류: " + ex.Message);
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
            if (cb_WaferFlat_Path.SelectedItem == null || string.IsNullOrEmpty(cb_WaferFlat_Path.Text) ||
                cb_FlatPlugin.SelectedItem == null || string.IsNullOrEmpty(cb_FlatPlugin.Text))
            {
                MessageBox.Show("Please select both a folder and a plugin.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string selectedFolder = cb_WaferFlat_Path.SelectedItem.ToString();
            string selectedPlugin = cb_FlatPlugin.SelectedItem.ToString();

            // Settings.ini의 [LibraryLink] 섹션에 "WaferFlat,  WaferFlatplugin = {폴더}, {플러그인}" 형식으로 기록합니다.
            settingsManager.SetLibraryLink(selectedFolder, selectedPlugin);

            MessageBox.Show("Library Link settings saved.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // 모니터링 시작은 MainForm의 btn_Run 이벤트에서 StartMonitoring()을 호출하여 진행합니다.
        }
    }
}
