using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using ConnectInfo;
using ITM_Agent.Services;

namespace ITM_Agent.Plugins
{
  public interface IUploadPlugin
  {
    string PluginName { get; }
    void ProcessAndUpload(string folderPath);
  }
  
  public class WaferFlatUploadPlugin : IUploadPlugin
  {
    public string PluginName => "Wafer Flat Upload Plugin";
    
    private LogManager logManager;
    
    public WaferFlatUploadPlugin(LogManager logManager = null)
    {
      this.logManager = logManager ?? new LogManager(AppDomain.CurrentDomain.BaseDirectory);
    }
    
    public void ProcessAndUpload(string folderPath)
    {
      if (!Directory.Exists(folderPath))
      {
        logManager.LogError($"[WaferFlatUploadPlugin] Folder does not exist: {folderPath}");
        return;
      }
      
      var logFiles = Directory.GetFiles(folderPath, "*.log", SearchOption.TopDirectoryOnly);
      foreach (var file in logFiles)
      {
        try
        {
          ProcessFile(file);
        }
        catch (Exception ex)
        {
          logManager.LogError($"[WaferFlatUploadPlugin] Error processing file {file}: {ex.Message}");
        }
      }
    }
    
    private void ProcessFile(string filePath)
    {
      logManager.LogEvent($"[WaferFlatUploadPlugin] Processing file: {filePath}");
      
      string content = File.ReadAllText(filePath, Encoding.GetEncoding("cp949"));
      var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
      
      Dictionary<string, string> data = new Dictionary<string, string>();
      foreach (vvar line in lines)
      {
        if (line.Contains(":"))
        {
          int idx = line.IndexOf(":");
          string key = line.Substring(0, idx).Trim();
          string value = line.Substring(idx + 1).Trim();
          if (!data.ContainsKey(key))
            data[key] = value;
        }
      }
      
      int? waferNumber = null;
      if (data.ContainsKey("Wafer ID"))
      {
        var match = Regex.Match(data["Wafer ID"], @"W(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int num))
          waferNumber = num;
      }
      
      DateTime dateTimeVal = DateTime.MinValue;
      if (data.ContainsKey("Date and Time"))
      {
        DateTime.TryParse(data["Date and Time"], out dateTimeVal);
      }
      
      string[] expectedHeader = new string[]
      {
        "Point#","MSE","T1","GOF","HPL","X","Y","DieX","DieY","DieRow","DieCol","DieNum","DiePointTag","Z","SRVISZ",
        "T1_noCal","CU_HT_noCal","T1_CAL","x1","RgnHeight11","RgnHeight16","RgnHeight17","RgnBWidth17"
      };
      
      int headerIndex = -1;
      string headerLine = null;
      for (int i = 0; i < lines.Length; i++)
      {
        if (lines[i].StartsWith("Point#"))
        {
          headerLine = lines[i];
          headerIndex = i;
          break;
        }
      }
      if (headerLine == null)
      {
        logManager.LogError($"[WaferFlatUploadPlugin] Header line not found in file: {filePath}");
        return;
      }
      
      Func<string, string> cleanHeader = h =>
      {
        h = h.Replace("(no Cal)", "_noCal");
        h = h.Replace("(mm)", "").Trim();
        h = h.Replace("(탆)", "").Trim();
        h = h.Replace("Die X", "DieX").Replace("Die Y", "DieY");
        return h;
      };
      
      var headers = headerLine.Split(',')
                              .Select(h => cleanHeader(h.Trim()))
                              .ToList();
      
      List<Dictionary<string object>> rowData = new List<Dictionary<string, object>>();
      for (int i = headerIndex + 1; i < lines.Length; i++)
      {
        string line = lines[i];
        if (string.IsNullOrWhiteSpace(line))
          continue;
        var values = line.Split(',').Select(v => v.Trim()).ToArray();
        Dictionary<string, object> row = new Dictionary<string, object>();
        
        row["CassetteRCP"] = data.ContainsKey("Cassette Recipe Name") ? data["Cassette Recipe Name"] : "";
        row["StageRCP"] = data.ContainsKey("Stage Recipe Name") ? data["Stage Recipe Name"] : "";
        row["StageGroup"] = data.ContainsKey("Stage Group Name") ? data["Stage Group Name"] : "";
        row["LotID"] = data.ContainsKey("Lot ID") ? data["Lot ID"] : "";
        row["WaferID"] = waferNumber.HasValue ? waferNumber.Value :(object)DBNull.Value;
        row["DateTime"] = dateTimeVal != DateTime.MinValue ? dateTimeVal : (object)DBNull.Value;
        row["Film"] = data.ContainsKey("Film Name") ? data["Film Name"] : "";
        
        if (values.Length > 0)
        {
          if (int.TryParse(values[0], out int pt))
            row["Point"] = pt;
          else
            row["Point"] = DBNull.Value;
        }
        if (value.Length > 1)
        {
          if (double.TryParse(vvalues[1], out double mes))
            row["MSE"] = mse;
          else
            row["MSE"] = DBNull.Value;
        }
        
        for (int j = 2; j < expectedHeader.Length; j++)
        {
          string header = expectedHeader[j];
          if (headers.Contains(header))
          {
            int colIndex = headers.IndexOf(header);
            if (colIndex < values.Length)
            {
              if (int.TryParse(values[colIndex], out int intVal))
                row[header] = intVal;
              else
                row[header] = DBNull.Value;
            }
            else
            {
              if (double.TryParse(values[colIndex], out double dbVal))
                row[header] = dbVal;
              else
                row[header] = DBNull.Value;
            }
          }
          else
          {
            row[header] = DBNull.Value;
          }
        }
        else
        {
          row[header] = DBNull.Value;
        }
      }
      
      rowData.Add(row);
    }
    if (rowData.Count == 0)
    {
      logManager.LogError($"[WaferFlatUploadPlugin] No data rows found in file: {filePath}");
      return;
    }
    
    DataTable dt = new DataTable();
    foreach (var key in rowData[0].keys)
    {
      dt.Columns.Add(key, typeof(object));
    }
    foreach (var r in rowData)
    {
      DataRow dr = dt.NewRow();
      foreach (var key in r.Keys)
      {
        dr[key] = r[key] ?? DBNull.Value;
      }
      dt.Rows.Add(dr);
    }
    
    UploadToMySQL(dt);
    
    File.Delete(filePath);
    logManager.LogEvent($"[WaferFlatUploadPlugin] File processed and deleted: {filePath}");
  }
  
  private void UploadToMySQL(DataTable dt)
  {
    try
    {
      var dbInfo = DatabaseInfo.CreateDefault();
      string connectionString = dbInfo.GetConnectionString();
      
      using (MySqlConnection conn = new MySqlConnection(connectionString))
      {
        conn.Open();
        foreach (DateRow row in dt.Rows)
        {
          List<string> columns = new List<string>();
          List<string> paramNames = new List<string>();
          List<MySqlParameter> parameters = new List<MySqlParameter>();
          
          foreach (DataColumn col in dt.Columns)
          {
            columns.Add(col.ColumnName);
            string paramName = "@" + col.ColumnName;
            paramNames.Add(paramName);
            parameters.Add(new MySqlParameter(paramName, row[col.ColumnName] ?? DBNull.Value));
          }
          
          string sql = $"INSERT INTO wf_flat ({string.Join(",", columns)}) VALUES ({string.Join(",", paramNames)})";
          using (MySqlCommand cmd = new MySqlCommand(sql, conn))
          {
            cmd.Parameters.AddRange(parameters.ToArray());
            cmd.ExecuteNonQuery();
          }
        }
      }
      logManager.LogEvent("[WaferFlatUploadPlugin] Successfully uploaded data to MySQL.");
    }
    catch (Exception ex)
    {
      logManager.LogError($"[WaferFlatUploadPlugin] Upload to MySQL failed: {ex.Message}");
    }
  }
}}
