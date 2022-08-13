using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using ReactiveUI;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.HLE.FileSystem;
using Ryujinx.Rsc.Models;
using Ryujinx.Rsc.ViewModels;
using Ryujinx.Rsc.Views.SettingPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ryujinx.Rsc.Views
{
    public partial class SettingsView : UserControl
    {
        public Stack<UserControl> Pages { get; }
        public SettingsViewModel ViewModel { get; set; }
        
        public MainView Owner { get; private set; }

        public SettingsView()
        {
            Pages = new Stack<UserControl>();
            ViewModel = new SettingsViewModel();
            DataContext = ViewModel;
            InitializeComponent();

            if (AppConfig.PreviewerDetached)
            {
                AddHandler(Frame.NavigatedToEvent, (s, e) =>
                {
                    NavigatedTo(e);
                }, RoutingStrategies.Direct);

                AddHandler(Frame.NavigatingFromEvent, async (s, e) =>
                {
                    NavigatedFrom(e);
                }, RoutingStrategies.Direct);
            }
        }

        private void NavigatedTo(NavigationEventArgs arg)
        {
            if (AppConfig.PreviewerDetached)
            {
                if (arg.NavigationMode == NavigationMode.New)
                {
                    Owner = (MainView)arg.Parameter;
                    ViewModel = new SettingsViewModel(Owner.VirtualFileSystem, Owner.ContentManager, this);
                }

                DataContext = ViewModel;

                ViewModel.Title = LocaleManager.Instance["SettingsTabGeneralGeneral"];
            }
        }

        private void NavigatedFrom(NavigatingCancelEventArgs arg)
        {
            if (AppConfig.PreviewerDetached)
            {
                if (arg.NavigationMode == NavigationMode.Back)
                {
                    
                    ViewModel?.SaveSettings();
                }
            }
        }

        public void MoveBack()
        {
            Owner.GoBack();
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            MoveBack();
        }
    }
}