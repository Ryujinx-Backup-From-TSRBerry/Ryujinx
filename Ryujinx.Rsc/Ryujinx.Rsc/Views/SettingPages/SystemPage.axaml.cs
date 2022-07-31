using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Ryujinx.Rsc.ViewModels;
using System;
using TimeZone = Ryujinx.Ava.Common.Ui.Models.TimeZone;

namespace Ryujinx.Rsc.Views.SettingPages
{
    public partial class SystemPage : UserControl
    {
        public SettingsViewModel ViewModel { get; }

        public SystemPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
        }
        
        public SystemPage()
        {
            ViewModel = new SettingsViewModel();
            InitializeComponent();
        }
        
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            ViewModel.Title = "System";
        }

        private void TimeZoneBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                if (e.AddedItems[0] is TimeZone timeZone)
                {
                    e.Handled = true;

                    ViewModel.ValidateAndSetTimeZone(timeZone.Location);
                }
            }
        }

        private void TimeZoneBox_OnTextChanged(object sender, EventArgs e)
        {
            if (sender is AutoCompleteBox box)
            {
                if (box.SelectedItem != null && box.SelectedItem is TimeZone timeZone)
                {
                    ViewModel.ValidateAndSetTimeZone(timeZone.Location);
                }
            }
        }
    }
}