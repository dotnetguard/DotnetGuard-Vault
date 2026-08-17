using System;
using System.Windows;
using DotnetGuard.KeyBox.App.Helpers;
using DotnetGuard.KeyBox.Core.Models;
using DotnetGuard.KeyBox.Data;

namespace DotnetGuard.KeyBox.App.Views
{
    public partial class EntryWindow : Window
    {
        private readonly VaultSession _session;
        private readonly VaultEntry _existingEntry;

        public EntryWindow(VaultSession session, VaultEntry existingEntry)
        {
            InitializeComponent();
            SourceInitialized += (s, e) => DarkTitleBar.Apply(this);

            _session = session;
            _existingEntry = existingEntry;

            IconComboBox.ItemsSource = VaultIcons.Keys;

            if (_existingEntry != null)
            {
                Title = "Edit entry";
                TitleBox.Text = _existingEntry.Title;
                IconComboBox.SelectedItem = _existingEntry.IconKey;
                CategoryBox.Text = _existingEntry.Category;
                EntryUsernameBox.Text = _existingEntry.EntryUsername;
                UrlBox.Text = _existingEntry.Url;
                NotesBox.Text = _existingEntry.Notes;
                PasswordBox.Text = _session.RevealPassword(_existingEntry);
            }
            else
            {
                Title = "New entry";
                IconComboBox.SelectedItem = VaultIcons.Default;
                CategoryBox.Text = "GENERAL";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = string.Empty;

            string title = TitleBox.Text.Trim();
            string password = PasswordBox.Text;
            string iconKey = IconComboBox.SelectedItem as string ?? VaultIcons.Default;
            string category = string.IsNullOrWhiteSpace(CategoryBox.Text) ? "GENERAL" : CategoryBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(password))
            {
                ErrorText.Text = "Title and password cannot be empty.";
                return;
            }

            try
            {
                if (_existingEntry == null)
                {
                    _session.AddEntry(title, iconKey, category, EntryUsernameBox.Text.Trim(), password, UrlBox.Text.Trim(), NotesBox.Text.Trim());
                }
                else
                {
                    _session.UpdateEntry(_existingEntry.Id, title, iconKey, category, EntryUsernameBox.Text.Trim(), password, UrlBox.Text.Trim(), NotesBox.Text.Trim());
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ErrorText.Text = "Could not save: " + ex.Message;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
