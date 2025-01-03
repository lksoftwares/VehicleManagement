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
        public DataTable ExecuteQueryWithResult(string query, IDictionary<string, object> sqlParam = null)
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
                        throw new Exception("Database connection failed.", ex);
                    }
                }

                if (sqlParam != null)
                {
                    foreach (var parameter in sqlParam)
                    {
                        command.Parameters.AddWithValue(parameter.Key, parameter.Value);
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
                if (_connection.State != ConnectionState.Open)
                {
                    try
                    {
                        _connection.Open();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Database connection failed.", ex);
                    }
                }
                return command.ExecuteScalar();

            }
        }
        public string GetOldImagePathFromDatabase(int User_ID)
        {
            string oldImagePath = null;

            try
            {
                string query = "SELECT image FROM User_Mst WHERE User_Id = @User_ID";
             
                using (SqlCommand command = new SqlCommand(query, _connection))
                {
                    _connection.Open();

                    command.Parameters.AddWithValue("@User_ID", User_ID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oldImagePath = reader["image"].ToString();
                            Console.WriteLine(oldImagePath);
                        }
                    }
                    _connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving old image path from database: {ex.Message}");

            }

            return oldImagePath;
        }
    }
}
