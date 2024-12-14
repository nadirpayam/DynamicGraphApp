using DynamicGraphApp.Controllers;
using DynamicGraphApp.Models;

namespace DynamicGraphApp.Interfaces
{
    public interface IDataService
    {
        List<DataPoint> GetData(string connectionString, DataQuery dataQuery);
    }

}
