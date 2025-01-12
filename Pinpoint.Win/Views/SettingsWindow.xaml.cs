﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using NHotkey.Wpf;
using Pinpoint.Core;
using Pinpoint.Win.ViewModels;

namespace Pinpoint.Win.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, IPluginListener<AbstractPlugin, object>
    {
        private readonly PluginEngine _pluginEngine;

        public SettingsWindow(PluginEngine pluginEngine)
        {
            InitializeComponent();

            DataContext = App.Current.SettingsViewModel;
            _pluginEngine = pluginEngine;

            if (AppSettings.TryGet(AppConstants.LocalPluginsDirectoryKey, out string localPluginsDirectory))
            {
                Model.LocalPluginsDirectory = localPluginsDirectory;
            }

            _ = Dispatcher.InvokeAsync(DownloadChangelog);
        }

        public SettingsViewModel Model => (SettingsViewModel)DataContext;

        private async Task DownloadChangelog()
        {
            try
            {
                using var httpClient = new HttpClient();
                var html = await httpClient.GetStringAsync("https://usepinpoint.com/api/github_changelog");
                Dispatcher.Invoke(() => UpdateLog.NavigateToString(html));
            }
            catch (HttpRequestException)
            {
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;

            foreach (var plugin in Model.Plugins)
            {
                plugin.Save();
            }

            AppSettings.PutAndSave(AppConstants.LocalPluginsDirectoryKey, Model.LocalPluginsDirectory);

            Hide();
        }

        private void TxtHotkeyClipboardManager_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            // Get modifiers and key data
            var modifiers = Keyboard.Modifiers;
            var key = e.Key;

            // When Alt is pressed, SystemKey is used instead
            if (key == Key.System)
            {
                key = e.SystemKey;
            }

            // Clear modifier keys
            if (modifiers == ModifierKeys.None && (key == Key.Delete || key == Key.Back || key == Key.Escape))
            {
                Model.HotkeyPasteClipboard = new HotkeyModel(Key.V, ModifierKeys.Control & ModifierKeys.Alt);
                return;
            }

            var invalidKeys = new[]
            {
                Key.LeftCtrl, Key.RightCtrl, Key.LeftAlt, Key.RightAlt, Key.LeftShift, Key.RightShift, Key.LWin,
                Key.RWin, Key.Clear, Key.OemClear, Key.Apps
            };

            if (invalidKeys.Contains(key))
            {
                return;
            }

            // Save and set new hotkey
            Model.HotkeyPasteClipboard = new HotkeyModel(key, modifiers);
            HotkeyManager.Current.AddOrReplace(AppConstants.HotkeyPasteId, Model.HotkeyPasteClipboard.Key, Model.HotkeyPasteClipboard.Modifiers, App.Current.MainWindow.OnSystemClipboardPaste);
            AppSettings.PutAndSave("hotkey_paste_clipboard", Model.HotkeyPasteClipboard);
        }

        private void TxtHotkey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            // Get modifiers and key data
            var modifiers = Keyboard.Modifiers;
            var key = e.Key;

            // When Alt is pressed, SystemKey is used instead
            if (key == Key.System)
            {
                key = e.SystemKey;
            }

            // Clear modifier keys
            if (modifiers == ModifierKeys.None && (key == Key.Delete || key == Key.Back || key == Key.Escape))
            {
                Model.HotkeyToggleVisibility = new HotkeyModel(Key.Space, ModifierKeys.Alt);
                return;
            }

            var invalidKeys = new[]
            {
                Key.LeftCtrl, Key.RightCtrl, Key.LeftAlt, Key.RightAlt, Key.LeftShift, Key.RightShift, Key.LWin,
                Key.RWin, Key.Clear, Key.OemClear, Key.Apps
            };

            if (invalidKeys.Contains(key))
            {
                return;
            }

            // Save and set new hotkey
            Model.HotkeyToggleVisibility = new HotkeyModel(key, modifiers);
            HotkeyManager.Current.AddOrReplace(AppConstants.HotkeyToggleVisibilityId, Model.HotkeyToggleVisibility.Key, Model.HotkeyToggleVisibility.Modifiers, App.Current.MainWindow.OnToggleVisibility);
            AppSettings.PutAndSave("hotkey_toggle_visibility", Model.HotkeyToggleVisibility);
        }

        public void PluginChange_Added(object sender, AbstractPlugin plugin, object target)
        {
            Model.Plugins.Add(plugin);

            // Add to tab control
            var pluginTabItem = new PluginTabItem(plugin);
            if (plugin.Storage.User.Count == 0)
            {
                pluginTabItem.LblSettings.Visibility = pluginTabItem.PluginSettings.Visibility = Visibility.Hidden;
            }

            Model.PluginTabItems.Add(pluginTabItem);
            Model.PluginTabItems = new ObservableCollection<PluginTabItem>(Model.PluginTabItems.OrderBy(p => p.Model.Plugin.Manifest.Name).ToList());
        }

        public void PluginChange_Removed(object sender, AbstractPlugin plugin, object target)
        {
            Model.Plugins.Remove(plugin);

            var pluginTabItem = Model.PluginTabItems.FirstOrDefault(p => p.Model.Plugin == plugin);
            if (pluginTabItem != null)
            {
                Model.PluginTabItems.Remove(pluginTabItem);
            }
        }

        private void BtnToggleStartupLaunch_OnClick(object sender, RoutedEventArgs evt)
        {
            var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            var registryKey = Registry.CurrentUser.OpenSubKey(path, true);
            if (registryKey.GetValue("Pinpoint") == null)
            {
                var applicationExecutablePath = Process.GetCurrentProcess().MainModule.FileName;
                registryKey.SetValue("Pinpoint", applicationExecutablePath);
                MessageBox.Show("Successfully enabled", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                registryKey.DeleteValue("Pinpoint");
                MessageBox.Show("Successfully disabled", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnReCenterWindow_OnClick(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.MoveWindowToDefaultPosition();
            Close();
            App.Current.MainWindow.Show();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Select Folder",
                Filter = "Folders|*.none",
                Title = "Select Local Plugins Directory"
            };

            if (dialog.ShowDialog() == true)
            {
                var selectedPath = Path.GetDirectoryName(dialog.FileName);
                Model.LocalPluginsDirectory = selectedPath;
            }
        }

        private async void ReloadLocalPlugins_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Model.LocalPluginsDirectory))
            {
                MessageBox.Show("Please select a directory first", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Directory.Exists(Model.LocalPluginsDirectory))
            {
                MessageBox.Show("Directory does not exist", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ReloadLocalPlugins.IsEnabled = false;

            for (int i = 0; i < _pluginEngine.LocalPlugins.Count; i++)
            {
                var plugin = _pluginEngine.LocalPlugins[i];
                _pluginEngine.Unload(plugin);
            }

            var exceptions = await _pluginEngine.LoadLocalPlugins(Model.LocalPluginsDirectory);
            if (exceptions.Count > 0)
            {
                var message = string.Join("\n", exceptions.Select(ex => ex.Message));
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                AppSettings.PutAndSave(AppConstants.LocalPluginsDirectoryKey, Model.LocalPluginsDirectory);
            }

            ReloadLocalPlugins.IsEnabled = true;
        }
    }
}
