using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using DotnetGuard.KeyBox.App.Helpers;
using DotnetGuard.KeyBox.Core.Exceptions;
using DotnetGuard.KeyBox.Core.Models;
using DotnetGuard.KeyBox.Data;
using Microsoft.Win32;

namespace DotnetGuard.KeyBox.App.Views
{
    public partial class MainWindow : Window
    {
        private const int ClipboardClearSeconds = 15;
        private const int IdleLockMinutes = 5;

        private readonly VaultSession _session;
        private readonly DispatcherTimer _clipboardTimer;
        private readonly DispatcherTimer _idleTimer;
        private int _clipboardSecondsRemaining;
        private List<VaultEntry> _allEntries = new List<VaultEntry>();
        private List<CategoryGroup> _categoryGroups = new List<CategoryGroup>();

        public MainWindow(VaultSession session)
        {
            InitializeComponent();
            SourceInitialized += (s, e) => DarkTitleBar.Apply(this);

            _session = session;

            _clipboardTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _clipboardTimer.Tick += ClipboardTimer_Tick;

            _idleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(IdleLockMinutes)
            };
            _idleTimer.Tick += IdleTimer_Tick;
            _idleTimer.Start();

            LoadEntries();
        }

        private void Window_ActivityDetected(object sender, InputEventArgs e)
        {
            _idleTimer.Stop();
            _idleTimer.Start();

            if (e is KeyEventArgs keyEventArgs && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (keyEventArgs.Key == Key.C)
                {
                    CopySelectedPasswordToClipboard();
                    keyEventArgs.Handled = true;
                }
                else if (keyEventArgs.Key == Key.B)
                {
                    CopySelectedUsernameToClipboard();
                    keyEventArgs.Handled = true;
                }
            }
        }

        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            _idleTimer.Stop();
            LockVault();
        }

        private void LoadEntries()
        {
            _allEntries = _session.GetEntries();
            EntriesGrid.ItemsSource = _allEntries;
            BuildCategoryTree(_allEntries);
            UpdateNotesDetail();
        }

        private void BuildCategoryTree(List<VaultEntry> entries)
        {
            _categoryGroups = entries
                .GroupBy(entry => string.IsNullOrWhiteSpace(entry.Category) ? "GENERAL" : entry.Category)
                .OrderBy(group => group.Key)
                .Select(group => new CategoryGroup
                {
                    Name = group.Key,
                    Entries = group.OrderBy(entry => entry.Title).ToList()
                })
                .ToList();

            CategoryTree.ItemsSource = _categoryGroups;
        }

        private void CategoryTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is CategoryGroup group)
            {
                EntriesGrid.ItemsSource = group.Entries;
            }
            else if (e.NewValue is VaultEntry entry)
            {
                CategoryGroup owningGroup = _categoryGroups.FirstOrDefault(g => g.Entries.Contains(entry));
                EntriesGrid.ItemsSource = owningGroup != null ? owningGroup.Entries : _allEntries;
                EntriesGrid.SelectedItem = entry;
                EntriesGrid.ScrollIntoView(entry);
            }

            UpdateNotesDetail();
        }

        private VaultEntry GetSelectedEntry()
        {
            return EntriesGrid.SelectedItem as VaultEntry;
        }

        private void EntriesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateNotesDetail();
        }

        private void EntryRow_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                row.IsSelected = true;
            }
        }

        private void UpdateNotesDetail()
        {
            VaultEntry selected = GetSelectedEntry();
            NotesDetailText.Text = selected != null && !string.IsNullOrWhiteSpace(selected.Notes)
                ? selected.Notes
                : "(no notes)";
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            EntryWindow entryWindow = new EntryWindow(_session, null);

            if (entryWindow.ShowDialog() == true)
            {
                LoadEntries();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            VaultEntry selected = GetSelectedEntry();

            if (selected == null)
            {
                MessageBox.Show("Select an entry first.", "DotnetGuard KeyBox");
                return;
            }

            EntryWindow entryWindow = new EntryWindow(_session, selected);

            if (entryWindow.ShowDialog() == true)
            {
                LoadEntries();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            VaultEntry selected = GetSelectedEntry();

            if (selected == null)
            {
                MessageBox.Show("Select an entry first.", "DotnetGuard KeyBox");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to delete '{selected.Title}'?",
                "DotnetGuard KeyBox", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                _session.DeleteEntry(selected.Id);
                LoadEntries();
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            CopySelectedPasswordToClipboard();
        }

        private void CopySelectedPasswordToClipboard()
        {
            VaultEntry selected = GetSelectedEntry();

            if (selected == null)
            {
                MessageBox.Show("Select an entry first.", "DotnetGuard KeyBox");
                return;
            }

            try
            {
                string plainPassword = _session.RevealPassword(selected);
                Clipboard.SetText(plainPassword);
                StartClipboardCountdown();
            }
            catch (VaultLockedException)
            {
                MessageBox.Show("Vault is locked.", "DotnetGuard KeyBox");
            }
        }

        private void CopySelectedUsernameToClipboard()
        {
            VaultEntry selected = GetSelectedEntry();

            if (selected == null || string.IsNullOrWhiteSpace(selected.EntryUsername))
            {
                return;
            }

            Clipboard.SetText(selected.EntryUsername);
            StartClipboardCountdown();
        }

        private void StartClipboardCountdown()
        {
            _clipboardSecondsRemaining = ClipboardClearSeconds;
            ClipboardStatusText.Visibility = Visibility.Visible;
            UpdateClipboardStatusText();

            _clipboardTimer.Stop();
            _clipboardTimer.Start();
        }

        private void UpdateClipboardStatusText()
        {
            ClipboardStatusText.Text = $"Copied to clipboard — clearing in {_clipboardSecondsRemaining}s";
        }

        private void ClipboardTimer_Tick(object sender, EventArgs e)
        {
            _clipboardSecondsRemaining--;

            if (_clipboardSecondsRemaining <= 0)
            {
                _clipboardTimer.Stop();
                Clipboard.Clear();
                ClipboardStatusText.Visibility = Visibility.Collapsed;
            }
            else
            {
                UpdateClipboardStatusText();
            }
        }

        private void UrlHyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink hyperlink && hyperlink.Tag is string url && !string.IsNullOrWhiteSpace(url))
            {
                string target = url.Contains("://") ? url : "https://" + url;
                Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "DotnetGuard vault export (*.guard)|*.guard",
                FileName = "keybox-export.guard"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                List<VaultExportEntry> entries = _session.ExportEntries();
                string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(dialog.FileName, json);

                MessageBox.Show($"Exported {entries.Count} entries.", "DotnetGuard KeyBox");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed: " + ex.Message, "DotnetGuard KeyBox");
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "DotnetGuard vault export (*.guard)|*.guard"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                string json = File.ReadAllText(dialog.FileName);
                List<VaultExportEntry> entries = JsonSerializer.Deserialize<List<VaultExportEntry>>(json);

                int importedCount = _session.ImportEntries(entries);
                LoadEntries();

                MessageBox.Show($"Imported {importedCount} entries.", "DotnetGuard KeyBox");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Import failed: " + ex.Message, "DotnetGuard KeyBox");
            }
        }

        private void LockButton_Click(object sender, RoutedEventArgs e)
        {
            LockVault();
        }

        private void LockVault()
        {
            _clipboardTimer.Stop();
            _idleTimer.Stop();
            _session.Lock();

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}
