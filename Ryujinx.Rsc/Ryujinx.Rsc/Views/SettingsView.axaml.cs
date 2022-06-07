using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI;
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
        public SettingsViewModel Viewmodel { get; set; }
        
        public MainView Owner { get; }

        public SettingsView(VirtualFileSystem fileSystem, ContentManager contentManager, MainView owner)
        {
            Pages = new Stack<UserControl>();
            Viewmodel = new SettingsViewModel(fileSystem, contentManager ,this);
            DataContext = Viewmodel;
            InitializeComponent();
            Pages.Push(new SettingsHome(Viewmodel));
            Owner = owner;
        }

        public SettingsView()
        {
            Pages = new Stack<UserControl>();
            Viewmodel = new SettingsViewModel();
            DataContext = Viewmodel;
            InitializeComponent();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            if (Pages.Count > 0)
            {
                Frame.Content = Pages.Peek();
            }
            else
            {
                NavigateToPage(new SettingsHome());
            }
            
            Viewmodel.NotifyPageChanged();
        }

        public void NavigateToPage(UserControl page)
        {
            if (page != null)
            {
                Pages.Push(page);
                Frame.Content = page;
            }
            
            Viewmodel.NotifyPageChanged();
        }

        public void MoveBack()
        {
            lock (Pages)
            {
                if (Pages.Count > 1)
                {
                    Pages.Pop();
                }

                Frame.Content = Pages.Peek();
            }
            
            Viewmodel.NotifyPageChanged();
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            MoveBack();
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            Viewmodel.SaveSettings();
        }
    }
}