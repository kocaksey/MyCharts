using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Npgsql;
using System.Collections.Generic;
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

        public async Task<IEnumerable<string>> GetColumnsByDatabase(string tableName)
        {
            var connectionString = _httpContextAccessor.HttpContext.Session.GetString("ConnectionString");


            if (connectionString.Contains("Uid"))
            {
                var builder = new MySqlConnectionStringBuilder(connectionString);
                string databaseName = builder.Database;

                string query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND TABLE_SCHEMA = '{databaseName}'";

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var columns = await connection.QueryAsync<string>(query);
                    return columns.ToList();
                }
            }
            else if (connectionString.Contains("User Id") || connectionString.Contains("Trust"))

            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                string databaseName = builder.InitialCatalog;

                string query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND TABLE_SCHEMA = 'dbo'";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var columns = await connection.QueryAsync<string>(query);
                    return columns.ToList();
                }
            }
            else
            {
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                string databaseName = builder.Database;

                string query = $@"
                SELECT column_name 
                FROM information_schema.columns 
                WHERE table_name = '{tableName}' AND table_schema = 'public'";

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    var columns = await connection.QueryAsync<string>(query);
                    return columns.ToList();
                }
            }
        }
        public async Task<IEnumerable<dynamic>> GetColumnValueCountsByTables(string tableName, string columnName)
        {
            var connectionString = _httpContextAccessor.HttpContext.Session.GetString("ConnectionString");

            if (connectionString.Contains("Uid"))
            {
                string query = $@"
            SELECT `{columnName}`, COUNT(*) AS Count
            FROM `{tableName}`
            GROUP BY `{columnName}`";

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var data = await connection.QueryAsync<dynamic>(query);
                    return data.ToList();
                }
            }
            else if (connectionString.Contains("User Id") || connectionString.Contains("Trust"))
            {
                string query = $@"
            SELECT [{columnName}], COUNT(*) AS Count
            FROM [{tableName}]
            GROUP BY [{columnName}]";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var data = await connection.QueryAsync<dynamic>(query);
                    return data.ToList();
                }
            }
            else
            {
                string query = $@"
            SELECT ""{columnName}"", COUNT(*) AS Count
            FROM ""{tableName}""
            GROUP BY ""{columnName}""";

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    var data = await connection.QueryAsync<dynamic>(query);
                    return data.ToList();
                }
            }
        }

        public async Task<IEnumerable<dynamic>> GetColumnValueJoinCountsByTables(string tableName, string columnName, string joinTable, string joinColumn, string displayColumn)
        {
            var connectionString = _httpContextAccessor.HttpContext.Session.GetString("ConnectionString");

            if (connectionString.Contains("Uid"))
            {
                string query = $@"
                SELECT `{displayColumn}`, COUNT(*) AS Count
                FROM `{tableName}`
                JOIN `{joinTable}` ON `{tableName}`.`{columnName}` = `{joinTable}`.`{joinColumn}`
                GROUP BY `{displayColumn}`";

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var data = await connection.QueryAsync<dynamic>(query);
                    return data.ToList();
                }
            }
            else if (connectionString.Contains("User Id") || connectionString.Contains("Trust"))
            {
                string query = $@"
                SELECT [{displayColumn}], COUNT(*) AS Count
                FROM [{tableName}]
                JOIN [{joinTable}] ON [{tableName}].[{columnName}] = [{joinTable}].[{joinColumn}]
                GROUP BY [{displayColumn}]";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var data = await connection.QueryAsync<dynamic>(query);
                    return data.ToList();
                }

            }
            else
            {
                string query = $@"
                SELECT ""{displayColumn}"", COUNT(*) AS Count
                FROM ""{tableName}""
                JOIN ""{joinTable}"" ON ""{tableName}"".""{columnName}"" = ""{joinTable}"".""{joinColumn}""
                GROUP BY ""{displayColumn}""";

                using (var connection = new NpgsqlConnection(connectionString)) 
                {
                    connection.Open();
                    var data = await connection.QueryAsync<dynamic>(query);
                    return data.ToList();
                }
            }
        }
    }
}
