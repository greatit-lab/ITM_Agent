// Program.cs
using ITM_Agent.Services;
using ITM_Agent;
using System;
using System.Windows.Forms;
using System.IO;

namespace ITM_Agent
{
    internal static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            
            // SettingsManager 인스턴스 생성
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var settingsManager = new SettingsManager(Path.Combine(baseDir, "Settings.ini"));
            
            // MainForm 실행
            Application.Run(new MainForm(settingsManager));
        }
    }
}
