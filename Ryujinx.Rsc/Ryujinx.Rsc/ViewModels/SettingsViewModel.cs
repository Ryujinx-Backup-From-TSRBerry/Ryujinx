using Avalonia.Collections;
using LibHac.Tools.FsSystem;
using ReactiveUI;
using Ryujinx.Ava.Common;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.GraphicsDriver;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS.Services.Time.TimeZone;
using Ryujinx.Rsc.Models;
using Ryujinx.Rsc.Views;
using Ryujinx.Rsc.Views.SettingPages;
using Ryujinx.Ui.Common.Configuration;
using Ryujinx.Ui.Common.Configuration.System;
using System;
using System.Collections.Generic;

namespace Ryujinx.Rsc.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsView _owner;
        private string _title;
        private bool _showToolbar = true;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;

                this.RaisePropertyChanged();
            }
        }

        public bool HasBackStack
        {
            get => _owner != null && _owner.Pages.Count > 1;
            set => throw new NotImplementedException();
        }

        public SettingsViewModel()
        {
            TimeZones = new AvaloniaList<Ryujinx.Ava.Common.Ui.Models.TimeZone>();
            _validTzRegions = new List<string>();
            GameDirectories = new AvaloniaList<string>();

            if (AppConfig.PreviewerDetached)
            {
                LoadCurrentConfiguration();
            }
        }

        public void MoveBack()
        {
            _owner.MoveBack();
        }

        public SettingsViewModel(VirtualFileSystem virtualFileSystem, ContentManager contentManager, SettingsView owner)
        {
            _virtualFileSystem = virtualFileSystem;
            _contentManager = contentManager;
            _owner = owner;
            TimeZones = new AvaloniaList<Ryujinx.Ava.Common.Ui.Models.TimeZone>();
            _validTzRegions = new List<string>();
            GameDirectories = new AvaloniaList<string>();

            if (AppConfig.PreviewerDetached)
            {
                LoadCurrentConfiguration();
            }
        }

        public void NotifyPageChanged()
        {
            this.RaisePropertyChanged(nameof(HasBackStack));
        }

        public void MoveToPage(SettingPages page)
        {
            switch (page)
            {
                case SettingPages.General:
                    _owner.NavigateToPage(new GeneralPage(this));
                    break;
                case SettingPages.Graphics:
                    _owner.NavigateToPage(new GraphicsPage(this));
                    break;
                case SettingPages.System:
                    _owner.NavigateToPage(new SystemPage(this));
                    break;
                case SettingPages.Log:
                    _owner.NavigateToPage(new LoggingPage(this));
                    break;
                case SettingPages.Input:
                    _owner.NavigateToPage(new InputPage(this));
                    break;
                case SettingPages.VirtualController:
                    _owner.NavigateToPage(new VirtualControllerPage(this));
                    break;
            }
        }

        private readonly VirtualFileSystem _virtualFileSystem;
        private readonly ContentManager _contentManager;
        private TimeZoneContentManager _timeZoneContentManager;

        private readonly List<string> _validTzRegions;

        private float _customResolutionScale;
        private int _resolutionScale;
        private int _graphicsBackendMultithreadingIndex;
        private float _previousVolumeLevel;
        private float _volume;

        public int ResolutionScale
        {
            get => _resolutionScale;
            set
            {
                _resolutionScale = value;

                this.RaisePropertyChanged(nameof(CustomResolutionScale));
                this.RaisePropertyChanged(nameof(IsResolutionScaleActive));
            }
        }
        public int GraphicsBackendMultithreadingIndex
        {
            get => _graphicsBackendMultithreadingIndex;
            set
            {
                _graphicsBackendMultithreadingIndex = value;

                this.RaisePropertyChanged();
            }
        }

        public float CustomResolutionScale
        {
            get => _customResolutionScale;
            set
            {
                _customResolutionScale = MathF.Round(value, 1);

                this.RaisePropertyChanged();
            }
        }

        public bool HideCursorOnIdle { get; set; }
        public bool EnableDockedMode { get; set; }
        public bool EnableVsync { get; set; }
        public bool EnablePptc { get; set; }
        public bool EnableInternetAccess { get; set; }
        public bool EnableFsIntegrityChecks { get; set; }
        public bool PreferNativeCodeExecution { get; set; }
        public bool IgnoreMissingServices { get; set; }
        public bool ExpandDramSize { get; set; }
        public bool EnableShaderCache { get; set; }
        public bool EnableFileLog { get; set; }
        public bool EnableStub { get; set; }
        public bool EnableInfo { get; set; }
        public bool EnableWarn { get; set; }
        public bool EnableError { get; set; }
        public bool EnableTrace { get; set; }
        public bool EnableGuest { get; set; }
        public bool EnableFsAccessLog { get; set; }
        public bool EnableDebug { get; set; }
        public AvaloniaList<string> GameDirectories { get; set; }
        public bool IsResolutionScaleActive
        {
            get => _resolutionScale == 0;
            set => throw new NotImplementedException();
        }

        public string TimeZone { get; set; }

        public int Language { get; set; }
        public int Region { get; set; }
        public int FsGlobalAccessLogMode { get; set; }
        public int MaxAnisotropy { get; set; }
        public int AspectRatio { get; set; }
        public int OpenglDebugLevel { get; set; }
        public int MemoryMode { get; set; }

        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;

                ConfigurationState.Instance.System.AudioVolume.Value = (float)(_volume / 100);

                this.RaisePropertyChanged();
            }
        }

        public bool ShowMainViewTabs
        {
            get => _owner.Owner.ViewModel.ShowTabs;
            set => _owner.Owner.ViewModel.ShowTabs = value;
        }

        public DateTimeOffset DateOffset { get; set; }
        public TimeSpan TimeOffset { get; set; }
        public AvaloniaList<Ava.Common.Ui.Models.TimeZone> TimeZones { get; set; }

        public bool ShowToolbar
        {
            get => _showToolbar;
            set
            {
                _showToolbar = value;
                
                this.RaisePropertyChanged();
            }
        }

        public void LoadTimeZones()
        {
            _timeZoneContentManager = new TimeZoneContentManager();

            _timeZoneContentManager.InitializeInstance(_virtualFileSystem, _contentManager, IntegrityCheckLevel.None);

            foreach ((int offset, string location, string abbr) in _timeZoneContentManager.ParseTzOffsets())
            {
                int hours = Math.DivRem(offset, 3600, out int seconds);
                int minutes = Math.Abs(seconds) / 60;

                string abbr2 = abbr.StartsWith('+') || abbr.StartsWith('-') ? string.Empty : abbr;

                TimeZones.Add(new Ava.Common.Ui.Models.TimeZone($"UTC{hours:+0#;-0#;+00}:{minutes:D2}", location, abbr2));

                _validTzRegions.Add(location);
            }
        }

        public void ValidateAndSetTimeZone(string location)
        {
            if (_validTzRegions.Contains(location))
            {
                TimeZone = location;

                this.RaisePropertyChanged(nameof(TimeZone));
            }
        }

        public void LoadCurrentConfiguration()
        {
            ConfigurationState config = ConfigurationState.Instance;

            GameDirectories.Clear();
            GameDirectories.AddRange(config.Ui.GameDirs.Value);

            HideCursorOnIdle = config.HideCursorOnIdle;
            EnableDockedMode = config.System.EnableDockedMode;
            EnableVsync = config.Graphics.EnableVsync;
            EnablePptc = config.System.EnablePtc;
            EnableInternetAccess = config.System.EnableInternetAccess;
            EnableFsIntegrityChecks = config.System.EnableFsIntegrityChecks;
            PreferNativeCodeExecution = config.System.PreferNativeExecution;
            IgnoreMissingServices = config.System.IgnoreMissingServices;
            ExpandDramSize = config.System.ExpandRam;
            EnableShaderCache = config.Graphics.EnableShaderCache;
            EnableFileLog = config.Logger.EnableFileLog;
            EnableStub = config.Logger.EnableStub;
            EnableInfo = config.Logger.EnableInfo;
            EnableWarn = config.Logger.EnableWarn;
            EnableError = config.Logger.EnableError;
            EnableTrace = config.Logger.EnableTrace;
            EnableGuest = config.Logger.EnableGuest;
            EnableDebug = config.Logger.EnableDebug;
            EnableFsAccessLog = config.Logger.EnableFsAccessLog;
            Volume = config.System.AudioVolume * 100;

            GraphicsBackendMultithreadingIndex = (int)config.Graphics.BackendThreading.Value;

            OpenglDebugLevel = (int)config.Logger.GraphicsDebugLevel.Value;

            TimeZone = config.System.TimeZone;

            Language = (int)config.System.Language.Value;
            Region = (int)config.System.Region.Value;
            FsGlobalAccessLogMode = config.System.FsGlobalAccessLogMode;
            MemoryMode = (int)config.System.MemoryManagerMode.Value;

            float anisotropy = config.Graphics.MaxAnisotropy;

            MaxAnisotropy = anisotropy == -1 ? 0 : (int)(MathF.Log2(anisotropy));
            AspectRatio = (int)config.Graphics.AspectRatio.Value;

            int resolution = config.Graphics.ResScale;

            ResolutionScale = resolution == -1 ? 0 : resolution;
            CustomResolutionScale = config.Graphics.ResScaleCustom;

            DateTime dateTimeOffset = DateTime.Now.AddSeconds(config.System.SystemTimeOffset);

            DateOffset = dateTimeOffset.Date;
            TimeOffset = dateTimeOffset.TimeOfDay;

            _previousVolumeLevel = Volume;
        }

        public void SaveSettings()
        {
            List<string> gameDirs = new List<string>(GameDirectories);

            ConfigurationState config = ConfigurationState.Instance;

            if (_validTzRegions.Contains(TimeZone))
            {
                config.System.TimeZone.Value = TimeZone;
            }

            config.Logger.EnableError.Value = EnableError;
            config.Logger.EnableTrace.Value = EnableTrace;
            config.Logger.EnableWarn.Value = EnableWarn;
            config.Logger.EnableInfo.Value = EnableInfo;
            config.Logger.EnableStub.Value = EnableStub;
            config.Logger.EnableDebug.Value = EnableDebug;
            config.Logger.EnableGuest.Value = EnableGuest;
            config.Logger.EnableFsAccessLog.Value = EnableFsAccessLog;
            config.Logger.EnableFileLog.Value = EnableFileLog;
            config.Logger.GraphicsDebugLevel.Value = (GraphicsDebugLevel)OpenglDebugLevel;
            config.System.EnableDockedMode.Value = EnableDockedMode;
            config.HideCursorOnIdle.Value = HideCursorOnIdle;
            config.Graphics.EnableVsync.Value = EnableVsync;
            config.Graphics.EnableShaderCache.Value = EnableShaderCache;
            config.System.EnablePtc.Value = EnablePptc;
            config.System.EnableInternetAccess.Value = EnableInternetAccess;
            config.System.EnableFsIntegrityChecks.Value = EnableFsIntegrityChecks;
            config.System.PreferNativeExecution.Value = PreferNativeCodeExecution;
            config.System.IgnoreMissingServices.Value = IgnoreMissingServices;
            config.System.ExpandRam.Value = ExpandDramSize;
            config.System.Language.Value = (Language)Language;
            config.System.Region.Value = (Region)Region;

            if (ConfigurationState.Instance.Graphics.BackendThreading != (BackendThreading)GraphicsBackendMultithreadingIndex)
            {
                DriverUtilities.ToggleOGLThreading(GraphicsBackendMultithreadingIndex == (int)BackendThreading.Off);
            }

            config.Graphics.BackendThreading.Value = (BackendThreading)GraphicsBackendMultithreadingIndex;

            TimeSpan systemTimeOffset = DateOffset - DateTime.Now;

            config.System.SystemTimeOffset.Value = systemTimeOffset.Seconds;
            config.System.FsGlobalAccessLogMode.Value = FsGlobalAccessLogMode;
            config.System.MemoryManagerMode.Value = (MemoryManagerMode)MemoryMode;

            float anisotropy = MaxAnisotropy == 0 ? -1 : MathF.Pow(2, MaxAnisotropy);

            config.Ui.GameDirs.Value = gameDirs;

            config.Graphics.MaxAnisotropy.Value = anisotropy;
            config.Graphics.AspectRatio.Value = (AspectRatio)AspectRatio;
            config.Graphics.ResScale.Value = ResolutionScale == 0 ? -1 : ResolutionScale;
            config.Graphics.ResScaleCustom.Value = CustomResolutionScale;
            config.System.AudioVolume.Value = Volume / 100;

            config.ToFileFormat().SaveConfig(AppConfig.ConfigurationPath);

            _previousVolumeLevel = Volume;

            MainView.UpdateGraphicsConfig();
        }

        public void RevertIfNotSaved()
        {
            Volume = _previousVolumeLevel;
        }
    }
}