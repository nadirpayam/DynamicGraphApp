using Microsoft.AspNetCore.Mvc;
using DynamicGraphApp.Interfaces;
using DynamicGraphApp.Models;

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
    }
}

