using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Rsc.ViewModels;
using Ryujinx.Rsc.Views;
using Ryujinx.Ui.Common.Configuration;
using System;
using System.IO;
using Ryujinx.Rsc.Controls;
using Ryujinx.Audio.Integration;
using Ryujinx.Audio.Backends.Dummy;
using Ryujinx.Ui.Common.Helper;

namespace Ryujinx.Rsc
{
    public partial class App : Application
    {
        private static Orientation _requestedOrientation;

        public static bool PreviewerDetached { get; set; }
        public static string BaseDirectory { get; set; }
        public static RenderTimer RenderTimer { get; set; }

        public static Func<AudioBackend, IHardwareDeviceDriver> CreateAudioHardwareDeviceDriver { get; set; }

        static App()
        {
            CreateAudioHardwareDeviceDriver = (_) => new DummyHardwareDeviceDriver();
        }

        public static Orientation RequestedOrientation
        {
            get => _requestedOrientation; set
            {
                _requestedOrientation = value;

                OrientationRequested?.Invoke(null, new OrientationRequestedArgs(_requestedOrientation));
            }
        }

        public static event EventHandler<OrientationRequestedArgs> OrientationRequested;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static void LoadConfiguration()
        {
            if (PreviewerDetached)
            {
                if (BaseDirectory != null)
                {
                    Directory.CreateDirectory(BaseDirectory);
                }

                // Setup base data directory.
                AppDataManager.Initialize(BaseDirectory);

                // Initialize the configuration.
                ConfigurationState.Initialize();

                // Initialize the logger system.
                LoggerModule.Initialize();

                string localConfigurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json");
                string appDataConfigurationPath = Path.Combine(AppDataManager.BaseDirPath, "Config.json");

                // Now load the configuration as the other subsystems are now registered
                ConfigurationPath = File.Exists(localConfigurationPath)
                    ? localConfigurationPath
                    : File.Exists(appDataConfigurationPath)
                        ? appDataConfigurationPath
                        : null;

                if (ConfigurationPath == null)
                {
                    // No configuration, we load the default values and save it to disk
                    ConfigurationPath = appDataConfigurationPath;

                    ConfigurationState.Instance.LoadDefault();
                    ConfigurationState.Instance.ToFileFormat().SaveConfig(ConfigurationPath);
                }
                else
                {
                    if (ConfigurationFileFormat.TryLoad(ConfigurationPath, out ConfigurationFileFormat configurationFileFormat))
                    {
                        ConfigurationState.Instance.Load(configurationFileFormat, ConfigurationPath);
                    }
                    else
                    {
                        ConfigurationState.Instance.LoadDefault();
                        Logger.Warning?.PrintMsg(LogClass.Application, $"Failed to load config! Loading the default config instead.\nFailed config location {ConfigurationPath}");
                    }
                }
            }
        }

        public static string ConfigurationPath { get; set; }

        public static Func<IFileSystemHelper> FileSystemHelperFactory{ get; set; }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainViewModel()
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = new MainViewModel()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}