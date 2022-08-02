using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using ReactiveUI;
using Ryujinx.Ava.Common;
using Ryujinx.HLE.FileSystem;
using Ryujinx.Rsc.Models;
using Ryujinx.Rsc.ViewModels;
using Ryujinx.Rsc.Views.SettingPages;
using System.Collections.Generic;

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
                
                ViewModel.NotifyPageChanged();
            }
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            if (Pages.Count > 0)
            {
                ContentFrame.Content = Pages.Peek();
            }
            else
            {
                NavigateToPage(new SettingsHome());
            }
            
            ViewModel.NotifyPageChanged();
        }

        public void NavigateToPage(UserControl page)
        {
            if (page != null)
            {
                Pages.Push(page);
                ContentFrame.Content = page;
            }
            
            ViewModel.NotifyPageChanged();
        }

        public void MoveBack()
        {
            lock (Pages)
            {
                if (Pages.Count > 1)
                {
                    Pages.Pop();
                }

                ContentFrame.Content = Pages.Peek();
            }
            
            ViewModel.NotifyPageChanged();
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            MoveBack();
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveSettings();
        }

        private void HomeButton_OnClick(object sender, RoutedEventArgs e)
        {
            Owner.GoBack();
        }
    }
}