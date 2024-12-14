using DynamicGraphApp.Controllers;
using DynamicGraphApp.Interfaces;
using DynamicGraphApp.Models;
using System.Data.SqlClient;

namespace DynamicGraphApp.Services
{
    public class DbConnectionService : IDbConnectionService
    {
        public string GetConnectionString(DatabaseConnectionInfo dbInfo)
        {
            return $"Server={dbInfo.Server}; Database={dbInfo.Database}; User Id={dbInfo.User}; Password={dbInfo.Pass};";
        }

        public bool TestConnection(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
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

}
