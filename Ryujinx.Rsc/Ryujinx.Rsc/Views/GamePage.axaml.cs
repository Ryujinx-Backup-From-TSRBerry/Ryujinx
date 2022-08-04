using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Ui.Controls;
using Ryujinx.Ava.Common.Ui.Models;
using Ryujinx.Common.Logging;
using Ryujinx.Rsc.Controls;
using Ryujinx.Rsc.ViewModels;
using Ryujinx.Ui.Common.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ryujinx.Rsc.Views
{
    public partial class GamePage : UserControl
    {
        private bool _isClosing;
        private bool _canNavigateFrom;
        private ManualResetEvent _rendererWaitEvent;
        public VulkanRendererControl VkRenderer { get; set; }

        public AppHost AppHost { get; set; }

        public GamePage()
        {
            InitializeComponent();
            _rendererWaitEvent = new ManualResetEvent(false);

            if (AppConfig.PreviewerDetached)
            {
                AddHandler(Frame.NavigatedToEvent, (s, e) =>
                {
                    NavigatedTo(e);
                }, RoutingStrategies.Direct);

                AddHandler(Frame.NavigatingFromEvent, async (s, e) =>
                {
                    await NavigatedFrom(e);
                }, RoutingStrategies.Direct);
            }
        }

        private void NavigatedTo(NavigationEventArgs arg)
        {
            if (AppConfig.PreviewerDetached)
            {
                if (arg.NavigationMode == NavigationMode.New)
                {
                    ViewModel = (MainViewModel)arg.Parameter;
                }

                DataContext = ViewModel;

                ViewModel.GamePage = this;

                if (!string.IsNullOrWhiteSpace(ViewModel.ApplicationPath))
                {
                    StartGame(ViewModel.ApplicationPath);
                }
            }
        }

        private async Task NavigatedFrom(NavigatingCancelEventArgs arg)
        {
            if (AppConfig.PreviewerDetached)
            {
                if (arg.NavigationMode == NavigationMode.Back)
                {
                    if (_canNavigateFrom)
                    {
                        return;
                    }
                    arg.Cancel = true;
                    _canNavigateFrom = true;
                    var shouldExit = await ContentDialogHelper.CreateStopEmulationDialog();

                    if (shouldExit)
                    {
                        AppHost.Stop();
                    }
                }
            }
        }

        public MainViewModel ViewModel { get; set; }

        private void StartGame(string path, string titleName = "")
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

            VkRenderer = new VulkanRendererControl(ConfigurationState.Instance.Logger.GraphicsDebugLevel);
            AppHost = new AppHost(VkRenderer, ViewModel.Owner.InputManager, path, ViewModel.Owner.VirtualFileSystem,
                ViewModel.Owner.ContentManager, ViewModel.Owner.AccountManager, ViewModel.Owner.ChannelPersistence,
                this);

            if (!AppHost.LoadGuestApplication().Result)
            {
                AppHost.DisposeContext();

                return;
            }

            ViewModel.TitleName =
                string.IsNullOrWhiteSpace(titleName) ? AppHost.Device.Application.TitleName : titleName;

            Thread gameThread = new Thread(InitializeGame) { Name = "GUI.WindowThread" };
            gameThread.Start();
        }

        public double Scaling { get; set; }


        private void InitializeGame()
        {
            VkRenderer.RendererInitialized += Renderer_Created;
            AppHost.AppExit += AppHost_AppExit;
            AppHost.StatusUpdatedEvent += AppHost_StatusUpdatedEvent;

            Dispatcher.UIThread.Post(() =>
            {
                ContentFrame.Content = VkRenderer;
                ControllerLayout.Reload();
            });

            _rendererWaitEvent.WaitOne();

            Dispatcher.UIThread.Post(ControllerLayout.Reload);
            AppHost?.Start();

            AppHost.DisposeContext();
        }

        private void Renderer_Created(object sender, EventArgs e)
        {
            _rendererWaitEvent.Set();

            Dispatcher.UIThread.Post(() =>
            {
                ViewModel.EnableVirtualController = true;
            });
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
            _rendererWaitEvent.Dispose();

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ContentFrame.Content = null;
                ViewModel.EnableVirtualController = false;
                AppHost = null;
                App.RequestedOrientation = Orientation.Normal;
            });

            VkRenderer.RendererInitialized -= Renderer_Created;
            VkRenderer = null;
            ViewModel.ApplicationPath = string.Empty;

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ViewModel.Title = $"Ryujinx Test";
                ViewModel.Owner.GoBack();
            });
        }
    }
}