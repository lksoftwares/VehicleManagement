using LkDataConnection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;


namespace VehicleManagement.Classes
{
    public class ConnectionClass
    {
        private string _ConnectionString;

        private SqlConnection _connection;
        public ConnectionClass(IConfiguration configuration)
        {
           
           EncryptDecrypt _lkencr = new EncryptDecrypt();


            string encryptedServer = configuration.GetConnectionString("server");
            string encryptedUser = configuration.GetConnectionString("user");
            string encryptedPassword = configuration.GetConnectionString("password");
            string encryptedDatabase = configuration.GetConnectionString("database");

            string decryptedServer = _lkencr.Decrypt("ABC", encryptedServer);
            string decryptedUser = _lkencr.Decrypt("ABC", encryptedUser);
            string decryptedPassword = _lkencr.Decrypt("ABC", encryptedPassword);
            string decryptedDatabase = _lkencr.Decrypt("ABC", encryptedDatabase);

            string connectionString = configuration.GetConnectionString("dbcs");
            _ConnectionString = connectionString
                .Replace("$server", decryptedServer)
                .Replace("$user", decryptedUser)
                .Replace("$password", decryptedPassword)
                .Replace("$database", decryptedDatabase);
            _connection = new SqlConnection(_ConnectionString);
          


        }
        public SqlConnection GetSqlConnection()
        {
            return _connection;
        }
        public DataTable ExecuteQueryWithResult(string query)
        {
            using (SqlCommand command = new SqlCommand(query, _connection))
            {
                if (_connection.State != ConnectionState.Open)
                {
                    try
                    {
                        _connection.Open();
                    }
                    catch (Exception ex)
                    {

                    }
                }
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }
        public object ExecuteScalar(string query)
        {
            {
                SqlCommand command = new SqlCommand(query, _connection);
                _connection.Open();
                return command.ExecuteScalar();
            }
        }
    }
}
