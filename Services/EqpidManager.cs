// Services\EqpidManager.cs
using System.Windows.Forms;

namespace ITM_Agent.Services
{
    /// <summary>
    /// Eqpid 값을 관리하는 클래스입니다.
    /// 설정 파일(Settings.ini)에 [Eqpid] 섹션이 없거나 값이 없을 경우 EqpidInputForm을 통해 장비명을 입력받아 저장합니다.
    /// </summary>
    public class EqpidManager
    {
        private readonly SettingsManager settingsManager;

        public EqpidManager(SettingsManager settings)
        {
            this.settingsManager = settings;
        }

        public void InitializeEqpid()
        {
            string eqpid = settingsManager.GetEqpid();
            if (string.IsNullOrEmpty(eqpid))
            {
                PromptForEqpid();
            }
        }

        private void PromptForEqpid()
        {
            using (var form = new EqpidInputForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    if (!string.IsNullOrEmpty(form.Eqpid))
                    {
                        settingsManager.SetEqpid(form.Eqpid.ToUpper());
                    }
                    else
                    {
                        MessageBox.Show("Eqpid input is required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        PromptForEqpid();
                    }
                }
                else
                {
                    // 사용자가 취소하면 애플리케이션 종료
                    Application.Exit();
                }
            }
        }
    }
}
