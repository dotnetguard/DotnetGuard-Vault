using System;
using System.Windows;
using System.Windows.Controls;
using DotnetGuard.KeyBox.Core.Models;
using DotnetGuard.KeyBox.Data;

namespace DotnetGuard.KeyBox.App.Views
{
    public partial class EntryPanel : UserControl
    {
        private VaultSession _session;
        private VaultEntry _existingEntry;

        public event EventHandler Saved;
        public event EventHandler Cancelled;

        public EntryPanel()
        {
            InitializeComponent();
            IconComboBox.ItemsSource = VaultIcons.Keys;
        }

        public void Initialize(VaultSession session)
        {
            _session = session;
        }

        public void ShowForAdd()
        {
            _existingEntry = null;
            ErrorText.Text = string.Empty;

            HeaderText.Text = "> new entry";
            TitleBox.Text = string.Empty;
            IconComboBox.SelectedItem = VaultIcons.Default;
            CategoryBox.Text = "GENERAL";
            EntryUsernameBox.Text = string.Empty;
            PasswordBox.Text = string.Empty;
            UrlBox.Text = string.Empty;
            NotesBox.Text = string.Empty;

            Visibility = Visibility.Visible;
        }

        public void ShowForEdit(VaultEntry entry)
        {
            _existingEntry = entry;
            ErrorText.Text = string.Empty;

            HeaderText.Text = "> edit entry";
            TitleBox.Text = entry.Title;
            IconComboBox.SelectedItem = entry.IconKey;
            CategoryBox.Text = entry.Category;
            EntryUsernameBox.Text = entry.EntryUsername;
            PasswordBox.Text = _session.RevealPassword(entry);
            UrlBox.Text = entry.Url;
            NotesBox.Text = entry.Notes;

            Visibility = Visibility.Visible;
        }

        public void HidePanel()
        {
            Visibility = Visibility.Collapsed;
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

                HidePanel();
                Saved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ErrorText.Text = "Could not save: " + ex.Message;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            HidePanel();
            Cancelled?.Invoke(this, EventArgs.Empty);
        }
    }
}
