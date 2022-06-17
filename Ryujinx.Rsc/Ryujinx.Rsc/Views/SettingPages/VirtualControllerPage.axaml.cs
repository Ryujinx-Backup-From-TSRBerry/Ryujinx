using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Ryujinx.Rsc.ViewModels;

namespace Ryujinx.Rsc.Views.SettingPages
{
    public partial class VirtualControllerPage : UserControl
    {
        public SettingsViewModel ViewModel { get; }

        public VirtualControllerPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
            Layout.IsEditMode = true;
            Layout.OnBackRequested = OnBackRequested;
        }

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
        }
        
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            ViewModel.ShowMainViewTabs = false;
            ViewModel.ShowToolbar = false;
            App.RequestedOrientation = Controls.Orientation.Landscape;
            base.OnAttachedToVisualTree(e);
            ViewModel.Title = "VirtualController";
        }
    }
}