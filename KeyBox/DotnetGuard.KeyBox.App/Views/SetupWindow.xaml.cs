using System;
using System.Windows;
using DotnetGuard.KeyBox.App.Helpers;
using DotnetGuard.KeyBox.Data;
using MySqlConnector;

namespace DotnetGuard.KeyBox.App.Views
{
    public partial class SetupWindow : Window
    {
        public SetupWindow()
        {
            InitializeComponent();
            SourceInitialized += (s, e) => DarkTitleBar.Apply(this);
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = string.Empty;

            ConnectionSettings settings = new ConnectionSettings
            {
                Server = ServerBox.Text.Trim(),
                Database = DatabaseBox.Text.Trim(),
                Username = UsernameBox.Text.Trim(),
                Password = PasswordBox.Password
            };

            if (string.IsNullOrWhiteSpace(settings.Server) || string.IsNullOrWhiteSpace(settings.Database)
                || string.IsNullOrWhiteSpace(settings.Username))
            {
                ErrorText.Text = "Server, database name and username cannot be empty.";
                return;
            }

            ConnectButton.IsEnabled = false;

            try
            {
                DatabaseInitializer.TestServerConnection(settings);
                DatabaseInitializer.EnsureDatabaseAndSchema(settings);
                AppSettings.Save(settings);

                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
            catch (MySqlException ex)
            {
                ErrorText.Text = "Could not connect to MySQL: " + ex.Message;
            }
            catch (Exception ex)
            {
                ErrorText.Text = "Setup failed: " + ex.Message;
            }
            finally
            {
                ConnectButton.IsEnabled = true;
            }
        }
    }
}
