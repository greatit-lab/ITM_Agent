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

        // ================== 개선된 전체 메서드 ==================
        private void RotateLogFileIfNeeded(string filePath)
        {
            if (!File.Exists(filePath))
                return;
        
            FileInfo fi = new FileInfo(filePath);
        
            // (1) 5 MB 이하이면 그대로 사용
            if (fi.Length <= MAX_LOG_SIZE)
                return;
        
            // (2) 확장자 / 기본 이름 분리
            string extension   = fi.Extension;                               // ".log"
            string withoutExt  = Path.GetFileNameWithoutExtension(filePath); // "20250711_event"
        
            // (3) 다음 회전 인덱스 계산
            int index = 1;
            string rotatedPath;
            do
            {
                string rotatedName = $"{withoutExt}_{index}{extension}";     // ex) "20250711_event_3.log"
                rotatedPath = Path.Combine(logFolderPath, rotatedName);
                index++;
            }
            while (File.Exists(rotatedPath));  // 존재하는 파일이 없을 때까지 증가
        
            // (4) 원본 → 새 인덱스 파일로 이동
            File.Move(filePath, rotatedPath);
        
            // (5) 이후 WriteLogWithRotation() 가 호출되면서
            //     같은 이름의 새로운 원본 로그(0번) 파일이 자동 생성되어
            //     이어서 로그가 기록됨.
        }
    }
}
