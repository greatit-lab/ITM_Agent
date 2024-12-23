// Services\LogManager.cs
using System;
using System.IO;

namespace ITM_Agent.Services
{
    /// <summary>
    /// 이벤트 로그와 디버그 로그 기록을 담당하는 클래스입니다.
    /// </summary>
    public class LogManager
    {
        private readonly string logFolderPath;
    
        public LogManager(string baseDir)
        {
            logFolderPath = Path.Combine(baseDir, "Logs");
            Directory.CreateDirectory(logFolderPath);
        }
    
        public void LogEvent(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");    // 날짜와 시간 형식 설정
            WriteLog($"{timestamp} - {message}", $"{DateTime.Now:yyyyMMdd}_event.log");
        }
    
        public void LogEvent(string message, bool isDebug)
        {
            if (isDebug)
            {
                LogDebug(message);
            }
            else
            {
                LogEvent(message);
            }
        }
    
        public void LogError(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            WriteLog($"{timestamp} - ERROR: {message}", $"{DateTime.Now:yyyyMMdd}_error.log");
        }
    
        public void LogDebug(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            WriteLog($"{timestamp} - DEBUG: {message}", $"{DateTime.Now:yyyyMMdd}_debug.log");
        }
    
        private void WriteLog(string message, string fileName)
        {
            string filePath = Path.Combine(logFolderPath, fileName);
    
            try
            {
                using (var writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write log: {ex.Message}");
            }
        }
    }
}
