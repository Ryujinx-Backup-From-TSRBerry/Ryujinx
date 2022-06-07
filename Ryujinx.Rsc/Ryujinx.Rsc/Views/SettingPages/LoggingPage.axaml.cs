using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Ryujinx.Rsc.ViewModels;

namespace Ryujinx.Rsc.Views.SettingPages
{
    public partial class LoggingPage : UserControl
    {
        public SettingsViewModel ViewModel { get; }

        public LoggingPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
        }
        
        public LoggingPage()
        {
            ViewModel = new SettingsViewModel();
            InitializeComponent();
        }
        
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            ViewModel.Title = "System";
        }
    }
}