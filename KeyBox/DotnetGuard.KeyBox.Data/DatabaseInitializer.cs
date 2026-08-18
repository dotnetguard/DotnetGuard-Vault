using MySqlConnector;

namespace DotnetGuard.KeyBox.Data
{
    public static class DatabaseInitializer
    {
        public static void TestServerConnection(ConnectionSettings settings)
        {
            using (MySqlConnection connection = new MySqlConnection(AppSettings.BuildServerOnlyConnectionString(settings)))
            {
                connection.Open();
            }
        }

        public static void EnsureDatabaseAndSchema(ConnectionSettings settings)
        {
            using (MySqlConnection connection = new MySqlConnection(AppSettings.BuildServerOnlyConnectionString(settings)))
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand(
                    $"CREATE DATABASE IF NOT EXISTS `{settings.Database}`", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            using (MySqlConnection connection = new MySqlConnection(AppSettings.BuildConnectionString(settings)))
            {
                connection.Open();

                ExecuteNonQuery(connection, @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id              INT AUTO_INCREMENT PRIMARY KEY,
                        Username        VARCHAR(50) NOT NULL UNIQUE,
                        MasterHash      VARBINARY(64) NOT NULL,
                        Salt            VARBINARY(32) NOT NULL,
                        CreatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                    )");

                ExecuteNonQuery(connection, @"
                    CREATE TABLE IF NOT EXISTS VaultEntries (
                        Id                  INT AUTO_INCREMENT PRIMARY KEY,
                        UserId              INT NOT NULL,
                        Title               VARCHAR(100) NOT NULL,
                        IconKey             VARCHAR(10) NOT NULL DEFAULT 'OTHER',
                        Category            VARCHAR(50) NOT NULL DEFAULT 'GENERAL',
                        EntryUsername       VARCHAR(100),
                        EncryptedPassword   VARBINARY(512) NOT NULL,
                        Nonce               VARBINARY(16) NOT NULL,
                        Tag                 VARBINARY(16) NOT NULL,
                        Url                 VARCHAR(255),
                        Notes               VARCHAR(500),
                        CreatedAt           DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        UpdatedAt           DATETIME NULL,
                        CONSTRAINT FK_VaultEntries_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
                    )");
            }
        }

        private static void ExecuteNonQuery(MySqlConnection connection, string commandText)
        {
            using (MySqlCommand command = new MySqlCommand(commandText, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
