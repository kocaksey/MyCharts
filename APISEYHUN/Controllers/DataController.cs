using APISEYHUN.DataServices;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;

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
        [HttpGet("GetColumnData")]
        public IActionResult GetColumnData(string tableName, string columnName)
        {
            var connectionString = HttpContext.Session.GetString("ConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest("Connection string is not set.");
            }

            // MySQL'de köşeli parantez kullanmak yerine doğrudan sütun ve tablo adlarını kullanabiliriz
            string query = $"SELECT `{columnName}` FROM `{tableName}`";

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var data = connection.Query(query).ToList();
                return Ok(data);
            }
        }

        [HttpGet("GetColumnValueCounts")]
        public IActionResult GetColumnValueCounts(string tableName, string columnName)
        {
            var connectionString = HttpContext.Session.GetString("ConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest("Connection string is not set.");
            }

            // SQL sorgusu: Her bir farklı sütun değerinin kaç kez tekrarlandığını sayar
            //    string query = $@"
            //SELECT `{columnName}`, COUNT(*) AS Count
            //FROM `{tableName}`
            //GROUP BY `{columnName}`";
            string query = $@"
                SELECT `parentcategories`.`parentcategoryname`, COUNT(*) AS Count
                FROM `{tableName}`
                INNER JOIN `parentcategories` ON `{tableName}`.`{columnName}` = `parentcategories`.`id`
                GROUP BY `parentcategories`.`parentcategoryname`";

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var data = connection.Query(query).ToList();
                return Ok(data);
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







    }
}
