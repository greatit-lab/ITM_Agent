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

        public ucImageTransPanel(SettingsManager settingsManager)
        {
            this.settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            InitializeComponent();

            // PDF 병합 관리자 초기화
            pdfMergeManager = new PdfMergeManager(AppDomain.CurrentDomain.BaseDirectory);

            // 이벤트 연결
            btn_SelectOutputFolder.Click += Btn_SelectOutputFolder_Click;
            cb_TargetImageFolder.SelectedIndexChanged += Cb_TargetImageFolder_SelectedIndexChanged;
        }

        private void Btn_SelectOutputFolder_Click(object sender, EventArgs e)
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

        private async void Cb_TargetImageFolder_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_TargetImageFolder.SelectedItem is string selectedFolder && Directory.Exists(selectedFolder))
            {
                int waitTime = int.Parse(cb_WaitTime.SelectedItem?.ToString() ?? "30");
                await pdfMergeManager.MergeImagesToPDF(selectedFolder, waitTime);
                MessageBox.Show("PDF 병합이 완료되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void LoadTargetFolders()
        {
            cb_TargetImageFolder.Items.Clear();
            var folders = settingsManager.GetFoldersFromSection("[TargetFolders]");
            cb_TargetImageFolder.Items.AddRange(folders.ToArray());
            cb_TargetImageFolder.SelectedIndex = -1;
        }

        public void LoadWaitTimes()
        {
            cb_WaitTime.Items.Clear();
            cb_WaitTime.Items.AddRange(new object[] { "30", "60", "120", "180", "240", "300" });
            cb_WaitTime.SelectedIndex = 0;
        }

        public void RefreshUI()
        {
            LoadTargetFolders();
            LoadWaitTimes();
        }
    }
}
