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
        private readonly LogManager logManager;

        public EqpidManager(SettingsManager settings, LogManager logManager)
        {
            this.settingsManager = settings ?? throw new ArgumentNullException(nameof(settings));
            this.logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));
        }

        public void InitializeEqpid()
        {
            logManager.LogEvent("[EqpidManager] Initializing Eqpid.");
            
            string eqpid = settingsManager.GetEqpid();
            if (string.IsNullOrEmpty(eqpid))
            {
                logManager.LogEvent("[EqpidManager] Eqpid is empty. Prompting for input.");
                PromptForEqpid();
            }
            else
            {
                logManager.LogEvent($"[EqpidManager] Eqpid found: {eqpid}");
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
                    if (result == DialogResult.OK)
                    {
                        logManager.LogEvent($"[EqpidManager] Eqpid input accepted: {form.Eqpid}");
                        settingsManager.SetEqpid(form.Eqpid.ToUpper());
                        settingsManager.SetType(form.Type); // Type 값 설정
                        logManager.LogEvent($"[EqpidManager] Type set to: {form.Type}");
                        isValidInput = true;
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        logManager.LogEvent("[EqpidManager] Eqpid input canceled. Application will exit.");
                        MessageBox.Show("Eqpid 입력이 취소되었습니다. 애플리케이션을 종료합니다.",
                                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Environment.Exit(0);
                    }
                }
            }
        }
    }
}
