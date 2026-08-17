using System;
using DotnetGuard.KeyBox.Core.Models;
using MySqlConnector;

namespace DotnetGuard.KeyBox.Data
{
    public class UserRepository
    {
        public bool AnyUserExists()
        {
            using (MySqlConnection connection = DbConnectionFactory.CreateConnection())
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand("SELECT COUNT(*) FROM Users", connection))
                {
                    long count = (long)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public User GetByUsername(string username)
        {
            using (MySqlConnection connection = DbConnectionFactory.CreateConnection())
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand(
                    "SELECT Id, Username, MasterHash, Salt, CreatedAt FROM Users WHERE Username = @Username", connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapUser(reader);
                        }
                    }
                }
            }

            return null;
        }

        public void CreateUser(User user)
        {
            using (MySqlConnection connection = DbConnectionFactory.CreateConnection())
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand(
                    "INSERT INTO Users (Username, MasterHash, Salt) VALUES (@Username, @MasterHash, @Salt)", connection))
                {
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@MasterHash", user.MasterHash);
                    command.Parameters.AddWithValue("@Salt", user.Salt);

                    command.ExecuteNonQuery();
                }
            }
        }

        private static User MapUser(MySqlDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32("Id"),
                Username = reader.GetString("Username"),
                MasterHash = (byte[])reader["MasterHash"],
                Salt = (byte[])reader["Salt"],
                CreatedAt = reader.GetDateTime("CreatedAt")
            };
        }
    }
}
