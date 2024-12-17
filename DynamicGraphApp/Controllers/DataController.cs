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

        public DataController(IDbConnectionService dbConnectionService)
        {
            _dbConnectionService = dbConnectionService;
        }

        private string GetConnectionString(DatabaseConnectionInfo dbInfo)
        {
            return _dbConnectionService.GetConnectionString(dbInfo);
        }

        private IActionResult GetDatabaseInfo()
        {
            string server = HttpContext.Session.GetString("Server");
            string user = HttpContext.Session.GetString("User");
            string pass = HttpContext.Session.GetString("Pass");
            string database = HttpContext.Session.GetString("Database");

            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(database))
            {
                return BadRequest("Connection bilgileri bulunamadı. Lütfen bağlantıyı önce doğrulayın.");
            }

            return Ok(new { server, user, pass, database });
        }

        private List<string> ExecuteQuery(string query)
        {
            var result = new List<string>();
            var dbInfoResult = GetDatabaseInfo() as OkObjectResult;

            if (dbInfoResult == null)
                return result;

            var dbInfo = dbInfoResult.Value as dynamic;
            string connectionString = $"Server={dbInfo.server}; Database={dbInfo.database}; User Id={dbInfo.user}; Password={dbInfo.pass};";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(reader[0].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Veritabanı hatası: {ex.Message}");
            }

            return result;
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

        [HttpGet("getProcedures")]
        public IActionResult GetProcedures()
        {
            string query = "SELECT name AS ProcedureName FROM sys.procedures";
            var procedures = ExecuteQuery(query);

            if (procedures.Count == 0)
            {
                return StatusCode(500, "Stored procedure'lar alınırken hata oluştu.");
            }

            return Ok(procedures);
        }

        [HttpPost("executeProcedure")]
        public IActionResult ExecuteProcedure([FromBody] string procedureName)
        {
            string query = $"EXEC {procedureName}";
            var result = new List<dynamic>();

            var dbInfoResult = GetDatabaseInfo() as OkObjectResult;

            if (dbInfoResult == null)
                return BadRequest("Connection bilgileri bulunamadı. Lütfen bağlantıyı önce doğrulayın.");

            var dbInfo = dbInfoResult.Value as dynamic;
            string connectionString = $"Server={dbInfo.server}; Database={dbInfo.database}; User Id={dbInfo.user}; Password={dbInfo.pass};";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(new
                                {
                                    XValue = reader[0], 
                                    YValue = reader[1]  
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
            string query = "SELECT name AS ViewName FROM sys.views";
            var views = ExecuteQuery(query);

            if (views.Count == 0)
            {
                return StatusCode(500, "Views alınırken hata oluştu.");
            }

            return Ok(views);
        }

        [HttpPost("executeViews")]
        public IActionResult ExecuteViews([FromBody] string viewName)
        {
            string query = $"SELECT * FROM {viewName}";
            var result = new List<dynamic>();

            var dbInfoResult = GetDatabaseInfo() as OkObjectResult;

            if (dbInfoResult == null)
                return BadRequest("Connection bilgileri bulunamadı. Lütfen bağlantıyı önce doğrulayın.");

            var dbInfo = dbInfoResult.Value as dynamic;
            string connectionString = $"Server={dbInfo.server}; Database={dbInfo.database}; User Id={dbInfo.user}; Password={dbInfo.pass};";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(new
                                {
                                    XValue = reader[0],
                                    YValue = reader[1]  
                                });
                            }
                        }
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"View çalıştırılırken hata: {ex.Message}");
            }
        }
    }
}

