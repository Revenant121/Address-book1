using System;
using System.Security.Cryptography;
using System.Text;
using Npgsql;

namespace Address_book1
{
    public class AuthService
    {
        private string connectionString = "Host=localhost;Port=5432;Database=AddressbookDB;Username=postgres;Password=QWERTY1221;Client Encoding=UTF8";

        public UserModel AuthenticateUser(string username, string password)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT user_id, username, password_hash, role FROM users WHERE username = @username";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("username", username);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedHash = reader.GetString(2);
                            if (VerifyPassword(password, storedHash))
                            {
                                return new UserModel
                                {
                                    user_id = reader.GetInt32(0),
                                    username = reader.GetString(1),
                                    password_hash = storedHash,
                                    role = reader.GetString(3)
                                };
                            }
                        }
                    }
                }
            }
            return null;
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                string hashString = Convert.ToBase64String(hashBytes);
                return hashString == storedHash;
            }
        }
    }
}
