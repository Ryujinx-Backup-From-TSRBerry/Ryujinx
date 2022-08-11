using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Ryujinx.Rsc.ViewModels;

namespace Ryujinx.Rsc.Views.SettingPages
{
    public partial class GraphicsPage : UserControl
    {
        public SettingsViewModel ViewModel { get; }

        public GraphicsPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
        }
        
        public GraphicsPage()
        {
            ViewModel = new SettingsViewModel();
            InitializeComponent();
        }
        
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            ViewModel.Title = "Graphics";
        }

        private void SelectDriver_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.MoveToPage(Models.SettingPages.Driver);
        }
    }
}