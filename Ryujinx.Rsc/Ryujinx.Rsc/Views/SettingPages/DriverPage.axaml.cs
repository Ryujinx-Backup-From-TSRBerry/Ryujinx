using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Ryujinx.Rsc.ViewModels;

namespace Ryujinx.Rsc.Views.SettingPages
{
    public partial class DriverPage : UserControl
    {
        public SettingsViewModel ViewModel { get; }

        public DriverPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
        }
        
        public DriverPage()
        {
            ViewModel = new SettingsViewModel();
            InitializeComponent();
        }
        
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            ViewModel.Title = "Driver";
        }
    }
}