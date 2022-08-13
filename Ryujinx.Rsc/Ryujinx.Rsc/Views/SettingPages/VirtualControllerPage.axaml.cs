using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Rsc.ViewModels;

namespace Ryujinx.Rsc.Views.SettingPages
{
    public partial class VirtualControllerPage : UserControl
    {
        public SettingsViewModel ViewModel { get; set; }

        private void OnBackRequested()
        {
            ViewModel.ShowMainViewTabs = true;
            ViewModel.ShowToolbar = true;
            App.RequestedOrientation = Controls.Orientation.Normal;
            ViewModel.MoveBack();
        }

        public VirtualControllerPage()
        {
            ViewModel = new SettingsViewModel();
            InitializeComponent();
            Layout.IsEditMode = true;

            if (AppConfig.PreviewerDetached)
            {
                AddHandler(Frame.NavigatedToEvent, (s, e) =>
                {
                    NavigatedTo(e);
                }, RoutingStrategies.Direct);
            }
        }

        private void NavigatedTo(NavigationEventArgs arg)
        {
            if (AppConfig.PreviewerDetached)
            {
                if (arg.NavigationMode == NavigationMode.New)
                {
                    ViewModel = (SettingsViewModel)arg.Parameter;
                }

                DataContext = ViewModel;
            }
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            ViewModel.ShowToolbar = false;
            App.RequestedOrientation = Controls.Orientation.Landscape;
            base.OnAttachedToVisualTree(e);
        }
    }
}