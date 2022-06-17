using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Ryujinx.Rsc.ViewModels;

namespace Ryujinx.Rsc.Views.SettingPages
{
    public partial class SettingsHome : UserControl
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsHome(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
        }
        
        public SettingsHome()
        {
            ViewModel = new SettingsViewModel();
            InitializeComponent();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            ViewModel.Title = "Settings";
        }
    }
}