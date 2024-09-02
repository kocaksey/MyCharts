using APISEYHUN.DataServices;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using Org.BouncyCastle.Tls.Crypto;
using System.Text.RegularExpressions;

namespace APISEYHUN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly DataService _dataService;

        public DataController(DataService dataService)
        {
            _dataService = dataService;
        }

        [HttpGet("GetData")]
        public IActionResult GetData(string tableName, [FromQuery] string[] columns)
        {
            if (string.IsNullOrEmpty(tableName) || columns == null || columns.Length == 0)
            {
                return BadRequest("Tablo ismi ve sütun adı geçerli olmalıdır.");
            }

            try
            {
                var jsonData = _dataService.GetData(tableName, columns);
                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        //sdasasas
        //as
        [HttpGet("GetTables")]
        public IActionResult GetTables()
        {
            try
            {
                var tables = _dataService.GetTables();
                return Ok(tables);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpGet("GetColumns")]
        public IActionResult GetColumns(string tableName)
        {
            var connectionString = HttpContext.Session.GetString("ConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest("Connection string is not set.");
            }

            // MySqlConnectionStringBuilder kullanarak bağlantı dizesinden veritabanı adını alıyoruz
            if (connectionString.Contains("Uid"))
            {
                var builder = new MySqlConnectionStringBuilder(connectionString);
                string databaseName = builder.Database;

                string query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND TABLE_SCHEMA = '{databaseName}'";

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var columns = connection.Query<string>(query).ToList();
                    return Ok(columns);
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
                    var columns = connection.Query<string>(query).ToList();
                    return Ok(columns);
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
                    var columns = connection.Query<string>(query).ToList();
                    return Ok(columns);
                }
            }



        }

        // buraya bakılacak


        //[HttpGet("GetColumnData")]
        //public IActionResult GetColumnData(string tableName, string columnName)
        //{
        //    var connectionString = HttpContext.Session.GetString("ConnectionString");
        //    if (string.IsNullOrEmpty(connectionString))
        //    {
        //        return BadRequest("Connection string is not set.");
        //    }

        //    // MySQL'de köşeli parantez kullanmak yerine doğrudan sütun ve tablo adlarını kullanabiliriz
        //    string query = $"SELECT `{columnName}` FROM `{tableName}`";

        //    using (var connection = new MySqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        var data = connection.Query(query).ToList();
        //        return Ok(data);
        //    }
        //}

        [HttpGet("GetColumnValueCounts")]
        public IActionResult GetColumnValueCounts(string tableName, string columnName)
        {
            var connectionString = HttpContext.Session.GetString("ConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest("Connection string is not set.");
            }

            if (connectionString.Contains("Uid"))
            {
                string query = $@"
            SELECT `{columnName}`, COUNT(*) AS Count
            FROM `{tableName}`
            GROUP BY `{columnName}`";

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var data = connection.Query(query).ToList();
                    return Ok(data);
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
                    var data = connection.Query(query).ToList();
                    return Ok(data);
                }
            } else
            {
                string query = $@"
                SELECT ""{columnName}"", COUNT(*) AS Count
                FROM ""{tableName}""
                GROUP BY ""{columnName}""";

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    var data = connection.Query(query).ToList();
                    return Ok(data);
                }

            }

        }


        [HttpGet("GetColumnValueCountsWithJoin")]
        public IActionResult GetColumnValueCountsWithJoin(string tableName, string columnName, string joinTable, string joinColumn, string displayColumn)
        {
            var connectionString = HttpContext.Session.GetString("ConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest("Connection string is not set.");
            }

            if (connectionString.Contains("Uid"))
            {
                // SQL sorgusu: İlgili tabloyu ve ilişkili tabloyu birleştirip, belirli bir sütuna göre gruplama yaparak değer sayısını alır.
                string query = $@"
                SELECT `{displayColumn}`, COUNT(*) AS Count
                FROM `{tableName}`
                JOIN `{joinTable}` ON `{tableName}`.`{columnName}` = `{joinTable}`.`{joinColumn}`
                GROUP BY `{displayColumn}`";

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var data = connection.Query(query).ToList();
                    return Ok(data);
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
                    var data = connection.Query(query).ToList();
                    return Ok(data);
                }

            }else
            {
                string query = $@"
                SELECT ""{displayColumn}"", COUNT(*) AS Count
                FROM ""{tableName}""
                JOIN ""{joinTable}"" ON ""{tableName}"".""{columnName}"" = ""{joinTable}"".""{joinColumn}""
                GROUP BY ""{displayColumn}""";

                using (var connection = new NpgsqlConnection(connectionString)) // PostgreSQL için NpgsqlConnection kullanılır
                {
                    connection.Open();
                    var data = connection.Query(query).ToList();
                    return Ok(data);
                }


            }


        }







    }
}
