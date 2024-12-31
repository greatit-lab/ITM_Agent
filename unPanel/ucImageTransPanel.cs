using ITM_Agent.Services;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace ITM_Agent.ucPanel
{
    public partial class ucImageTransPanel : UserControl
    {
        private readonly PdfMergeManager pdfMergeManager;
        private readonly SettingsManager settingsManager;
        private readonly ucConfigurationPanel configPanel;

        public ucImageTransPanel(SettingsManager settingsManager, ucConfigurationPanel configPanel)
        {
            this.settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            this.configPanel = configPanel ?? throw new ArgumentNullException(nameof(configPanel));
            InitializeComponent();

            // PDF 병합 관리자 초기화
            pdfMergeManager = new PdfMergeManager(AppDomain.CurrentDomain.BaseDirectory);

            // 이벤트 연결
            btn_SetFolder.Click += btn_SetFolder_Click;
            btn_FolderClear.Click += btn_FolderClear_Click;
            
            btn_SetTime.Click += btn_SetTime_Click;
            btn_TimeClear.Click += btn_TimeClear_Click;
            
            btn_SelectOutputFolder.Click += btn_SelectOutputFolder_Click;
            
            // UI 초기화
            LoadRegexFolderPaths();
            LoadWaitTimes();
        }

        private void btn_SelectOutputFolder_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    pdfMergeManager.UpdateOutputFolder(folderDialog.SelectedPath);
                    MessageBox.Show("출력 폴더가 설정되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        
        private void btn_SetFolder_Click(object sender, EventArgs e)
        {
            if (cb_TargetImageFolder.SelectedItem is string selectedFolder)
            {
                // 선택된 폴더를 설정 파일에 기록
                settingsManager.SetValueToSection("ImageTrans", "Target", selectedFolder);
                MessageBox.Show($"폴더가 설정되었습니다: {selectedFolder}", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("폴더를 선택하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        private void btn_FolderClear_Click(object sender, EventArgs e)
        {
            if (cb_TargetImageFolder.SelectedItem != null)
            {
                // 콤보박스 선택 해제
                cb_TargetImageFolder.SelectedIndex = -1;

                // 설정 파일에서 제거
                settingsManager.RemoveSection("ImageTrans");

                MessageBox.Show("폴더 설정이 초기화되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("선택된 폴더가 없습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        public void LoadRegexFolderPaths()
        {
            cb_TargetImageFolder.Items.Clear();

            // ucConfigurationPanel의 lb_RegexList에서 폴더 경로 가져오기
            var regexFolders = configPanel.GetRegexList();
            cb_TargetImageFolder.Items.AddRange(regexFolders.ToArray());

            // 설정 파일에서 마지막으로 저장된 폴더 가져와 콤보박스에 표시
            string selectedPath = settingsManager.GetValueFromSection("ImageTrans", "Target");
            if (!string.IsNullOrEmpty(selectedPath) && cb_TargetImageFolder.Items.Contains(selectedPath))
            {
                cb_TargetImageFolder.SelectedItem = selectedPath;
            }
            else
            {
                cb_TargetImageFolder.SelectedIndex = -1;
            }
        }

        public void LoadWaitTimes()
        {
            cb_WaitTime.Items.Clear();
            cb_WaitTime.Items.AddRange(new object[] { "30", "60", "120", "180", "240", "300" });

            // 기본 선택 없음
            cb_WaitTime.SelectedIndex = -1;

            // 설정 파일에서 저장된 값을 가져와 선택
            string savedWaitTime = settingsManager.GetValueFromSection("ImageTrans", "Wait");
            if (!string.IsNullOrEmpty(savedWaitTime) && cb_WaitTime.Items.Contains(savedWaitTime))
            {
                cb_WaitTime.SelectedItem = savedWaitTime;
            }
        }
        
        private void btn_SetTime_Click(object sender, EventArgs e)
        {
            if (cb_WaitTime.SelectedItem is string selectedWaitTime)
            {
                // 선택된 Wait 값을 설정 파일에 저장
                settingsManager.SetValueToSection("ImageTrans", "Wait", selectedWaitTime);
                MessageBox.Show($"대기 시간이 설정되었습니다: {selectedWaitTime} 초", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("대기 시간을 선택하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        private void btn_TimeClear_Click(object sender, EventArgs e)
        {
            if (cb_WaitTime.SelectedItem != null)
            {
                // 콤보박스 선택 해제
                cb_WaitTime.SelectedIndex = -1;

                // 설정 파일에서 Wait 값을 제거
                settingsManager.SetValueToSection("ImageTrans", "Wait", string.Empty);

                MessageBox.Show("대기 시간이 초기화되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("선택된 대기 시간이 없습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void RefreshUI()
        {
            LoadRegexFolderPaths();
            LoadWaitTimes();
        }
    }
}
