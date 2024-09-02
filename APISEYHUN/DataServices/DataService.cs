using Dapper;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Npgsql;
using System.Data;

namespace APISEYHUN.DataServices
{
    public class DataService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DataService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetData(string tableName, string[] columns)
        {
            var connectionString = _httpContextAccessor.HttpContext.Session.GetString("ConnectionString");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Connection string not found.");
            }

            var databaseType = GetDatabaseType(connectionString);

            using IDbConnection connection = databaseType switch
            {
                "sqlserver" => new SqlConnection(connectionString),
                "mysql" => new MySqlConnection(connectionString),
                "postgresql" => new NpgsqlConnection(connectionString),
                _ => throw new NotSupportedException("Database desteklenmiyor.")
            };

            connection.Open();
            var columnList = string.Join(", ", columns);
            var query = $"SELECT {columnList} FROM {tableName}";

            var result = connection.Query(query).ToList();
            return JsonConvert.SerializeObject(result);
        }

        private string GetDatabaseType(string connectionString)
        {
            if (connectionString.Contains("Server") && connectionString.Contains("User Id"))
            {
                return "sqlserver";
            }
            else if (connectionString.Contains("Server") && connectionString.Contains("TrustServerCertificate"))
            {
                return "sqlserver";
            }
            else if (connectionString.Contains("Server") && connectionString.Contains("Uid"))
            {
                return "mysql";
            }
            else if (connectionString.Contains("Host") && connectionString.Contains("Username"))
            {
                return "postgresql";
            }
            else
            {
                throw new NotSupportedException("Veri tabanı türü belirlenemedi..");
            }
        }
        public List<string> GetTables()
        {
            var connectionString = _httpContextAccessor.HttpContext.Session.GetString("ConnectionString");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Connection string not found.");
            }

            var databaseType = GetDatabaseType(connectionString);

            using IDbConnection connection = databaseType switch
            {
                "sqlserver" => new SqlConnection(connectionString),
                "mysql" => new MySqlConnection(connectionString),
                "postgresql" => new NpgsqlConnection(connectionString),
                _ => throw new NotSupportedException("Database desteklenmiyor.")
            };

            connection.Open();

            var query = databaseType switch
            {
                "sqlserver" => "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'",
                "mysql" => "SHOW TABLES",
                "postgresql" => "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'",
                _ => throw new NotSupportedException("Veri tabanı türü desteklenmiyor.")
            };

            var tables = connection.Query<string>(query).ToList();
            return tables;
        }


    }
}
