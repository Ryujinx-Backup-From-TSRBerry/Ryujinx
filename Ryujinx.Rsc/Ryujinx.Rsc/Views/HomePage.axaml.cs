using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Ui.Controls;
using Ryujinx.Rsc.ViewModels;
using System;
using System.IO;

namespace Ryujinx.Rsc.Views
{
    public partial class HomePage : UserControl
    {
        public MainViewModel ViewModel { get; private set; }
        
        public HomePage()
        {
            InitializeComponent();
            
            GameGrid.ApplicationOpened += Application_Opened;
            GameList.ApplicationOpened += Application_Opened;

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
                    ViewModel = (MainViewModel)arg.Parameter;
                }

                DataContext = ViewModel;
            }
        }
        
        private void Application_Opened(object sender, ApplicationOpenedEventArgs e)
        {
            if (e.Application != null)
            {
                string path = OperatingSystem.IsAndroid() ? e.Application.Path : new FileInfo(e.Application.Path).FullName;

                ViewModel.ApplicationPath = path;
                
                ViewModel.Owner.Navigate(typeof(GamePage), ViewModel);
            }

            e.Handled = true;
        }

        private void SearchBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            ViewModel.SearchText = SearchBox.Text;
        }
        

        public void Sort_Checked(object sender, RoutedEventArgs args)
        {
            if (sender is RadioButton button)
            {
                var sort = Enum.Parse<ApplicationSort>(button.Tag.ToString());
                ViewModel.Sort(sort);
            }
        }
        

        public void Order_Checked(object sender, RoutedEventArgs args)
        {
            if (sender is RadioButton button)
            {
                var tag = button.Tag.ToString();
                ViewModel.Sort(tag != "Descending");
            }
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenSettings();
        }
    }
}