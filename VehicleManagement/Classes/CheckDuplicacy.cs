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
      
        public bool CheckDuplicate(CheckDuplicacyPerameter duplicacyPerameter)
        {
            string conditions = string.Join(" OR ", Enumerable.Range(0, duplicacyPerameter.fields.Length)
                .Select(i => $"{duplicacyPerameter.fields[i]} = '{duplicacyPerameter.values[i]}'"));

            string query = $"SELECT * FROM {duplicacyPerameter.tableName} WHERE ({conditions})";

            if (!string.IsNullOrEmpty(duplicacyPerameter.idField) && duplicacyPerameter.idValue != null)
            {
                query += $" AND {duplicacyPerameter.idField} != '{duplicacyPerameter.idValue}'";
            }

            DataTable result = _connection.ExecuteQueryWithResult(query);

            return result.Rows.Count > 0;
        }



    }
}
