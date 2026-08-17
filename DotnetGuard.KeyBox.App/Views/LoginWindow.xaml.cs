using System;
using System.Windows;
using DotnetGuard.KeyBox.App.Helpers;
using DotnetGuard.KeyBox.Core.Exceptions;
using DotnetGuard.KeyBox.Data;

namespace DotnetGuard.KeyBox.App.Views
{
    public partial class LoginWindow : Window
    {
        private readonly VaultSession _session = new VaultSession();
        private readonly bool _setupRequired;

        public LoginWindow()
        {
            InitializeComponent();
            SourceInitialized += (s, e) => DarkTitleBar.Apply(this);

            _setupRequired = _session.SetupRequired;

            if (_setupRequired)
            {
                HeaderText.Text = "> set up vault";
                ActionButton.Content = "Create vault";
                ConfirmLabel.Visibility = Visibility.Visible;
                ConfirmPasswordBox.Visibility = Visibility.Visible;
            }
            else
            {
                HeaderText.Text = "> unlock vault";
                ActionButton.Content = "Unlock";
                ConfirmLabel.Visibility = Visibility.Collapsed;
                ConfirmPasswordBox.Visibility = Visibility.Collapsed;
            }
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = string.Empty;

            string username = UsernameBox.Text.Trim();
            string masterPassword = MasterPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(masterPassword))
            {
                ErrorText.Text = "Username and master password cannot be empty.";
                return;
            }

            try
            {
                if (_setupRequired)
                {
                    if (masterPassword != ConfirmPasswordBox.Password)
                    {
                        ErrorText.Text = "Master password confirmation does not match.";
                        return;
                    }

                    _session.Register(username, masterPassword);
                }
                else
                {
                    _session.Unlock(username, masterPassword);
                }

                MainWindow mainWindow = new MainWindow(_session);
                mainWindow.Show();
                Close();
            }
            catch (InvalidMasterPasswordException)
            {
                ErrorText.Text = "Invalid username or master password.";
            }
            catch (Exception ex)
            {
                ErrorText.Text = "An unexpected error occurred: " + ex.Message;
            }
        }
    }
}
