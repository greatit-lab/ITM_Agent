// Library\IOnto_WaferFlatData.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using ConnectInfo;

namespace Onto_WaferFlatDataLib
{
    /*──────────────────────── Logger (수정 버전) ────────────────────────*/
    internal static class SimpleLogger
    {
        private static readonly object _sync  = new object();
        private static readonly string _logDir =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
    
        private static string GetPath(string suffix) =>
            Path.Combine(_logDir, $"{DateTime.Now:yyyyMMdd}_{suffix}.log");
    
        private static void Write(string suffix, string msg)
        {
            lock (_sync)
            {
                if (!Directory.Exists(_logDir)) Directory.CreateDirectory(_logDir);
                string line =
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [Onto_WaferFlatData] {msg}{Environment.NewLine}";
                File.AppendAllText(GetPath(suffix), line, Encoding.UTF8);
            }
        }

        public static void Event(string msg) => Write("event",  msg);
        public static void Error(string msg) => Write("error",  msg);
        public static void Debug(string msg) => Write("debug",  msg);
    }
    /*────────────────────────────────────────────────────────────────────*/

    public interface IOnto_WaferFlatData
    {
        string PluginName { get; }
        void ProcessAndUpload(string folderPath, string settingsFilePath = "Settings.ini");
    }
    
    public class Onto_WaferFlatData : IOnto_WaferFlatData
    {
        private static string ReadAllTextSafe(string path, Encoding enc, int timeoutMs = 30000)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (true)
            {
                try
                {
                    using (var fs = new FileStream(path, FileMode.Open,
                                                   FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, enc))
                        return sr.ReadToEnd();
                }
                catch (IOException)
                {
                    if (sw.ElapsedMilliseconds > timeoutMs)
                        throw;
                    System.Threading.Thread.Sleep(500);
                }
            }
        }
        
        
        public string PluginName => "Onto_WaferFlatData";
        
        static Onto_WaferFlatData()                           // ← 추가
        {
            // .NET Core/5+/6+/8+ 에서 CP949 등 코드 페이지 인코딩 사용 가능하게 등록
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        
        public string PluginName => "Onto_WaferFlatData";

        #region === 외부 호출 ===
        public void ProcessAndUpload(string filePath)
        {
            SimpleLogger.Event($"ProcessAndUpload(file) ▶ {filePath}");
            if (!File.Exists(filePath))
            {
                SimpleLogger.Error($"File NOT FOUND ▶ {filePath}");
                return;
            }
            string eqpid = GetEqpidFromSettings("Settings.ini");
            try { ProcessFile(filePath, eqpid); }
            catch (Exception ex) { SimpleLogger.Error(ex.Message); }
        }
        
        /* ② UploadPanel이 2-파라미터로 호출할 때 */
        public void ProcessAndUpload(string filePath, string settingsPath = "Settings.ini")
        {
            SimpleLogger.Event($"ProcessAndUpload(file,ini) ▶ {filePath}");
            if (!File.Exists(filePath))
            {
                SimpleLogger.Error($"File NOT FOUND ▶ {filePath}");
                return;
            }
            string eqpid = GetEqpidFromSettings(settingsPath);
            try { ProcessFile(filePath, eqpid); }
            catch (Exception ex) { SimpleLogger.Error(ex.Message); }
        }
        
        #endregion
        
        #region === 파일 처리 ===
        private void ProcessFile(string filePath, string eqpid)
        {
            SimpleLogger.Debug($"PARSE ▶ {Path.GetFileName(filePath)}");
        
            string raw = ReadAllTextSafe(filePath, Encoding.GetEncoding(949)); // cp949
            var    lines = raw.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        
            /* ---- 1) Key–Value 메타 ---- */
            var meta = new Dictionary<string, string>();
            foreach (var ln in lines)
            {
                int idx = ln.IndexOf(':');
                if (idx > 0)
                {
                    string key = ln.Substring(0, idx).Trim();
                    string val = ln.Substring(idx + 1).Trim();
                    if (!meta.ContainsKey(key)) meta[key] = val;
                }
            }
        
            /* ---- 2) Wafer·DateTime ---- */
            int? waferNo = null;
            if (meta.TryGetValue("Wafer ID", out string waferId))
            {
                var m = Regex.Match(waferId, @"W(\d+)");
                if (m.Success && int.TryParse(m.Groups[1].Value, out int w)) waferNo = w;
            }
            DateTime dtVal = DateTime.MinValue;
            if (meta.TryGetValue("Date and Time", out string dtStr))
                DateTime.TryParse(dtStr, out dtVal);
        
            /* ---- 3) 헤더 찾기 ---- */
            int hdrIdx = Array.FindIndex(lines, l =>
                         l.TrimStart().StartsWith("Point#", StringComparison.OrdinalIgnoreCase));
            if (hdrIdx == -1)
            {
                SimpleLogger.Error("Header NOT FOUND → skip");
                return;
            }
        
            /* 3-1) 헤더 정규화 ― (탆) 토큰 제거 추가 */
            Func<string, string> clean = h => h.Replace("(no Cal)", "_noCal")
                                               .Replace("(mm)", "")
                                               .Replace("(탆)", "")        // ★ 추가
                                               .Replace("Die X", "DieX")
                                               .Replace("Die Y", "DieY")
                                               .Trim();
        
            var headers = lines[hdrIdx].Split(',').Select(clean).ToList();
            var rows    = new List<Dictionary<string, object>>();
        
            /* ---- 4) 데이터 라인 파싱 ---- */
            for (int i = hdrIdx + 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                var vals = lines[i].Split(',').Select(v => v.Trim()).ToArray();
                if (vals.Length < headers.Count) continue;
        
                var row = new Dictionary<string, object>
                {
                    ["CassetteRCP"] = meta.ContainsKey("Cassette Recipe Name") ? meta["Cassette Recipe Name"] : "",
                    ["StageRCP"]    = meta.ContainsKey("Stage Recipe Name")    ? meta["Stage Recipe Name"]    : "",
                    ["StageGroup"]  = meta.ContainsKey("Stage Group Name")     ? meta["Stage Group Name"]     : "",
                    ["LotID"]       = meta.ContainsKey("Lot ID")               ? meta["Lot ID"]               : "",
                    ["WaferID"]     = waferNo ?? (object)DBNull.Value,
                    ["DateTime"]    = (dtVal != DateTime.MinValue) ? (object)dtVal : DBNull.Value,
                    ["Film"]        = meta.ContainsKey("Film Name") ? meta["Film Name"] : ""
                };
        
                int tmpInt; double tmpDbl;
                row["Point"] = (vals.Length > 0 && int.TryParse(vals[0], out tmpInt)) ? (object)tmpInt : DBNull.Value;
                row["MSE"]   = (vals.Length > 1 && double.TryParse(vals[1], out tmpDbl)) ? (object)tmpDbl : DBNull.Value;
        
                for (int col = 2; col < headers.Count && col < vals.Length; col++)
                {
                    string k = headers[col];
                    string v = vals[col];
        
                    if (new[] { "DieRow","DieCol","DieNum","DiePointTag" }.Contains(k)
                        && int.TryParse(v, out tmpInt))
                        row[k] = tmpInt;
                    else if (double.TryParse(v, out tmpDbl))
                        row[k] = tmpDbl;
                    else
                        row[k] = DBNull.Value;
                }
                rows.Add(row);
            }
        
            if (rows.Count == 0)
            {
                SimpleLogger.Debug("rows=0 → skip");
                return;
            }
        
            /* ---- 5) DataTable 생성 ---- */
            DataTable dt = new DataTable();
            foreach (var k in rows[0].Keys) dt.Columns.Add(k, typeof(object));
            dt.Columns.Add("Eqpid", typeof(string));
        
            foreach (var r in rows)
            {
                var dr = dt.NewRow();
                foreach (var k in r.Keys) dr[k] = r[k] ?? DBNull.Value;
                dr["Eqpid"] = eqpid;
                dt.Rows.Add(dr);
            }
        
            UploadToMySQL(dt);
            SimpleLogger.Event($"{Path.GetFileName(filePath)} ▶ rows={dt.Rows.Count}");
            try { File.Delete(filePath); } catch { /* ignore */ }
        }
        #endregion
        
        #region === DB Upload ===
        private void UploadToMySQL(DataTable dt)
        {
            var dbInfo = DatabaseInfo.CreateDefault();
            using var conn = new MySqlConnection(dbInfo.GetConnectionString());
            conn.Open();
            using var tx = conn.BeginTransaction();
        
            var cols = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
            string sql = $"INSERT INTO wf_flat ({string.Join(",", cols)})" +
                         $" VALUES ({string.Join(",", cols.Select(c => "@" + c))})";
        
            using var cmd = new MySqlCommand(sql, conn, tx);
            foreach (var c in cols) cmd.Parameters.Add(new MySqlParameter("@" + c, DBNull.Value));
        
            int ok = 0;
            try
            {
                foreach (DataRow r in dt.Rows)
                {
                    foreach (var c in cols) cmd.Parameters["@" + c].Value = r[c] ?? DBNull.Value;
                    cmd.ExecuteNonQuery();
                    ok++;
                }
                tx.Commit();
                SimpleLogger.Debug($"DB OK ▶ {ok} rows");
            }
            catch (MySqlException mex)
            {
                tx.Rollback();
                /* ▼▼ 상세 정보 기록 ▼▼ */
                var sb = new StringBuilder();
                sb.AppendLine($"MySQL ERRNO={mex.Number}");
                sb.AppendLine($"Message={mex.Message}");
                sb.AppendLine("SQL=" + sql);
                foreach (var p in cmd.Parameters.Cast<MySqlParameter>())
                    sb.AppendLine($"{p.ParameterName}={p.Value}");
                SimpleLogger.Error("DB FAIL ▶ " + sb.ToString());
            }
            catch (Exception ex)
            {
                tx.Rollback();
                SimpleLogger.Error("DB FAIL ▶ " + ex);
            }
        }
        #endregion
        
        #region === Eqpid 읽기 ===
        private string GetEqpidFromSettings(string iniPath)
        {
            if (!File.Exists(iniPath)) return "";
            foreach (var line in File.ReadLines(iniPath))
            {
                if (line.Trim().StartsWith("Eqpid", StringComparison.OrdinalIgnoreCase))
                {
                    int idx = line.IndexOf('=');
                    if (idx > 0) return line.Substring(idx + 1).Trim();
                }
            }
            return "";
        }
        #endregion
    }
}
