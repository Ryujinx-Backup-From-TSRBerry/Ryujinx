using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Rsc.ViewModels;

namespace Ryujinx.Rsc.Views.SettingPages
{
    public partial class GraphicsPage : UserControl
    {
        public SettingsViewModel ViewModel { get; set; }
        
        public GraphicsPage()
        {
            ViewModel = new SettingsViewModel();
            InitializeComponent();

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
                    ViewModel.Title = LocaleManager.Instance["SettingsTabGraphics"];
                }

                DataContext = ViewModel;
            }
        }

        private void SelectDriver_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.MoveToPage(Models.SettingPages.Driver);
        }
    }
}