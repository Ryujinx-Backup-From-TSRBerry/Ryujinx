using Avalonia;
using Avalonia.Controls;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Services.Account.Acc;
using Ryujinx.Rsc.Controls;
using Ryujinx.Rsc.ViewModels;
using Ryujinx.Ui.App.Common;

namespace Ryujinx.Rsc.Views
{
    public partial class MainView : UserControl
    {
        public ApplicationLibrary ApplicationLibrary { get; set; }

        public VirtualFileSystem VirtualFileSystem { get; private set; }
        public ContentManager ContentManager { get; private set; }
        public AccountManager AccountManager { get; private set; }

        public LibHacHorizonManager LibHacHorizonManager { get; private set; }
        public MainViewModel ViewModel { get; set; }

        public MainView()
        {
            InitializeComponent();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            if (ViewModel == null)
            {
                ViewModel = (MainViewModel) DataContext;

                Initialize();

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
            VirtualFileSystem = VirtualFileSystem.CreateInstance();
            LibHacHorizonManager = new LibHacHorizonManager();
            ContentManager = new ContentManager(VirtualFileSystem);

            LibHacHorizonManager.InitializeFsServer(VirtualFileSystem);
            LibHacHorizonManager.InitializeArpServer();
            LibHacHorizonManager.InitializeBcatServer();
            LibHacHorizonManager.InitializeSystemClients();

            ApplicationLibrary = new ApplicationLibrary(VirtualFileSystem);ApplicationLibrary = new ApplicationLibrary(VirtualFileSystem);

            VirtualFileSystem.FixExtraData(LibHacHorizonManager.RyujinxClient);

            AccountManager = new AccountManager(LibHacHorizonManager.RyujinxClient);

            VirtualFileSystem.ReloadKeySet();
        }

        private void Application_Opened(object sender, ApplicationOpenedEventArgs e)
        {
            if (e.Application != null)
            {
                string path = new FileInfo(e.Application.Path).FullName;

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

            _currentEmulatedGamePath = path;
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

            _rendererWaitEvent.WaitOne();

            AppHost?.Start();

            AppHost.DisposeContext();
        }

        private void AppHost_AppExit(object sender, EventArgs e)
        {
            if (_isClosing)
            {
                return;
            }

            ViewModel.IsGameRunning = false;

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (ContentFrame.Content != _mainViewContent)
                {
                    ContentFrame.Content = _mainViewContent;
                }

                AppHost = null;
            });

            VkRenderer.RendererInitialized -= Renderer_Created;
            VkRenderer = null;

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ViewModel.Title = $"Ryujinx Test";
            });
        }
    }
}