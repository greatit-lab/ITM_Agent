// ConnectInfo\DatabaseInfo.cs
using System;
using MySql.Data.MySqlClient;

namespace ConnectInfo
{
    public sealed class DatabaseInfo
    {
        /* -- 하드코딩된 접속 정보 -- */
        public const string _server = "00.000.00.00";
        public const string _database = "itm";
        public const string _userId = "userid";
        public const string _password = "pw";
        public const int _port = 3306;
        
        private DatabaseInfo() {}
        
        public static DatabaseInfo CreateDefault() => new DatabaseInfo();
        
        public string GetConnectionString()
        {
            var csb = new MySqlConnectionStringBuilder
            {
                Server = _server,
                Database = _database,
                UserID = _userId,
                Password = _password,
                Port = (uint)_port,
                SslMode = MySqlSslMode.Disabled,    // MySqlSslMode.Node => Disabled 로 변경
                CharacterSet = "utf8"
            };
        }
        
        /* C# 7.3 호환: 전통적 using 블록 */
        public void TestConnection()
        {
            Console.WriteLine($"[DB] Connection > {GetConnectionString()}");
            using (var conn = new MySqlConnection(GetConnectionString()))
            {
                conn.Open();
                Console.WriteLine($"[DB] 연결 성공 > {conn.ServerVersion}");
            }
        }
    }
}
