using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Rsc.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeZone = Ryujinx.Ava.Common.Ui.Models.TimeZone;

namespace Ryujinx.Rsc.Views.SettingPages
{
    public partial class GeneralPage : UserControl
    {
        public SettingsViewModel ViewModel { get; set; }
        
        public GeneralPage()
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

        private async void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            string path = string.Empty;

            var storage = await (VisualRoot as TopLevel).StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions());

            if (storage.Count > 0)
            {
                var folder = storage.First();

                if (folder.TryGetUri(out _))
                {
                    path = await folder.SaveBookmarkAsync();
                }
            }

            if (!string.IsNullOrWhiteSpace(path))
            {
                ViewModel.GameDirectories.Add(path);
            }
        }

        private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
        {
            List<string> selected = new(GameList.SelectedItems.Cast<string>());

            foreach (string path in selected)
            {
                ViewModel.GameDirectories.Remove(path);
            }
        }
    }
}