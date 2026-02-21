using Microsoft.Data.SqlClient;
using System;

namespace ComputerBuilder.Services
{
    public class DatabaseManager
    {
        private static readonly Lazy<DatabaseManager> _instance =  new Lazy<DatabaseManager>(() => new DatabaseManager());

        public static DatabaseManager Instance => _instance.Value;

        private readonly string _connectionString;

        private const string SERVER_NAME = "(localdb)\\MSSQLLocalDB";
        private const string DATABASE_NAME = "pc_builder_ml";
        private const bool INTEGRATED_SECURITY = true;
        private const bool TRUST_SERVER_CERTIFICATE = true;

        private DatabaseManager()
        {
            _connectionString = $"Server={SERVER_NAME};" +
                                $"Database={DATABASE_NAME};" +
                                $"Integrated Security={INTEGRATED_SECURITY};" +
                                $"TrustServerCertificate={TRUST_SERVER_CERTIFICATE};";
        }

        public string TestConnection()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                Console.WriteLine("Connection successful!");
                return "Connection successful!";
            }
        }

        public string ExecuteQuery(string sql)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);

                connection.Open(); 

                string result = "";

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for(int i = 0; i < reader.FieldCount; i++)
                        {
                            if(i != reader.FieldCount - 1)
                            {
                                if (reader[i].ToString().Length != 0)
                                    result += reader[i].ToString() + " - ";
                                else
                                    result += "NULL - ";
                            } 
                            else
                            {
                                if (reader[i].ToString().Length != 0)
                                    result += reader[i].ToString() + "\n";
                                else
                                    result += "NULL \n";
                            }
                        }
                    }
                }

                return result;
            }
        }
    }
}
