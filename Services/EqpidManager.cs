// Services\EqpidManager.cs
using System;
using System.Windows.Forms;
using System.Data;
using MySql.Data.MySqlClient;  // MySql.Data.dll 참조 필요
using ConnectInfo;            // ConnectInfo.dll( DatabaseInfo ) 참조
using System.Globalization;

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
                        // (기존 코드) eqpid, type 설정
                        logManager.LogEvent($"[EqpidManager] Eqpid input accepted: {form.Eqpid}");
                        settingsManager.SetEqpid(form.Eqpid.ToUpper());
                        settingsManager.SetType(form.Type);
                        logManager.LogEvent($"[EqpidManager] Type set to: {form.Type}");
                        
                        // 여기서 DB 업로드를 호출
                        UploadAgentInfoToDatabase(form.Eqpid.ToUpper(), form.Type);
        
                        isValidInput = true;
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        // (기존 코드)
                        logManager.LogEvent("[EqpidManager] Eqpid input canceled. Application will exit.");
                        MessageBox.Show("Eqpid 입력이 취소되었습니다. 애플리케이션을 종료합니다.",
                                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Environment.Exit(0);
                    }
                }
            }
        }
        
        /// <summary>
        /// Eqpid와 Type, 그리고 PC 시스템 정보를 DB에 업로드하는 메서드
        /// </summary>
        private void UploadAgentInfoToDatabase(string eqpid, string type)
        {
            // 1) ConnectInfo.dll을 통해 Default DB 정보 가져오기
            DatabaseInfo dbInfo = DatabaseInfo.CreateDefault();
            string connectionString = dbInfo.GetConnectionString();
            // 예: "Server=localhost;Port=3306;Database=testdb;Uid=admin;Pwd=password123;"

            // 2) 시스템 정보 수집
            string osVersion    = SystemInfoCollector.GetOSVersion();
            string architecture = SystemInfoCollector.GetArchitecture();
            string machineName  = SystemInfoCollector.GetMachineName();
            string locale       = SystemInfoCollector.GetLocale();
            string timeZone     = SystemInfoCollector.GetTimeZone();

            // 3) DB 연결 후 INSERT
            //    (테이블 이름이 itm.agent_info 라고 가정)
            //    컬럼 구성(가정): eqpid, type, os_version, architecture, pc_name, locale, timezone, reg_date
            string query = @"
                INSERT INTO itm.agent_info
                (eqpid, type, os, system_type, pc_name, locale, timezone, reg_date)
                VALUES
                (@eqpid, @type, @os, @arch, @pc, @loc, @tz, NOW());
            ";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // 파라미터 바인딩
                        cmd.Parameters.AddWithValue("@eqpid", eqpid);
                        cmd.Parameters.AddWithValue("@type", type);
                        cmd.Parameters.AddWithValue("@os", osVersion);
                        cmd.Parameters.AddWithValue("@arch", architecture);
                        cmd.Parameters.AddWithValue("@pc", machineName);
                        cmd.Parameters.AddWithValue("@loc", locale);
                        cmd.Parameters.AddWithValue("@tz", timeZone);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        logManager.LogEvent($"[EqpidManager] DB 업로드 완료. (rows inserted={rowsAffected})");
                    }
                }
            }
            catch (Exception ex)
            {
                logManager.LogError($"[EqpidManager] DB 업로드 실패: {ex.Message}");
            }
        }
    }
    
    public class SystemInfoCollector
    {
        public static string GetOSVersion()
        {
            // 예: "Microsoft Windows NT 10.0.19044.0"
            return Environment.OSVersion.ToString();
        }
    
        public static string GetArchitecture()
        {
            // 64비트 OS 여부 판단
            return Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
        }
    
        public static string GetMachineName()
        {
            // 예: "DESKTOP-ABCD123"
            return Environment.MachineName;
        }
    
        public static string GetLocale()
        {
            // 현재 UI 문화권
            // 예: "ko-KR"
            return CultureInfo.CurrentUICulture.Name;
        }
    
        public static string GetTimeZone()
        {
            // 예: "Korea Standard Time"
            return TimeZoneInfo.Local.StandardName; 
        }
    }
}
