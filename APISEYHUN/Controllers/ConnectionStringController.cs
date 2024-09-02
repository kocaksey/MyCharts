using APISEYHUN.ConnectionStringClass;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using System.Data;

namespace APISEYHUN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConnectionStringController : ControllerBase
    {
        [HttpPost("SetConnection")]
        public IActionResult SetConnection([FromBody] ConnectionRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Bağlantı isteği boş olamaz." });
            }

            if (string.IsNullOrEmpty(request.DatabaseType) ||
                string.IsNullOrEmpty(request.ServerName) ||
                string.IsNullOrEmpty(request.DatabaseName))
            {
                return BadRequest(new { success = false, message = "Veritabanı türü, sunucu adı ve veritabanı adı gereklidir." });
            }

            try
            {
                string connectionString = BuildConnectionString(request);

                if (!TestDatabaseConnection(connectionString, request.DatabaseType))
                {
                    return BadRequest(new { success = false, message = "Veritabanı bağlantısı başarısız. Lütfen bilgilerinizi kontrol edin." });
                }

                HttpContext.Session.SetString("ConnectionString", connectionString);

                return Ok(new { success = true, message = "Bağlantı adresi başarılı bir şekilde ayarlandı." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error creating connection string: {ex.Message}" });
            }
        }


        private string BuildConnectionString(ConnectionRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            string databaseType = request.DatabaseType?.ToLower();
            if (string.IsNullOrEmpty(databaseType))
            {
                throw new ArgumentException("Database type is required.", nameof(request.DatabaseType));
            }

            switch (databaseType)
            {
                case "sqlserver":
                    if (string.IsNullOrEmpty(request.Username) && string.IsNullOrEmpty(request.Password))
                    {
                        // Windows Authentication
                        return $"Server={request.ServerName};Initial Catalog={request.DatabaseName};Integrated Security=True;TrustServerCertificate=True;";
                    }
                    else
                    {
                        // SQL Authentication
                        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                        {
                            throw new ArgumentException("SQL Server kimlik doğrulaması için kullanıcı adı ve parola gereklidir.");
                        }
                        return $"Server={request.ServerName};Database={request.DatabaseName};User Id={request.Username};Password={request.Password};TrustServerCertificate=True;";
                    }

                case "mysql":
                    if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                    {
                        throw new ArgumentException("MySQL için kullanıcı adı ve şifre gereklidir.");
                    }
                    return $"Server={request.ServerName};Database={request.DatabaseName};Uid={request.Username};Pwd={request.Password};";

                case "postgresql":
                    if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                    {
                        throw new ArgumentException("PostgreSQL için kullanıcı adı ve şifre gereklidir.");
                    }
                    return $"Host={request.ServerName};Database={request.DatabaseName};Username={request.Username};Password={request.Password};";

                default:
                    throw new NotSupportedException($"Veri tabanı tipi '{request.DatabaseType}' desteklenmemektedir.");
            }
        }

        private bool TestDatabaseConnection(string connectionString, string dbType)
        {
            try
            {
                using IDbConnection connection = dbType.ToLower() switch
                {
                    "sqlserver" => new SqlConnection(connectionString),
                    "mysql" => new MySqlConnection(connectionString),
                    "postgresql" => new NpgsqlConnection(connectionString),
                    _ => throw new NotSupportedException("Database type not supported")
                };
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
