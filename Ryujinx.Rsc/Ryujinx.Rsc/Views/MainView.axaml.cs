using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Gpu;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Services.Account.Acc;
using Ryujinx.Input.HLE;
using Ryujinx.Ui.Common.Configuration;
using Ryujinx.Rsc.Controls;
using Ryujinx.Rsc.ViewModels;
using Ryujinx.Ui.App.Common;
using System;
using System.IO;
using System.Threading;
using Avalonia.Media;
using Ryujinx.Rsc.Models;
using Ryujinx.Ava.Common.Ui.Controls;
using Ryujinx.Ava.Common.Input;
using Ryujinx.Ava.Common.Ui.Models;
using Ryujinx.Ava.Common;

namespace Ryujinx.Rsc.Views
{
    public partial class MainView : UserControl
    {
        private Control _mainViewContent;
        private ManualResetEvent _rendererWaitEvent;
        private bool _isClosing;
        private UserChannelPersistence _userChannelPersistence;
        public ApplicationLibrary ApplicationLibrary { get; set; }

        public VirtualFileSystem VirtualFileSystem { get; private set; }
        public ContentManager ContentManager { get; private set; }
        public AccountManager AccountManager { get; private set; }

        public LibHacHorizonManager LibHacHorizonManager { get; private set; }
        public MainViewModel ViewModel { get; set; }

        public SettingsView SettingsView { get; private set; }

        public MainView()
        {
            InitializeComponent();
            _rendererWaitEvent = new ManualResetEvent(false);
            SettingsView = new SettingsView();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            if (ViewModel == null && AppConfig.PreviewerDetached)
            {
                ViewModel = (MainViewModel) DataContext;

                Initialize();
                
                SettingsView = new SettingsView(VirtualFileSystem, ContentManager, this);

                LoadControls();

                ViewModel.Owner = this;
                ViewModel.Initialize();
            }
        }

        private void LoadControls()
        {
            GameGrid.ApplicationOpened += Application_Opened;

            GameGrid.DataContext = ViewModel;
        }

        private void Initialize()
        {
            _userChannelPersistence = new UserChannelPersistence();
            VirtualFileSystem = VirtualFileSystem.CreateInstance();
            LibHacHorizonManager = new LibHacHorizonManager();
            ContentManager = new ContentManager(VirtualFileSystem);

            LibHacHorizonManager.InitializeFsServer(VirtualFileSystem);
            LibHacHorizonManager.InitializeArpServer();
            LibHacHorizonManager.InitializeBcatServer();
            LibHacHorizonManager.InitializeSystemClients();

            ApplicationLibrary = new ApplicationLibrary(VirtualFileSystem, App.FileSystemHelperFactory.Invoke());

            VirtualFileSystem.FixExtraData(LibHacHorizonManager.RyujinxClient);

            AccountManager = new AccountManager(LibHacHorizonManager.RyujinxClient);

            VirtualFileSystem.ReloadKeySet();

            InputManager = new InputManager(new AvaloniaKeyboardDriver(this), AvaloniaVirtualControllerDriver.Instance);

            ConfigurationState.Instance.Ui.GameDirs.Event += (sender, args) =>
            {
                if (args.OldValue != args.NewValue)
                {
                    ViewModel.ReloadGameList();
                }
            };
        }

        private void Application_Opened(object sender, ApplicationOpenedEventArgs e)
        {
            if (e.Application != null)
            {
                string path = OperatingSystem.IsAndroid() ? e.Application.Path : new FileInfo(e.Application.Path).FullName;

                LoadApplication(path);
            }

            e.Handled = true;
        }

#pragma warning disable CS1998
        public async void LoadApplication(string path, bool startFullscreen = false, string titleName = "")
#pragma warning restore CS1998
        {
            if (AppHost != null)
            {
                return;
            }

#if RELEASE
            //await PerformanceCheck();
#endif

            Logger.RestartTime();

            Scaling = VisualRoot.RenderScaling;

            _mainViewContent = ContentFrame.Content as Control;

            VkRenderer = new VulkanRendererControl(ConfigurationState.Instance.Logger.GraphicsDebugLevel);
            AppHost = new AppHost(VkRenderer, InputManager, path, VirtualFileSystem, ContentManager, AccountManager, _userChannelPersistence, this);

            if (!AppHost.LoadGuestApplication().Result)
            {
                AppHost.DisposeContext();

                return;
            }

            ViewModel.TitleName = string.IsNullOrWhiteSpace(titleName) ? AppHost.Device.Application.TitleName : titleName;

            SwitchToGameControl();

            Thread gameThread = new Thread(InitializeGame)
            {
                Name = "GUI.WindowThread"
            };
            gameThread.Start();
        }

        private void InitializeGame()
        {
            VkRenderer.RendererInitialized += Renderer_Created;
            AppHost.AppExit += AppHost_AppExit;
            AppHost.StatusUpdatedEvent += AppHost_StatusUpdatedEvent;

            _rendererWaitEvent.WaitOne();
            
            Dispatcher.UIThread.Post(ControllerLayout.Reload);

            AppHost?.Start();

            AppHost.DisposeContext();
        }

        private void AppHost_StatusUpdatedEvent(object sender, StatusUpdatedEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (e.VSyncEnabled)
                {
                    ViewModel.VsyncColor = new SolidColorBrush(Color.Parse("#ff2eeac9"));
                }
                else
                {
                    ViewModel.VsyncColor = new SolidColorBrush(Color.Parse("#ffff4554"));
                }

                ViewModel.GameStatusText = e.GameStatus;
                ViewModel.FifoStatusText = e.FifoStatus;
                ViewModel.GpuStatusText = e.GpuName;
            });
        }

        private void AppHost_AppExit(object sender, EventArgs e)
        {
            if (_isClosing)
            {
                return;
            }

            ViewModel.IsGameRunning = false;

            ViewModel.ShowOverlay = false;
            AppHost.StatusUpdatedEvent -= AppHost_StatusUpdatedEvent;
            AppHost.AppExit -= AppHost_AppExit;

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (ContentFrame.Content != _mainViewContent)
                {
                    ContentFrame.Content = _mainViewContent;
                }

                ViewModel.EnableVirtualController = false;
                AppHost = null;
                App.RequestedOrientation = Orientation.Normal;
            });

            VkRenderer.RendererInitialized -= Renderer_Created;
            VkRenderer = null;

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ViewModel.Title = $"Ryujinx Test";
            });
        }

        private void Renderer_Created(object sender, EventArgs e)
        {
            _rendererWaitEvent.Set();
        }

        public VulkanRendererControl VkRenderer { get; set; }
        public InputManager InputManager { get; private set; }

        public AppHost AppHost { get; set; }

        public static void UpdateGraphicsConfig()
        {
            int resScale = ConfigurationState.Instance.Graphics.ResScale;
            float resScaleCustom = ConfigurationState.Instance.Graphics.ResScaleCustom;

            GraphicsConfig.ResScale = resScale == -1 ? resScaleCustom : resScale;
            GraphicsConfig.MaxAnisotropy = ConfigurationState.Instance.Graphics.MaxAnisotropy;
            GraphicsConfig.ShadersDumpPath = ConfigurationState.Instance.Graphics.ShadersDumpPath;
            GraphicsConfig.EnableShaderCache = ConfigurationState.Instance.Graphics.EnableShaderCache;
        }

        public void SwitchToGameControl(bool enableVirtualController = false)
        {
            Dispatcher.UIThread.Post(() =>
            {
                ViewModel.EnableVirtualController = enableVirtualController;
                ContentFrame.Content = VkRenderer;
            });
        }

        public static double Scaling { get; set; }

        private void GameListTab_OnClick(object? sender, RoutedEventArgs e)
        {
            ViewModel.CurrentView = View.GameList;

            ViewFrame.Content = GameGrid;
        }

        private void SettingsTab_OnClick(object? sender, RoutedEventArgs e)
        {
            ViewModel.CurrentView = View.Settings;
            ViewFrame.Content = SettingsView;
        }
    }
}