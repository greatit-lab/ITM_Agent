// ucPanel\ucPluginPanel.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using ITM_Agent.Plugins;
using ITM_Agent.Services;

namespace ITM_Agent.ucPanel
{
    public partial class ucPluginPanel : UserControl
    {
        // 플러그인 정보를 보관하는 리스트 (PluginListItem은 플러그인명과 DLL 경로 정보를 저장)
        private List<PluginListItem> loadedPlugins = new List<PluginListItem>();
        
        // 플러그인 목록 변경 이벤트 선언
        public event EventHandler PluginListUpdated;
        
        private SettingsManager settingsManager;
        private LogManager logManager;

        public ucPluginPanel(SettingsManager settings)
        {
            InitializeComponent();
            settingsManager = settings;
            logManager = new LogManager(AppDomain.CurrentDomain.BaseDirectory);

            // settings.ini의 [RegPlugins] 섹션에서 기존에 등록된 플러그인 정보를 불러옴
            LoadPluginsFromSettings();
        }

        /// <summary>
        /// btn_PlugAdd 클릭 이벤트 핸들러  
        /// OpenFileDialog를 열어 DLL 파일을 선택한 후, 파일을 바이트 배열로 로드하여 중복 검사하고,  
        /// 실행 경로 아래 "Library" 폴더에 DLL 파일을 복사하며, settings.ini의 [RegPlugins] 섹션에 기록합니다.
        /// </summary>
        // 예시: 플러그인 추가 버튼 클릭 이벤트
        private void btn_PlugAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "DLL Files (*.dll)|*.dll|All Files (*.*)|*.*",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedDllPath = openFileDialog.FileName;
                try
                {
                    // 파일을 바이트 배열로 읽어서 메모리로 로드 (파일 잠금 방지)
                    byte[] dllData = File.ReadAllBytes(selectedDllPath);
                    var asm = System.Reflection.Assembly.Load(dllData);
                    string pluginName = asm.GetName().Name;

                    // 이미 등록된 플러그인 검사
                    if (loadedPlugins.Any(p => p.PluginName.Equals(pluginName, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("이미 등록된 플러그인입니다.", "중복", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // 실행 경로 아래 "Library" 폴더가 없으면 생성
                    string libraryFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Library");
                    if (!Directory.Exists(libraryFolder))
                    {
                        Directory.CreateDirectory(libraryFolder);
                    }

                    // 선택한 DLL 파일을 Library 폴더로 복사 (동일 이름의 파일이 있는지 검사)
                    string destDllPath = Path.Combine(libraryFolder, Path.GetFileName(selectedDllPath));
                    if (File.Exists(destDllPath))
                    {
                        MessageBox.Show("이미 동일한 DLL 파일이 Library 폴더에 존재합니다.", "중복", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    File.Copy(selectedDllPath, destDllPath);

                    // PluginListItem 객체 생성 및 목록에 추가
                    PluginListItem pluginItem = new PluginListItem
                    {
                        PluginName = pluginName,
                        AssemblyPath = destDllPath
                    };
                    loadedPlugins.Add(pluginItem);
                    lb_PluginList.Items.Add(pluginItem.PluginName);

                    // settings.ini의 [RegPlugins] 섹션에 플러그인 정보 기록
                    //SavePluginInfoToSettings(pluginItem);
                    
                    //logManager.LogEvent($"Plugin registered: {pluginName}");
                    PluginListUpdated?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("플러그인 로드 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    logManager.LogError("플러그인 로드 오류: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// btn_PlugRemove 클릭 이벤트 핸들러  
        /// lb_PluginList에서 선택된 플러그인을 삭제 전 확인 메시지를 띄우고,  
        /// loadedPlugins와 lb_PluginList, settings.ini의 [RegPlugins] 섹션, 그리고 Library 폴더의 DLL 파일을 삭제합니다.
        /// </summary>
        private void btn_PlugRemove_Click(object sender, EventArgs e)
        {
            if (lb_PluginList.SelectedItem == null)
            {
                MessageBox.Show("삭제할 플러그인을 선택하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string selectedPluginName = lb_PluginList.SelectedItem.ToString();
            DialogResult result = MessageBox.Show($"플러그인 '{selectedPluginName}'을(를) 삭제하시겠습니까?", 
                                                  "삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                var pluginItem = loadedPlugins.FirstOrDefault(p => p.PluginName.Equals(selectedPluginName, StringComparison.OrdinalIgnoreCase));
                if (pluginItem != null)
                {
                    // Library 폴더의 DLL 파일 삭제 시도
                    if (File.Exists(pluginItem.AssemblyPath))
                    {
                        try
                        {
                            File.Delete(pluginItem.AssemblyPath);
                            logManager.LogEvent($"DLL 파일 삭제됨: {pluginItem.AssemblyPath}");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("DLL 파일 삭제 중 오류 발생: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            logManager.LogError("DLL 파일 삭제 중 오류: " + ex.Message);
                            return;
                        }
                    }
                    loadedPlugins.Remove(pluginItem);
                }
                lb_PluginList.Items.Remove(selectedPluginName);

                // settings.ini의 [RegPlugins] 섹션에서 해당 키 제거
                settingsManager.RemoveKeyFromSection("RegPlugins", selectedPluginName);

                logManager.LogEvent($"플러그인 삭제됨: {selectedPluginName}");
            }
        }

        /// <summary>
        /// settings.ini의 [RegPlugins] 섹션에 플러그인 정보를 기록합니다.
        /// 형식 예시:
        /// [RegPlugins]
        /// MyPlugin = C:\... \Library\MyPlugin.dll
        /// </summary>
        private void SavePluginInfoToSettings(PluginListItem pluginItem)
        {
            settingsManager.SetValueToSection("RegPlugins", pluginItem.PluginName, pluginItem.AssemblyPath);
        }

        /// <summary>
        /// settings.ini의 [RegPlugins] 섹션에서 저장된 플러그인 정보를 읽어와 로드합니다.
        /// </summary>
        private void LoadPluginsFromSettings()
        {
            var pluginLines = settingsManager.GetFoldersFromSection("[RegPlugins]");
            foreach (var line in pluginLines)
            {
                // line 형식: "PluginName = AssemblyPath"
                string[] parts = line.Split(new char[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    string pluginName = parts[0].Trim();
                    string assemblyPath = parts[1].Trim();
                    if (File.Exists(assemblyPath))
                    {
                        try
                        {
                            // 파일을 바이트 배열로 읽어 메모리로 로드
                            byte[] dllData = File.ReadAllBytes(assemblyPath);
                            Assembly asm = Assembly.Load(dllData);
                            if (asm.GetName().Name.Equals(pluginName, StringComparison.OrdinalIgnoreCase))
                            {
                                PluginListItem pluginItem = new PluginListItem
                                {
                                    PluginName = pluginName,
                                    AssemblyPath = assemblyPath
                                };
                                loadedPlugins.Add(pluginItem);
                                lb_PluginList.Items.Add(pluginItem.PluginName);
                                logManager.LogEvent($"Plugin loaded from settings: {pluginName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            logManager.LogError("Settings에서 플러그인 로드 오류: " + ex.Message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 외부에서 로드된 플러그인 목록을 반환합니다.
        /// </summary>
        public List<PluginListItem> GetLoadedPlugins()
        {
            return loadedPlugins;
        }
    }
}
