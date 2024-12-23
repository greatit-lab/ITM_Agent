using ITM_Agent.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ITM_Agent.ucPanel
{
    public partial class ucOverrideNamesPanel : UserControl
    {
        private readonly SettingsManager settingsManager;
        private List<string> regexFolders; // 정규표현식과 폴더 정보 저장
        private string baseFolder; // BaseFolder 저장
        
        public event Action<string, Color> StatusUpdated;

        public ucOverrideNamesPanel(SettingsManager settingsManager)
        {
            this.settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            InitializeComponent();
            
            InitializeCustomEvents();
            PopulateComboBox();
            
            // 데이터 로드
            LoadDataFromSettings();
            
            LoadRegexFolderPaths(); // 초기화 시 목록 로드
            LoadSelectedBaseDatePath(); // 저장된 선택 값 불러오기
        }

        private void InitializeCustomEvents()
        {
            cb_BaseDatePath.SelectedIndexChanged += cb_BaseDatePath_SelectedIndexChanged;
            btn_BaseClear.Click += btn_BaseClear_Click;
            btn_SelectFolder.Click += Btn_SelectFolder_Click;
            btn_Remove.Click += Btn_Remove_Click;
        }

        /// <summary>
        /// lb_RegexList에서 '->' 뒤의 폴더 경로 값만 가져와 cb_BaseDatePath를 초기화합니다.
        /// </summary>
        private void LoadRegexFolderPaths()
        {
            cb_BaseDatePath.Items.Clear();
            var regexList = settingsManager.GetRegexList();
            var folderPaths = regexList.Values.ToList();
            cb_BaseDatePath.Items.AddRange(folderPaths.ToArray());
            cb_BaseDatePath.SelectedIndex = -1; // 초기화
        }

        /// <summary>
        /// Settings.ini 파일에 저장된 [SelectedBaseDatePath] 값을 불러옵니다.
        /// </summary>
        private void LoadSelectedBaseDatePath()
        {
            string selectedPath = settingsManager.GetValueFromSection("SelectedBaseDatePath", "Path");
            if (!string.IsNullOrEmpty(selectedPath) && cb_BaseDatePath.Items.Contains(selectedPath))
            {
                cb_BaseDatePath.SelectedItem = selectedPath;
            }
        }

        /// <summary>
        /// ComboBox를 정규표현식 폴더 정보로 채웁니다.
        /// </summary>
        private void PopulateComboBox()
        {
            cb_BaseDatePath.SelectedIndex = -1; // 초기화
        }

        /// <summary>
        /// Clear 버튼 클릭 시 ComboBox 선택 초기화
        /// </summary>
        private void Btn_BaseClear_Click(object sender, EventArgs e)
        {
            cb_BaseDatePath.SelectedIndex = -1;
        }

        /// <summary>
        /// Select Folder 버튼 클릭 시 폴더 선택 및 ListBox에 추가
        /// </summary>
        private void Btn_SelectFolder_Click(object sender, EventArgs e)
        {
            var baseFolder = settingsManager.GetFoldersFromSection("[BaseFolder]").FirstOrDefault() ?? AppDomain.CurrentDomain.BaseDirectory;
            
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.SelectedPath = baseFolder;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    if (!lb_TargetComparePath.Items.Contains(folderDialog.SelectedPath))
                    {
                        lb_TargetComparePath.Items.Add(folderDialog.SelectedPath);
                        
                        // Settings.ini 에 추가
                        UpdateTargetComparePathInSettings();
                    }
                    else
                    {
                        MessageBox.Show("해당 폴더는 이미 추가되어 있습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        /// <summary>
        /// Remove 버튼 클릭 시 선택된 항목 삭제
        /// </summary>
        private void Btn_Remove_Click(object sender, EventArgs e)
        {
            if (lb_TargetComparePath.SelectedItems.Count > 0)
            {
                var confirmResult = MessageBox.Show("선택한 항목을 삭제하시겠습니까?", "삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirmResult == DialogResult.Yes)
                {
                    var selectedItems = lb_TargetComparePath.SelectedItems.Cast<string>().ToList();
                    foreach (var item in selectedItems)
                    {
                        lb_TargetComparePath.Items.Remove(item);
                    }
                    
                    // Settings.ini 갱신
                    UpdateTargetComparePathInSettings();
                }
            }
            else
            {
                MessageBox.Show("삭제할 항목을 선택하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        private void UpdateTargetComparePathInSettings()
        {
            var folders = lb_TargetComparePath.Items.Cast<string>().ToList();
            settingsManager.SetFoldersToSection("[TargetComparePath]", folders);
        }
        
        public void UpdateStatusOnRun(bool isRunning)
        {
            string status = isRunning ? "Running" : "Stopped";
            Color statusColor = isRunning ? Color.Green : Color.Red;
            
            StatusUpdated?.Invoke($"Status: {status}", statusColor);
        }
        
        public void InitializePanel(bool isRunning)
        {
            UpdateStatusOnRun(isRunning); // 초기화 시 상태 동기화
        }
        
        public void LoadDataFromSettings()
        {
            // [BaseFolder] 섹션에서 데이터를 로드하여 ComboBox에 반영
            var baseFolders = settingsManager.GetFoldersFromSection("[BaseFolder]");
            cb_BaseDatePath.Items.Clear();
            cb_BaseDatePath.Items.AddRange(baseFolders.ToArray());
        
            // [TargetComparePath] 섹션에서 데이터를 로드하여 ListBox에 반영
            var comparePaths = settingsManager.GetFoldersFromSection("[TargetComparePath]");
            lb_TargetComparePath.Items.Clear();
            foreach (var path in comparePaths)
            {
                lb_TargetComparePath.Items.Add(path);
            }
        }
        
        public void RefreshUI()
        {
            LoadDataFromSettings(); // 설정값을 다시 로드하여 UI 갱신
        }
    }
}
