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

        /// <summary>
        /// 일반 이벤트 로그 기록
        /// </summary>
        public void LogEvent(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            WriteLog($"{timestamp} - EVENT: {message}", $"{DateTime.Now:yyyyMMdd}_event.log");
        }

        /// <summary>
        /// 이벤트 또는 디버그 로그 기록 (조건부)
        /// </summary>
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

        /// <summary>
        /// 에러 로그 기록
        /// </summary>
        public void LogError(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            WriteLog($"{timestamp} - ERROR: {message}", $"{DateTime.Now:yyyyMMdd}_error.log");
        }

        /// <summary>
        /// 디버그 로그 기록
        /// </summary>
        public void LogDebug(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            WriteLog($"{timestamp} - DEBUG: {message}", $"{DateTime.Now:yyyyMMdd}_debug.log");
        }

        /// <summary>
        /// 로깅 메커니즘 확장 (추가적인 로그 유형 포함)
        /// </summary>
        public void LogCustom(string message, string logType)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string fileName = $"{DateTime.Now:yyyyMMdd}_{logType.ToLower()}.log";
            WriteLog($"{timestamp} - {logType.ToUpper()}: {message}", fileName);
        }

        /// <summary>
        /// 로그 파일에 메시지를 기록하는 메서드
        /// </summary>
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
