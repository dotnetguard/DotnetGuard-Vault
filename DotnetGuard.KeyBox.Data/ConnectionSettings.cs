namespace DotnetGuard.KeyBox.Data
{
    public class ConnectionSettings
    {
        public string Server { get; set; } = "localhost";
        public string Database { get; set; } = "dotnetguard_keybox";
        public string Username { get; set; } = "root";
        public string Password { get; set; } = "";
    }
}
