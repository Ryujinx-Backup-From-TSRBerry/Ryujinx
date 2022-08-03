using Avalonia;
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
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Common.Ui.Controls;
using Ryujinx.Rsc.Models;
using Silk.NET.Vulkan;
using System.Collections.Generic;
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
        private string _searchText;

        public MainView Owner { get; set; }
        public GamePage GamePage { get; set; }

        public MainViewModel()
        {
            Applications = new ObservableCollection<ApplicationData>();

            Applications.ToObservableChangeSet()
                .Bind(out _appsObservableList).AsObservableList();

            _vsyncColor = new SolidColorBrush(Colors.White);

            if (AppConfig.PreviewerDetached)
            {
                ConfigurationState.Instance.Ui.GridSize.Value = 2;
            }
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

        public bool IsGrid => Glyph == Glyph.Grid;
        public bool IsList => Glyph == Glyph.List;
        
        public Glyph Glyph
        {
            get => (Glyph)ConfigurationState.Instance.Ui.GameListViewMode.Value;
            set
            {
                ConfigurationState.Instance.Ui.GameListViewMode.Value = (int)value;

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(IsGrid));
                this.RaisePropertyChanged(nameof(IsList));

                ConfigurationState.Instance.ToFileFormat().SaveConfig(AppConfig.ConfigurationPath);
            }
        }

        public void SetListMode()
        {
            Glyph = Glyph.List;
        }

        public void SetGridMode()
        {
            Glyph = Glyph.Grid;
        }

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
                    GamePage.AppHost.Device.SetVolume(_volume);
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
                GamePage.AppHost.Device.SetVolume(_currentVolume);
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
                GamePage.AppHost.Device.EnableDeviceVsync = !GamePage.AppHost.Device.EnableDeviceVsync;
            }
        }

        public void Pause()
        {
            Task.Run(() =>
            {
                GamePage.AppHost.Pause();
            });
        }

        public void Resume()
        {
            Task.Run(() =>
            {
                GamePage.AppHost.Resume();
            });
        }

        public void Stop()
        {
            Task.Run(() =>
            {
                GamePage.AppHost.Stop();
            });
        }
        
        public Thickness GridItemPadding => ShowNames ? new Thickness() : new Thickness(5);
        
        public int GridSizeScale
        {
            get => ConfigurationState.Instance.Ui.GridSize;
            set
            {
                ConfigurationState.Instance.Ui.GridSize.Value = value;

                if (value < 2)
                {
                    ShowNames = false;
                }

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(IsGridSmall));
                this.RaisePropertyChanged(nameof(IsGridMedium));
                this.RaisePropertyChanged(nameof(IsGridLarge));
                this.RaisePropertyChanged(nameof(IsGridHuge));
                this.RaisePropertyChanged(nameof(ShowNames));
                this.RaisePropertyChanged(nameof(GridItemPadding));

                ConfigurationState.Instance.ToFileFormat().SaveConfig(AppConfig.ConfigurationPath);
            }
        }
        
        public bool ShowNames
        {
            get => ConfigurationState.Instance.Ui.ShowNames && ConfigurationState.Instance.Ui.GridSize > 1; set
            {
                ConfigurationState.Instance.Ui.ShowNames.Value = value;

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(GridItemPadding));
                this.RaisePropertyChanged(nameof(GridSizeScale));

                ConfigurationState.Instance.ToFileFormat().SaveConfig(AppConfig.ConfigurationPath);
            }
        }

        private void ApplicationLibrary_ApplicationAdded(object? sender, ApplicationAddedEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Applications.Add(e.AppData);
            });
        }

        public void OpenSettings()
        {
            Owner.Navigate(typeof(TestPage), Owner);
        }

        public void ReloadGameList()
        {
            if (_isLoading)
            {
                return;
            }

            _isLoading = true;

            Applications.Clear();

            Thread thread = new(() =>
            {
                Owner.ApplicationLibrary.LoadApplications(ConfigurationState.Instance.Ui.GameDirs.Value,
                    ConfigurationState.Instance.System.Language);

                _isLoading = false;
            })
            { Name = "GUI.AppListLoadThread", Priority = ThreadPriority.AboveNormal };

            thread.Start();
        }
        

        public bool IsSortedByFavorite => SortMode == ApplicationSort.Favorite;
        public bool IsSortedByTitle => SortMode == ApplicationSort.Title;
        public bool IsSortedByDeveloper => SortMode == ApplicationSort.Developer;
        public bool IsSortedByLastPlayed => SortMode == ApplicationSort.LastPlayed;
        public bool IsSortedByTimePlayed => SortMode == ApplicationSort.TotalTimePlayed;
        public bool IsSortedByType => SortMode == ApplicationSort.FileType;
        public bool IsSortedBySize => SortMode == ApplicationSort.FileSize;
        public bool IsSortedByPath => SortMode == ApplicationSort.Path;

        public string SortName
        {
            get
            {
                switch (SortMode)
                {
                    case ApplicationSort.Title:
                        return LocaleManager.Instance["GameListHeaderApplication"];
                    case ApplicationSort.Developer:
                        return LocaleManager.Instance["GameListHeaderDeveloper"];
                    case ApplicationSort.LastPlayed:
                        return LocaleManager.Instance["GameListHeaderLastPlayed"];
                    case ApplicationSort.TotalTimePlayed:
                        return LocaleManager.Instance["GameListHeaderTimePlayed"];
                    case ApplicationSort.FileType:
                        return LocaleManager.Instance["GameListHeaderFileExtension"];
                    case ApplicationSort.FileSize:
                        return LocaleManager.Instance["GameListHeaderFileSize"];
                    case ApplicationSort.Path:
                        return LocaleManager.Instance["GameListHeaderPath"];
                    case ApplicationSort.Favorite:
                        return LocaleManager.Instance["CommonFavorite"];
                }

                return string.Empty;
            }
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

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                RefreshView();
            }
        }

        public void Sort(ApplicationSort sort)
        {
            SortMode = sort;
            RefreshView();
        }

        public ApplicationSort SortMode { get; set; }

        internal void Sort(bool isAscending)
        {
            IsAscending = isAscending;
            RefreshView();
        }

        public bool IsAscending { get; set; }

        public string ApplicationPath { get; set; }

        private void RefreshView()
        {
            Applications.ToObservableChangeSet()
                .Filter(Filter)
                .Sort(GetComparer())
                .Bind(out _appsObservableList).AsObservableList();

            this.RaisePropertyChanged(nameof(AppsObservableList));
        }

        private IComparer<ApplicationData> GetComparer()
        {
            switch (SortMode)
            {
                case ApplicationSort.LastPlayed:
                    return new Ryujinx.Ava.Common.Ui.Models.Generic.LastPlayedSortComparer(IsAscending);
                case ApplicationSort.FileSize:
                    return new Ryujinx.Ava.Common.Ui.Models.Generic.FileSizeSortComparer(IsAscending);
                case ApplicationSort.TotalTimePlayed:
                    return new Ryujinx.Ava.Common.Ui.Models.Generic.TimePlayedSortComparer(IsAscending);
                case ApplicationSort.Title:
                    return IsAscending ? SortExpressionComparer<ApplicationData>.Ascending(app => app.TitleName) : SortExpressionComparer<ApplicationData>.Descending(app => app.TitleName);
                case ApplicationSort.Favorite:
                    return !IsAscending ? SortExpressionComparer<ApplicationData>.Ascending(app => app.Favorite) : SortExpressionComparer<ApplicationData>.Descending(app => app.Favorite);
                case ApplicationSort.Developer:
                    return IsAscending ? SortExpressionComparer<ApplicationData>.Ascending(app => app.Developer) : SortExpressionComparer<ApplicationData>.Descending(app => app.Developer);
                case ApplicationSort.FileType:
                    return IsAscending ? SortExpressionComparer<ApplicationData>.Ascending(app => app.FileExtension) : SortExpressionComparer<ApplicationData>.Descending(app => app.FileExtension);
                case ApplicationSort.Path:
                    return IsAscending ? SortExpressionComparer<ApplicationData>.Ascending(app => app.Path) : SortExpressionComparer<ApplicationData>.Descending(app => app.Path);
                default:
                    return null;
            }
        }
        
        private bool Filter(object arg)
        {
            if (arg is ApplicationData app)
            {
                return string.IsNullOrWhiteSpace(_searchText) || app.TitleName.ToLower().Contains(_searchText.ToLower());
            }

            return false;
        }
    }
}
