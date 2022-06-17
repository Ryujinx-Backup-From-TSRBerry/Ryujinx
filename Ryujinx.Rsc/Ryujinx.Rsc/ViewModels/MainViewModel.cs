using Avalonia.Threading;
using Ryujinx.Ui.App.Common;
using System;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using Ryujinx.Rsc.Views;
using Ryujinx.Ui.Common.Configuration;
using System.Collections.ObjectModel;
using System.Threading;
using Avalonia.Media;
using Ryujinx.Rsc.Models;
using System.Threading.Tasks;

namespace Ryujinx.Rsc.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ObservableCollection<ApplicationData> _applications;
        private ReadOnlyObservableCollection<ApplicationData> _appsObservableList;
        private bool _isLoading;
        private bool _enableVirtualController;
        private string _fifoStatusText;
        private string _gpuStatusText;
        private string _gameStatusText;
        private Brush _vsyncColor;
        private bool _isGameRunning;
        private bool _showOverlay;
        private View _currentView = View.GameList;
        private bool _showTabs = true;
        private float _volume;
        private float _currentVolume;
        private bool _isPaused;

        public MainView Owner { get; set; }

        public MainViewModel()
        {
            Applications = new ObservableCollection<ApplicationData>();

            Applications.ToObservableChangeSet()
                .Bind(out _appsObservableList).AsObservableList();

            _vsyncColor = new SolidColorBrush(Colors.White);
        }

        public ObservableCollection<ApplicationData> Applications
        {
            get => _applications;
            set
            {
                _applications = value;

                this.RaisePropertyChanged();
            }
        }

        public ReadOnlyObservableCollection<ApplicationData> AppsObservableList
        {
            get => _appsObservableList;
            set
            {
                _appsObservableList = value;

                this.RaisePropertyChanged();
            }
        }

        public View CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(IsGameListActive));
                this.RaisePropertyChanged(nameof(IsSettingsActive));
            }
        }

        public bool IsGameListActive => CurrentView == View.GameList;
        public bool IsSettingsActive => CurrentView == View.Settings;

        public bool IsGridSmall => ConfigurationState.Instance.Ui.GridSize == 1;
        public bool IsGridMedium => ConfigurationState.Instance.Ui.GridSize == 2;
        public bool IsGridLarge => ConfigurationState.Instance.Ui.GridSize == 3;
        public bool IsGridHuge => ConfigurationState.Instance.Ui.GridSize == 4;

        public bool IsGameRunning
        {
            get => _isGameRunning; set
            {
                _isGameRunning = value;

                this.RaisePropertyChanged();
            }
        }

        public bool ShowTabs
        {
            get => _showTabs; set
            {
                _showTabs = value;

                this.RaisePropertyChanged();
            }
        }

        public bool ShowOverlay
        {
            get => _showOverlay; set
            {
                _showOverlay = value;

                this.RaisePropertyChanged();
            }
        }

        public bool VolumeMuted => _currentVolume == 0;

        public float Volume
        {
            get => _currentVolume;
            set
            {
                _volume = value;
                _currentVolume = value;

                if (_isGameRunning)
                {
                    Owner.AppHost.Device.SetVolume(_volume);
                }

                this.RaisePropertyChanged(nameof(VolumeMuted));
                this.RaisePropertyChanged();
            }
        }

        public void ToggleMute()
        {
            _currentVolume = _currentVolume == 0 ? _volume : 0;

            if (_isGameRunning)
            {
                Owner.AppHost.Device.SetVolume(_currentVolume);
            }

            this.RaisePropertyChanged(nameof(VolumeMuted));
            this.RaisePropertyChanged(nameof(Volume));

        }

        public string Title { get; set; }
        public bool IsPaused
        {
            get => _isPaused; set
            {
                _isPaused = value;

                this.RaisePropertyChanged(nameof(IsPaused));
            }
        }
        public string TitleName { get; set; }

        public bool EnableVirtualController
        {
            get => _enableVirtualController; set
            {
                _enableVirtualController = value;

                this.RaisePropertyChanged();
            }
        }

        public void Initialize()
        {
            Owner.ApplicationLibrary.ApplicationCountUpdated += ApplicationLibrary_ApplicationCountUpdated;
            Owner.ApplicationLibrary.ApplicationAdded += ApplicationLibrary_ApplicationAdded;

            ReloadGameList();
        }

        public void ToggleVirtualController()
        {
            EnableVirtualController = !EnableVirtualController;
        }

        public void ToggleVSync()
        {
            if (IsGameRunning)
            {
                Owner.AppHost.Device.EnableDeviceVsync = !Owner.AppHost.Device.EnableDeviceVsync;
            }
        }

        public void Pause()
        {
            Task.Run(() =>
            {
                Owner.AppHost.Pause();
            });
        }

        public void Resume()
        {
            Task.Run(() =>
            {
                Owner.AppHost.Resume();
            });
        }

        public void Stop()
        {
            Task.Run(() =>
            {
                Owner.AppHost.Stop();
            });
        }

        private void ApplicationLibrary_ApplicationAdded(object? sender, ApplicationAddedEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Applications.Add(e.AppData);
            });
        }

        public void ReloadGameList()
        {
            if (_isLoading)
            {
                return;
            }

            _isLoading = true;

            Thread thread = new(() =>
            {
                Owner.ApplicationLibrary.LoadApplications(ConfigurationState.Instance.Ui.GameDirs.Value,
                    ConfigurationState.Instance.System.Language);

                _isLoading = false;
            })
            { Name = "GUI.AppListLoadThread", Priority = ThreadPriority.AboveNormal };

            thread.Start();
        }

        private void ApplicationLibrary_ApplicationCountUpdated(object? sender, ApplicationCountUpdatedEventArgs e)
        {
        }

        public string FifoStatusText
        {
            get => _fifoStatusText;
            set
            {
                _fifoStatusText = value;

                this.RaisePropertyChanged();
            }
        }

        public string GpuStatusText
        {
            get => _gpuStatusText;
            set
            {
                _gpuStatusText = value;

                this.RaisePropertyChanged();
            }
        }
        public string GameStatusText
        {
            get => _gameStatusText;
            set
            {
                _gameStatusText = value;

                this.RaisePropertyChanged();
            }
        }

        public Brush VsyncColor
        {
            get => _vsyncColor;
            set
            {
                _vsyncColor = value;

                this.RaisePropertyChanged();
            }
        }
    }
}
