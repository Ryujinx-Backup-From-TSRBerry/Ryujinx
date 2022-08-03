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
using FluentAvalonia.UI.Media.Animation;


namespace Ryujinx.Rsc.Views
{
    public partial class MainView : UserControl
    {
        private ManualResetEvent _rendererWaitEvent;
        private bool _isClosing;
        public UserChannelPersistence ChannelPersistence { get; private set; }
        public ApplicationLibrary ApplicationLibrary { get; set; }

        public VirtualFileSystem VirtualFileSystem { get; private set; }
        public ContentManager ContentManager { get; private set; }
        public AccountManager AccountManager { get; private set; }

        public LibHacHorizonManager LibHacHorizonManager { get; private set; }
        public MainViewModel ViewModel { get; set; }

        public MainView()
        {
            InitializeComponent();
            _rendererWaitEvent = new ManualResetEvent(false);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            if (ViewModel == null && AppConfig.PreviewerDetached)
            {
                ViewModel = (MainViewModel) DataContext;

                Initialize();

                ViewModel.Owner = this;
                ViewModel.Initialize();
            }
            
            Navigate(typeof(HomePage), ViewModel);
        }

        public void Navigate(Type sourcePageType, object parameter)
        {
            ViewFrame.Navigate(sourcePageType, parameter, new SuppressNavigationTransitionInfo());
        }

        public void GoBack(object parameter = null)
        {
            if (ViewFrame.BackStack.Count > 0)
            {
                ViewFrame.GoBack();
            }
        }

        private void Initialize()
        {
            ChannelPersistence = new UserChannelPersistence();
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

        public InputManager InputManager { get; set; }

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
            
        }

        public static void UpdateGraphicsConfig()
        {
            int resScale = ConfigurationState.Instance.Graphics.ResScale;
            float resScaleCustom = ConfigurationState.Instance.Graphics.ResScaleCustom;

            GraphicsConfig.ResScale = resScale == -1 ? resScaleCustom : resScale;
            GraphicsConfig.MaxAnisotropy = ConfigurationState.Instance.Graphics.MaxAnisotropy;
            GraphicsConfig.ShadersDumpPath = ConfigurationState.Instance.Graphics.ShadersDumpPath;
            GraphicsConfig.EnableShaderCache = ConfigurationState.Instance.Graphics.EnableShaderCache;
        }

        public static double Scaling { get; set; }

        private void GameListTab_OnClick(object? sender, RoutedEventArgs e)
        {
            ViewModel.CurrentView = View.GameList;

            //ViewFrame.Content = GameGrid;
        }
    }
}