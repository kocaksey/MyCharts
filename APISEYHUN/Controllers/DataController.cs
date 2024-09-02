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
        //
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
        public async Task<IActionResult> GetColumns(string tableName)
        {
            var connectionString = HttpContext.Session.GetString("ConnectionString");



            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest("Connection string is not set.");
            }

            var result = await _dataService.GetColumnsByDatabase(tableName);
            return Ok(result);





        }


        [HttpGet("GetColumnValueCounts")]
        public async Task<IActionResult> GetColumnValueCounts(string tableName, string columnName)
        {
            var connectionString = HttpContext.Session.GetString("ConnectionString");

            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest("Connection string is not set.");
            }
            var result = await _dataService.GetColumnValueCountsByTables(tableName, columnName);
            return Ok(result);


        }


        [HttpGet("GetColumnValueCountsWithJoin")]
        public async Task<IActionResult> GetColumnValueCountsWithJoin(string tableName, string columnName, string joinTable, string joinColumn, string displayColumn)
        {
            var connectionString = HttpContext.Session.GetString("ConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest("Connection string is not set.");
            }
            var result = await _dataService.GetColumnValueJoinCountsByTables(tableName, columnName, joinTable,  joinColumn,  displayColumn);
            return Ok(result);
            


        }







    }
}
