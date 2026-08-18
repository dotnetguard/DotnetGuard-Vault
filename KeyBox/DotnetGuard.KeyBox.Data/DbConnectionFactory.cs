using MySqlConnector;

namespace DotnetGuard.KeyBox.Data
{
    internal static class DbConnectionFactory
    {
        public static MySqlConnection CreateConnection()
        {
            ConnectionSettings settings = AppSettings.Load();
            return new MySqlConnection(AppSettings.BuildConnectionString(settings));
        }
    }
}
