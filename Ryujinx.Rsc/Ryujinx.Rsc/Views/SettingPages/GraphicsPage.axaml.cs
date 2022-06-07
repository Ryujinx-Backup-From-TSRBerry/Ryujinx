using Avalonia;
using Avalonia.Controls;
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
    }
}