using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Common.Ui.Controls;
using Ryujinx.Common.Configuration;
using Ryujinx.Rsc.ViewModels;
using Ryujinx.Ui.App.Common;
using System;
using System.Collections.Generic;
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

        public async void PurgePtcMenuItem_Click(object sender, RoutedEventArgs ev)
        {
            var selection = ViewModel.IsGrid ? GameGrid.SelectedApplication : GameList.SelectedApplication;

            if (selection != null)
            {
                DirectoryInfo mainDir = new(Path.Combine(AppDataManager.GamesDirPath, selection.TitleId, "cache", "cpu", "0"));
                DirectoryInfo backupDir = new(Path.Combine(AppDataManager.GamesDirPath, selection.TitleId, "cache", "cpu", "1"));

                // FIXME: Found a way to reproduce the bold effect on the title name (fork?).
                UserResult result = await ContentDialogHelper.CreateConfirmationDialog(LocaleManager.Instance["DialogWarning"],
                    string.Format(LocaleManager.Instance["DialogPPTCDeletionMessage"], selection.TitleName), LocaleManager.Instance["InputDialogYes"], LocaleManager.Instance["InputDialogNo"], LocaleManager.Instance["RyujinxConfirm"]);

                List<FileInfo> cacheFiles = new();

                if (mainDir.Exists)
                {
                    cacheFiles.AddRange(mainDir.EnumerateFiles("*.cache"));
                }

                if (backupDir.Exists)
                {
                    cacheFiles.AddRange(backupDir.EnumerateFiles("*.cache"));
                }

                if (cacheFiles.Count > 0 && result == UserResult.Yes)
                {
                    foreach (FileInfo file in cacheFiles)
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception e)
                        {
                            await ContentDialogHelper.CreateErrorDialog(string.Format(LocaleManager.Instance["DialogPPTCDeletionErrorMessage"], file.Name, e));
                        }
                    }
                }
            }
        }

        public async void PurgeShaderMenuItem_Click(object sender, RoutedEventArgs ev)
        {
            var selection = ViewModel.IsGrid ? GameGrid.SelectedApplication : GameList.SelectedApplication;

            if (selection != null)
            {
                DirectoryInfo shaderCacheDir = new(Path.Combine(AppDataManager.GamesDirPath, selection.TitleId, "cache", "shader"));

                // FIXME: Found a way to reproduce the bold effect on the title name (fork?).
                UserResult result = await ContentDialogHelper.CreateConfirmationDialog(LocaleManager.Instance["DialogWarning"],
                    string.Format(LocaleManager.Instance["DialogShaderDeletionMessage"], selection.TitleName), LocaleManager.Instance["InputDialogYes"], LocaleManager.Instance["InputDialogNo"], LocaleManager.Instance["RyujinxConfirm"]);

                List<DirectoryInfo> oldCacheDirectories = new List<DirectoryInfo>();
                List<FileInfo> newCacheFiles = new List<FileInfo>();

                if (shaderCacheDir.Exists)
                {
                    oldCacheDirectories.AddRange(shaderCacheDir.EnumerateDirectories("*"));
                    newCacheFiles.AddRange(shaderCacheDir.GetFiles("*.toc"));
                    newCacheFiles.AddRange(shaderCacheDir.GetFiles("*.data"));
                }

                if ((oldCacheDirectories.Count > 0 || newCacheFiles.Count > 0) && result == UserResult.Yes)
                {
                    foreach (DirectoryInfo directory in oldCacheDirectories)
                    {
                        try
                        {
                            directory.Delete(true);
                        }
                        catch (Exception e)
                        {
                            await ContentDialogHelper.CreateErrorDialog(string.Format(LocaleManager.Instance["DialogPPTCDeletionErrorMessage"], directory.Name, e));
                        }
                    }
                }

                foreach (FileInfo file in newCacheFiles)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception e)
                    {
                        await ContentDialogHelper.CreateErrorDialog(string.Format(LocaleManager.Instance["ShaderCachePurgeError"], file.Name, e));
                    }
                }
            }
        }

        public async void DeleteMenuItem_Click(object sender, RoutedEventArgs ev)
        {
            var selection = ViewModel.IsGrid ? GameGrid.SelectedApplication : GameList.SelectedApplication;

            if (selection != null)
            {
                UserResult result = await ContentDialogHelper.CreateConfirmationDialog(LocaleManager.Instance["DialogWarning"],
                    string.Format(LocaleManager.Instance["DialogGameDeletionMessage"], selection.TitleName), LocaleManager.Instance["InputDialogYes"], LocaleManager.Instance["InputDialogNo"], LocaleManager.Instance["RyujinxConfirm"]);

                if(result == UserResult.Yes)
                {
                    var fileSystem = App.FileSystemHelperFactory();

                    fileSystem.DeleteFile(selection.Path);

                    ViewModel.ReloadGameList();
                }
            }
        }
    }
}