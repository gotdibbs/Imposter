using Fiddler;
using MahApps.Metro.Controls;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Imposter.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Imposter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private ImposterSettings _settings = null;
        private Profile _currentProfile = null;

        private bool _isRunning = false;
        private FileSystemWatcher _watcher = null;
        private bool _hasChanges = false;

        private ObservableCollection<Match> _matches = new ObservableCollection<Match>();

        public ObservableCollection<Match> Matches
        {
            get { return _matches; }
            set { _matches = value; }
        }

        public MainWindow()
        {
            InitializeComponent();

            // Initialize collections
            RecentMatches.ItemsSource = Matches;

            // Set default enabled/disabled state for controls
            ToggleFields();

            // Attach Handlers
            ToggleProxy.Click += ToggleProxy_Clicked;
            EditProfile.Click += EditProfile_Click;
            Profiles.SelectionChanged += Profiles_SelectionChanged;
            AutoReload.Click += AutoReload_Checked;
            FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
            FiddlerApplication.BeforeResponse += FiddlerApplication_BeforeResponse;

            LoadProfiles();
        }

        #region UI Event Handlers

        private void ToggleProxy_Clicked(object sender, RoutedEventArgs e)
        {
            if (!_isRunning)
            {
                StartFiddler();
            }
            else
            {
                StopFiddler();
            }
        }

        private void Profiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentProfile = Profiles.SelectedItem as Model.Profile;
        }

        private void AutoReload_Checked(object sender, RoutedEventArgs e)
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = AutoReload.IsChecked != null ? AutoReload.IsChecked.Value : false;
            }
        }

        private void EditProfile_Click(object sender, RoutedEventArgs e)
        {
            var profile = Profiles.SelectedItem as Profile;
            if (profile != null)
            {
                var dialog = new ProfileEditor(profile);
                var result = dialog.ShowDialog();

                if (result != null && result.Value == true)
                {
                    _settings.Profiles.Remove(profile);
                    _settings.Profiles.Add(dialog.Profile);
                    _settings.Save();
                    LoadProfiles();
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_isRunning)
            {
                FiddlerApplication.oProxy.Detach();
                StopFiddler();
            }
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
            }
            base.OnClosed(e);
        }

        #endregion UI Event Handlers

        #region Proxy Logic

        public void StartFiddler()
        {
            if (_currentProfile == null)
            {
                MessageBox.Show("In order to start the proxy, you must first select a profile.");
                return;
            }
            if (_currentProfile.Name == Profile.DefaultName)
            {
                MessageBox.Show("You must first edit and save this profile before being able to start the proxy.");
                return;
            }
            if (!Directory.Exists(_currentProfile.LocalDirectory))
            {
                MessageBox.Show(string.Format("The folder located at '{0}' does not exist. Please correct this error before continuing.", _currentProfile.LocalDirectory));
                return;
            }

            RecentMatchesBox.Visibility = Visibility.Visible;
            Matches.Clear();

            _isRunning = true;
            ToggleFields();

            FiddlerApplication.Startup(_currentProfile.Port, true, _currentProfile.DecryptSsl);

            if (_watcher == null)
            {
                _watcher = new FileSystemWatcher();
                _watcher.IncludeSubdirectories = true;
                _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.DirectoryName | NotifyFilters.CreationTime;
                _watcher.Filter = "*.*";
                _watcher.Changed += FileWatchUpdate;
                _watcher.Created += FileWatchUpdate;
                _watcher.Deleted += FileWatchUpdate;
                _watcher.Renamed += FileWatchUpdate;
            }
            _watcher.Path = _currentProfile.LocalDirectory;
            _watcher.EnableRaisingEvents = AutoReload.IsChecked != null ? AutoReload.IsChecked.Value : false;
        }

        public void StopFiddler()
        {
            RecentMatchesBox.Visibility = Visibility.Collapsed;
            _isRunning = false;
            ToggleFields();

            FiddlerApplication.Shutdown();

            _watcher.EnableRaisingEvents = false;
        }

        #endregion Proxy Logic

        #region Helpers

        private void LoadProfiles()
        {
            // Load settings
            _settings = ImposterSettings.Load();
            // Insert default 'new profile'
            _settings.Profiles.Insert(0, new Profile
            {
                Name = Profile.DefaultName,
                RemoteUrl = string.Empty,
                LocalDirectory = string.Empty,
                Port = 8877,
                DecryptSsl = false,
                Overrides = new List<Override>()
            });
            Profiles.ItemsSource = _settings.Profiles;
            if (_settings != null && _settings.Profiles.Count > 0)
            {
                Profiles.SelectedIndex = _settings.Profiles.Count == 1 ? 0 : _settings.Profiles.Count - 1;
            }
        }

        private string GetStringAfterSubString(string fullString, string subString)
        {
            int index = fullString.IndexOf(subString);
            return fullString.Substring(index + subString.Length);
        }

        private string GetLocalFilePath(string urlFragment)
        {
            var path = _currentProfile.LocalDirectory + @"\" + urlFragment.Replace("/", @"\");

            if (File.Exists(path))
            {
                return path;
            }

            foreach (var ovr in _currentProfile.Overrides)
            {
                if (urlFragment.Contains(ovr.RemoteFile.ToLower()) && File.Exists(ovr.LocalFile))
                {
                    return ovr.LocalFile;
                }
            }

            return string.Empty;
        }

        private void ToggleFields()
        {
            ToggleProxy.Content = _isRunning ? "Stop" : "Start";
            Profiles.IsEnabled = !_isRunning;
            EditProfile.IsEnabled = !_isRunning;
        }

        #endregion Helpers

        private void FiddlerApplication_BeforeRequest(Session oSession)
        {
            string fullString = oSession.fullUrl.ToLower();
            if (fullString.Contains(_currentProfile.RemoteUrl.ToLower()))
            {
                fullString = GetStringAfterSubString(fullString, _currentProfile.RemoteUrl.ToLower()).Split(new char[] { '?' })[0];
                string path = GetLocalFilePath(fullString);
                if (!string.IsNullOrEmpty(path))
                {
                    oSession.utilCreateResponseAndBypassServer();
                    oSession.LoadResponseFromFile(path);

                    PushItem(path);
                }
            }
            if (fullString.EndsWith("imposter.js"))
            {
                oSession.utilCreateResponseAndBypassServer();
                var js = Path.GetFullPath("js\\imposter.js");
                oSession.LoadResponseFromFile(js);
            }
            if (fullString.Contains("/imposter-poll-for-changes"))
            {
                oSession.utilCreateResponseAndBypassServer();
                if (_hasChanges)
                {
                    oSession.utilSetResponseBody("true");
                    _hasChanges = false;
                }
                else
                {
                    oSession.utilSetResponseBody("false");
                }
            }
        }

        private void FiddlerApplication_BeforeResponse(Session oSession)
        {
            var fullString = oSession.fullUrl.ToLower();
            if (fullString.Contains(_currentProfile.RemoteUrl.ToLower()))
            {
                oSession.utilDecodeResponse();
                bool replaced = oSession.utilReplaceInResponse("</body>", "<script type='text/javascript' src='imposter.js'></script></body>");
            }
        }

        private void PushItem(string path)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                lock (Matches)
                {
                    if (Matches.Count > 200)
                    {
                        Matches.RemoveAt(200);
                    }

                    Matches.Insert(0, new Match { FileName = Path.GetFileName(path) });
                }
            }));
        }

        private void FileWatchUpdate(object sender, FileSystemEventArgs e)
        {
            _hasChanges = true;
        }
    }
}
