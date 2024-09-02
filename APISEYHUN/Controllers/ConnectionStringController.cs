using APISEYHUN.ConnectionStringClass;
using Microsoft.AspNetCore.Mvc;

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
                return BadRequest("Bağlantı isteği boş olamaz.");
            }

            if (string.IsNullOrEmpty(request.DatabaseType) ||
                string.IsNullOrEmpty(request.ServerName) ||
                string.IsNullOrEmpty(request.DatabaseName))
            {
                return BadRequest("Veritabanı türü, sunucu adı ve veritabanı adı gereklidir.");
            }

            try
            {
                // Dinamik connection string oluşturma
                string connectionString = BuildConnectionString(request);

                // Connection string'i session'da veya memory'de saklayabilirsiniz
                HttpContext.Session.SetString("ConnectionString", connectionString);

                return Ok("Bağlantı adresi başarılı bir şekilde ayarlandı.");
            }
            catch (Exception ex)
            {
                // Hata durumunda uygun mesaj döndürme
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error creating connection string: {ex.Message}");
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
                        return $"Server={request.ServerName};initial Catalog={request.DatabaseName};integrated Security=true;TrustServerCertificate=True;";
                    }
                    else
                    {
                        // SQL Authentication
                        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                        {
                            throw new ArgumentException("SQL Server kimlik doğrulaması için kullanıcı adı ve parola gereklidir.");
                        }
                        return $"Server={request.ServerName};Database={request.DatabaseName};User Id={request.Username};Password={request.Password};integrated Security=True;TrustServerCertificate=True;";
                    }

                case "mysql":
                    if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                    {
                        throw new ArgumentException("MySQL için kullanıcı adı ve şifre gereklidir.");
                    }
                    return $"Server={request.ServerName};Database={request.DatabaseName};Uid={request.Username};Password={request.Password};";

                case "postgresql":
                    if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                    {
                        throw new ArgumentException("PostgreSQL için kullanıcı adı ve şifre gereklidir..");
                    }
                    return $"Host={request.ServerName};Database={request.DatabaseName};Username={request.Username};Password={request.Password};";

                default:
                    throw new NotSupportedException($"Veri tabanı tipi '{request.DatabaseType}' desteklenmemektedir.");
            }
        }
    }


}
