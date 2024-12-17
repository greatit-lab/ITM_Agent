// Services\EqpidManager.cs
using System;
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
            bool isValidInput = false;

            while (!isValidInput)
            {
                using (var form = new EqpidInputForm())
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK && !string.IsNullOrEmpty(form.Eqpid))
                    {
                        settingsManager.SetEqpid(form.Eqpid.ToUpper());
                        isValidInput = true;
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        MessageBox.Show("Eqpid 입력이 취소되었습니다. 애플리케이션을 종료합니다.",
                                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Environment.Exit(0);    // 안전하게 애플리케이션 종료
                    }
                    else
                    {
                        MessageBox.Show("Eqpid input is required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
