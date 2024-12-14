using DynamicGraphApp.Controllers;
using DynamicGraphApp.Interfaces;
using DynamicGraphApp.Models;
using System.Data.SqlClient;

namespace DynamicGraphApp.Services
{
    public class DataService : IDataService
    {
        public List<DataPoint> GetData(string connectionString, DataQuery dataQuery)
        {
            var data = new List<DataPoint>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var query = $"SELECT {dataQuery.XColumn}, {dataQuery.YColumn} FROM {dataQuery.TableName}";
                SqlCommand command = new SqlCommand(query, connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data.Add(new DataPoint
                        {
                            XValue = reader[dataQuery.XColumn].ToString(),
                            YValue = reader[dataQuery.YColumn].ToString()
                        });
                    }
                }
            }
            return data;
        }
    }

}
