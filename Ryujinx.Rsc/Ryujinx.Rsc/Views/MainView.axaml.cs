using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Input;
using Ryujinx.Ava.Common.Ui.Controls;
using Ryujinx.Graphics.Gpu;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Services.Account.Acc;
using Ryujinx.Input.HLE;
using Ryujinx.Rsc.Models;
using Ryujinx.Rsc.ViewModels;
using Ryujinx.Ui.App.Common;
using Ryujinx.Ui.Common.Configuration;
using System;
using System.IO;
using System.Threading;

namespace Ryujinx.Rsc.Views
{
    public partial class MainView : UserControl
    {
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

            App.BackPressed += App_BackPressed;
        }

        private void MainView_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void App_BackPressed(object sender, RoutedEventArgs e)
        {
            if(ViewFrame.CanGoBack)
            {
                ViewFrame.GoBack();
                e.Handled = true;
            }
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            if (ViewModel == null && AppConfig.PreviewerDetached)
            {
                ViewModel = (MainViewModel) DataContext;

                Initialize();

                ViewModel.Owner = this;

                ConfigurationState.Instance.Ui.GridSize.Value = 2;

                foreach (var folder in ConfigurationState.Instance.Ui.GameDirs.Value)
                {
                    (VisualRoot as TopLevel)?.StorageProvider?.OpenFolderBookmarkAsync(folder);
                }

                ViewModel.Initialize();
            }
            
            Navigate(typeof(HomePage), ViewModel);
        }

        public void Navigate(Type sourcePageType, object parameter)
        {
            ViewFrame.Navigate(sourcePageType, parameter);
        }

        public void GoBack(object parameter = null)
        {
            if (ViewFrame.BackStackDepth > 0)
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

            ConfigurationState.Instance.Ui.GameDirs.Event += (sender, args) =>
            {
                if (args.OldValue != args.NewValue)
                {
                    ViewModel.ReloadGameList();
                }
            };
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
        }
    }
}