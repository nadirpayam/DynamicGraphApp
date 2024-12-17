using Microsoft.AspNetCore.Mvc;
using DynamicGraphApp.Interfaces;
using DynamicGraphApp.Models;
using System.Data.SqlClient;

namespace DynamicGraphApp.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly IDbConnectionService _dbConnectionService;
        private readonly IDataService _dataService;

        public DataController(IDbConnectionService dbConnectionService, IDataService dataService)
        {
            _dbConnectionService = dbConnectionService;
            _dataService = dataService;
        }
        private string GetConnectionString(DatabaseConnectionInfo dbInfo)
        {
            return _dbConnectionService.GetConnectionString(dbInfo);
        }

        [HttpPost("verifyConnection")]
        public IActionResult VerifyConnection([FromBody] DatabaseConnectionInfo dbInfo)
        {
            string connectionString = GetConnectionString(dbInfo);
            if (_dbConnectionService.TestConnection(connectionString))
            {
                HttpContext.Session.SetString("Server", dbInfo.Server);
                HttpContext.Session.SetString("User", dbInfo.User);
                HttpContext.Session.SetString("Pass", dbInfo.Pass);
                HttpContext.Session.SetString("Database", dbInfo.Database);

                return Ok(new { success = true });
            }

            return BadRequest("Veritabanına bağlanırken hata oluştu.");
        }

        [HttpPost("getData")]
        public IActionResult GetData([FromBody] DataQuery dataQuery)
        {
            string server = HttpContext.Session.GetString("Server");
            string user = HttpContext.Session.GetString("User");
            string pass = HttpContext.Session.GetString("Pass");
            string database = HttpContext.Session.GetString("Database");

            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(database))
            {
                return BadRequest("Connection bilgileri bulunamadı. Lütfen bağlantıyı önce doğrulayın.");
            }

            string connectionString = $"Server={server}; Database={database}; User Id={user}; Password={pass};";
            var data = _dataService.GetData(connectionString, dataQuery);

            return Ok(data);
        }

        [HttpGet("getProcedures")]
        public IActionResult GetProcedures()
        {
            string server = HttpContext.Session.GetString("Server");
            string user = HttpContext.Session.GetString("User");
            string pass = HttpContext.Session.GetString("Pass");
            string database = HttpContext.Session.GetString("Database");

            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(database))
            {
                return BadRequest("Connection bilgileri bulunamadı. Lütfen bağlantıyı önce doğrulayın.");
            }

            string connectionString = $"Server={server}; Database={database}; User Id={user}; Password={pass};";
            var procedures = new List<string>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $"SELECT name AS ProcedureName FROM {database}.sys.procedures";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                procedures.Add(reader["ProcedureName"].ToString());
                            }
                        }
                    }
                }
                return Ok(procedures);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Veritabanı hatası: {ex.Message}");
            }
        }

        [HttpPost("executeProcedure")]
        public IActionResult ExecuteProcedure([FromBody] string procedureName)
        {
            string server = HttpContext.Session.GetString("Server");
            string user = HttpContext.Session.GetString("User");
            string pass = HttpContext.Session.GetString("Pass");
            string database = HttpContext.Session.GetString("Database");

            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(database))
            {
                return BadRequest("Connection bilgileri bulunamadı. Lütfen bağlantıyı önce doğrulayın.");
            }

            string connectionString = $"Server={server}; Database={database}; User Id={user}; Password={pass};";
            var result = new List<dynamic>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand($"EXEC {procedureName}", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(new
                                {
                                    XValue = reader[0], // İlk sütun x değeri
                                    YValue = reader[1]  // İkinci sütun y değeri
                                });
                            }
                        }
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Stored procedure çalıştırılırken hata: {ex.Message}");
            }
        }

        [HttpGet("getViews")]
        public IActionResult GetViews()
        {
            string server = HttpContext.Session.GetString("Server");
            string user = HttpContext.Session.GetString("User");
            string pass = HttpContext.Session.GetString("Pass");
            string database = HttpContext.Session.GetString("Database");

            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(database))
            {
                return BadRequest("Connection bilgileri bulunamadı. Lütfen bağlantıyı önce doğrulayın.");
            }

            string connectionString = $"Server={server}; Database={database}; User Id={user}; Password={pass};";
            var procedures = new List<string>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $"SELECT name AS ViewName FROM {database}.sys.views";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                procedures.Add(reader["ViewName"].ToString());
                            }
                        }
                    }
                }
                return Ok(procedures);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Veritabanı hatası: {ex.Message}");
            }
        }

        [HttpPost("executeViews")]
        public IActionResult ExecuteViews([FromBody] string viewName)
        {
            string server = HttpContext.Session.GetString("Server");
            string user = HttpContext.Session.GetString("User");
            string pass = HttpContext.Session.GetString("Pass");
            string database = HttpContext.Session.GetString("Database");

            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(database))
            {
                return BadRequest("Connection bilgileri bulunamadı. Lütfen bağlantıyı önce doğrulayın.");
            }

            string connectionString = $"Server={server}; Database={database}; User Id={user}; Password={pass};";
            var result = new List<dynamic>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand($"Select * from {viewName}", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(new
                                {
                                    XValue = reader[0], // İlk sütun x değeri
                                    YValue = reader[1]  // İkinci sütun y değeri
                                });
                            }
                        }
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Stored procedure çalıştırılırken hata: {ex.Message}");
            }
        }

    }
}

