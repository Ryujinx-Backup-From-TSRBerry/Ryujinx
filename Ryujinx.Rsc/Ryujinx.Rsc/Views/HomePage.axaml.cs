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
using Ryujinx.Ui.App.Common;
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
            
            GameGrid.LongPressed += ApplicationLongPressed;
            GameList.LongPressed += ApplicationLongPressed;

            if (AppConfig.PreviewerDetached)
            {
                AddHandler(Frame.NavigatedToEvent, (s, e) =>
                {
                    NavigatedTo(e);
                }, RoutingStrategies.Direct);
            }
        }

        private void ApplicationLongPressed(object sender, ApplicationData e)
        {
            ViewModel.ShowContextOptions = true;
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
            if (!ViewModel.ShowContextOptions && e.Application != null)
            {
                LoadApplication(e.Application);
            }

            e.Handled = true;
        }

        private void LoadApplication(ApplicationData application)
        {
            string path = OperatingSystem.IsAndroid() ? application.Path : new FileInfo(application.Path).FullName;

            ViewModel.ApplicationPath = path;
                
            ViewModel.Owner.Navigate(typeof(GamePage), ViewModel);
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

        private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.ReloadGameList();
        }

        private void PlayButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedApplication = ViewModel.IsGrid ? GameGrid.SelectedApplication : GameList.SelectedApplication;

            if (selectedApplication != null)
            {
                LoadApplication(selectedApplication);

                ViewModel.ShowContextOptions = false;
            }
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void FavoriteButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedApplication = ViewModel.IsGrid ? GameGrid.SelectedApplication : GameList.SelectedApplication;

            if (selectedApplication != null)
            {
                ViewModel.ToggleFavorite(selectedApplication);
            }
        }

        private void DismissButton_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.ShowContextOptions = false;
        }
    }
}