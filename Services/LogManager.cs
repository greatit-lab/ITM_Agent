//Services\LogManager.cs
using System;
using System.IO;

namespace ITM_Agent.Services
{
    /// <summary>
    /// 이벤트 로그, 디버그 로그, 에러 로그 등을 기록하고,
    /// 각 로그 파일이 5MB를 초과할 경우 파일을 회전(로테이션)하는 클래스입니다.
    /// </summary>
    public class LogManager
    {
        private readonly string logFolderPath;
        private const long MAX_LOG_SIZE = 5 * 1024 * 1024; // 5MB

        public LogManager(string baseDir)
        {
            logFolderPath = Path.Combine(baseDir, "Logs");
            Directory.CreateDirectory(logFolderPath);
        }

        /// <summary>
        /// (이벤트 로그) 간략하고 핵심적인 메시지만 기록.
        /// 5MB 초과 시 "yyyyMMdd_event_1.log"로 회전.
        /// </summary>
        public void LogEvent(string message)
        {
            // 이벤트 로그: 간략 표기 [EVT]
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logFileName = $"{DateTime.Now:yyyyMMdd}_event.log";
            string logLine = $"{timestamp} [Event] {message}";

            WriteLogWithRotation(logLine, logFileName);
        }

        /// <summary>
        /// 이벤트/디버그 로그 통합 메서드
        /// isDebug가 true이면 디버그로, 아니면 이벤트로 기록
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
        /// (에러 로그) 오류 상황에만 기록.
        /// 5MB 초과 시 "yyyyMMdd_error_1.log"로 회전.
        /// </summary>
        public void LogError(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logFileName = $"{DateTime.Now:yyyyMMdd}_error.log";
            string logLine = $"{timestamp} [Error] {message}";

            WriteLogWithRotation(logLine, logFileName);
        }

        /// <summary>
        /// (디버그 로그) 단계별 상세한 정보 기록.
        /// 5MB 초과 시 "yyyyMMdd_debug_1.log"로 회전.
        /// </summary>
        public void LogDebug(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logFileName = $"{DateTime.Now:yyyyMMdd}_debug.log";
            string logLine = $"{timestamp} [Debug] {message}";

            WriteLogWithRotation(logLine, logFileName);
        }

        /// <summary>
        /// 필요시 사용자 정의 로그 유형 지정 가능
        /// ex) LogCustom("message", "info") => yyyyMMdd_info.log
        /// </summary>
        public void LogCustom(string message, string logType)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string fileName = $"{DateTime.Now:yyyyMMdd}_{logType.ToLower()}.log";
            string logLine = $"{timestamp} [{logType.ToUpper()}] {message}";

            WriteLogWithRotation(logLine, fileName);
        }

        /// <summary>
        /// 로그 기록 시, 파일 용량이 5MB 초과되면 기존 파일을 *_1.log 로 회전 후 새 파일에 기록
        /// </summary>
        private void WriteLogWithRotation(string message, string fileName)
        {
            string filePath = Path.Combine(logFolderPath, fileName);

            try
            {
                // (1) 파일 용량 확인 및 회전(로테이션) 처리
                RotateLogFileIfNeeded(filePath);

                // (2) 회전 처리 이후, 최종 파일에 쓰기
                using (var writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                // 심각한 문제 시 콘솔에 찍어보기
                Console.WriteLine($"Failed to write log: {ex.Message}");
            }
        }

        /// <summary>
        /// 파일 용량 체크 후 5MB 초과 시 "_1" 파일로 회전 (기존 파일 이름 변경)
        /// </summary>
        private void RotateLogFileIfNeeded(string filePath)
        {
            if (File.Exists(filePath))
            {
                FileInfo fi = new FileInfo(filePath);
                if (fi.Length > MAX_LOG_SIZE)
                {
                    // "_1" 파일 이름 만들기
                    string extension = fi.Extension; // ".log"
                    string withoutExt = Path.GetFileNameWithoutExtension(filePath); // ex) "20250122_event"
                    string rotatedName = withoutExt + "_1" + extension;            // ex) "20250122_event_1.log"
                    string rotatedPath = Path.Combine(logFolderPath, rotatedName);

                    // 만약 기존에 _1 파일이 있다면 삭제 (덮어쓰기)
                    if (File.Exists(rotatedPath))
                    {
                        File.Delete(rotatedPath);
                    }

                    // 원본 파일 -> _1 파일로 이동
                    File.Move(filePath, rotatedPath);
                }
            }
        }
    }
}
