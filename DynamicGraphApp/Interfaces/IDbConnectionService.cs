using DynamicGraphApp.Controllers;
using DynamicGraphApp.Models;

namespace DynamicGraphApp.Interfaces
{
    public interface IDbConnectionService
    {
        string GetConnectionString(DatabaseConnectionInfo dbInfo);
        bool TestConnection(string connectionString);
    }
}
