using System;
using System.Collections.Generic;
using DotnetGuard.KeyBox.Core.Models;
using MySqlConnector;

namespace DotnetGuard.KeyBox.Data
{
    public class VaultRepository
    {
        public List<VaultEntry> GetAllForUser(int userId)
        {
            List<VaultEntry> entries = new List<VaultEntry>();

            using (MySqlConnection connection = DbConnectionFactory.CreateConnection())
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand(
                    @"SELECT Id, UserId, Title, IconKey, Category, EntryUsername, EncryptedPassword, Nonce, Tag, Url, Notes, CreatedAt, UpdatedAt
                      FROM VaultEntries WHERE UserId = @UserId ORDER BY Title", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            entries.Add(MapEntry(reader));
                        }
                    }
                }
            }

            return entries;
        }

        public void Add(VaultEntry entry)
        {
            using (MySqlConnection connection = DbConnectionFactory.CreateConnection())
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand(
                    @"INSERT INTO VaultEntries (UserId, Title, IconKey, Category, EntryUsername, EncryptedPassword, Nonce, Tag, Url, Notes)
                      VALUES (@UserId, @Title, @IconKey, @Category, @EntryUsername, @EncryptedPassword, @Nonce, @Tag, @Url, @Notes)", connection))
                {
                    AddEntryParameters(command, entry);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update(VaultEntry entry)
        {
            using (MySqlConnection connection = DbConnectionFactory.CreateConnection())
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand(
                    @"UPDATE VaultEntries
                      SET Title = @Title, IconKey = @IconKey, Category = @Category, EntryUsername = @EntryUsername, EncryptedPassword = @EncryptedPassword,
                          Nonce = @Nonce, Tag = @Tag, Url = @Url, Notes = @Notes, UpdatedAt = UTC_TIMESTAMP()
                      WHERE Id = @Id", connection))
                {
                    AddEntryParameters(command, entry);
                    command.Parameters.AddWithValue("@Id", entry.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (MySqlConnection connection = DbConnectionFactory.CreateConnection())
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand("DELETE FROM VaultEntries WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void AddEntryParameters(MySqlCommand command, VaultEntry entry)
        {
            command.Parameters.AddWithValue("@UserId", entry.UserId);
            command.Parameters.AddWithValue("@Title", entry.Title);
            command.Parameters.AddWithValue("@IconKey", entry.IconKey ?? Core.Models.VaultIcons.Default);
            command.Parameters.AddWithValue("@Category", string.IsNullOrWhiteSpace(entry.Category) ? "GENERAL" : entry.Category.ToUpperInvariant());
            command.Parameters.AddWithValue("@EntryUsername", (object)entry.EntryUsername ?? DBNull.Value);
            command.Parameters.AddWithValue("@EncryptedPassword", entry.EncryptedPassword);
            command.Parameters.AddWithValue("@Nonce", entry.Nonce);
            command.Parameters.AddWithValue("@Tag", entry.Tag);
            command.Parameters.AddWithValue("@Url", (object)entry.Url ?? DBNull.Value);
            command.Parameters.AddWithValue("@Notes", (object)entry.Notes ?? DBNull.Value);
        }

        private static VaultEntry MapEntry(MySqlDataReader reader)
        {
            return new VaultEntry
            {
                Id = reader.GetInt32("Id"),
                UserId = reader.GetInt32("UserId"),
                Title = reader.GetString("Title"),
                IconKey = reader.GetString("IconKey"),
                Category = reader.GetString("Category"),
                EntryUsername = reader.IsDBNull(reader.GetOrdinal("EntryUsername")) ? null : reader.GetString("EntryUsername"),
                EncryptedPassword = (byte[])reader["EncryptedPassword"],
                Nonce = (byte[])reader["Nonce"],
                Tag = (byte[])reader["Tag"],
                Url = reader.IsDBNull(reader.GetOrdinal("Url")) ? null : reader.GetString("Url"),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString("Notes"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime("UpdatedAt")
            };
        }
    }
}
