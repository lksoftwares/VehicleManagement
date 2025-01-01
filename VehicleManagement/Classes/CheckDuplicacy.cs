using LkDataConnection;
using System.Data;

namespace VehicleManagement.Classes
{
    public class CheckDuplicacy
    {
        private ConnectionClass _connection;

        public CheckDuplicacy(ConnectionClass connection)
        {
            _connection = connection;
        }
        public bool CheckDuplicate(string tableName, string[] fields, string[] values, string idField = null, string idValue = null)
        {
            string conditions = string.Join(" OR ", Enumerable.Range(0, fields.Length)
                .Select(i => $"{fields[i]} = '{values[i]}'"));

            string query = $"SELECT * FROM {tableName} WHERE ({conditions})";

            if (!string.IsNullOrEmpty(idField) && idValue != null)
            {
                query += $" AND {idField} != '{idValue}'";
            }

            DataTable result = _connection.ExecuteQueryWithResult(query);

            return result.Rows.Count > 0;
        }



    }
}
