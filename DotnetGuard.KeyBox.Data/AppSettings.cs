using System;
using System.IO;
using System.Text.Json;

namespace DotnetGuard.KeyBox.Data
{
    public static class AppSettings
    {
        private static readonly string SettingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DotnetGuard.KeyBox");

        private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "settings.json");

        public static bool Exists()
        {
            return File.Exists(SettingsFilePath);
        }

        public static ConnectionSettings Load()
        {
            string json = File.ReadAllText(SettingsFilePath);
            return JsonSerializer.Deserialize<ConnectionSettings>(json);
        }

        public static void Save(ConnectionSettings settings)
        {
            Directory.CreateDirectory(SettingsDirectory);

            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
        }

        public static string BuildConnectionString(ConnectionSettings settings)
        {
            return $"Server={settings.Server};Database={settings.Database};User ID={settings.Username};Password={settings.Password};";
        }

        public static string BuildServerOnlyConnectionString(ConnectionSettings settings)
        {
            return $"Server={settings.Server};User ID={settings.Username};Password={settings.Password};";
        }
    }
}
