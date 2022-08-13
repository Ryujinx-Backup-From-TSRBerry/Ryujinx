using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LibHac.Common;
using Ryujinx.Ava.Common.Input;
using Ryujinx.Ava.Common.Ui.ViewModels;
using Ryujinx.Ui.App.Common;
using System;

namespace Ryujinx.Ava.Common.Ui.Controls
{
    public partial class GameListView : UserControl
    {
        private ApplicationData _selectedApplication;
        public static readonly RoutedEvent<ApplicationOpenedEventArgs> ApplicationOpenedEvent =
            RoutedEvent.Register<GameGridView, ApplicationOpenedEventArgs>(nameof(ApplicationOpened), RoutingStrategies.Bubble);

        public event EventHandler<string> OnSearch;
        public event EventHandler<ApplicationData> LongPressed;

        public event EventHandler<ApplicationOpenedEventArgs> ApplicationOpened
        {
            add { AddHandler(ApplicationOpenedEvent, value); }
            remove { RemoveHandler(ApplicationOpenedEvent, value); }
        }

        public void GameList_DoubleTapped(object sender, TappedEventArgs args)
        {
            if (sender is ListBox listBox)
            {
                if (listBox.SelectedItem is ApplicationData selected)
                {
                    RaiseEvent(new ApplicationOpenedEventArgs(selected, ApplicationOpenedEvent));
                }
            }
        }

        public void GameList_Tapped(object sender, TappedEventArgs args)
        {
            if (sender is ListBox listBox)
            {
                if (listBox.SelectedItem is ApplicationData selected)
                {
                    if (OperatingSystem.IsAndroid())
                    {
                        RaiseEvent(new ApplicationOpenedEventArgs(selected, ApplicationOpenedEvent));
                    }
                    else
                    {
                        OnHold();
                    }
                }
            }
        }

        public void GameList_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (sender is ListBox listBox)
            {
                var selected = listBox.SelectedItem as ApplicationData;

                _selectedApplication = selected;
            }
        }

        public ApplicationData SelectedApplication => _selectedApplication;

        public GameListView()
        {
            InitializeComponent();

            GameListBox.AddHandler(HoldGestureRecognizer.HoldGestureEvent, (s, e) =>
            {
                OnHold();
            }, RoutingStrategies.Direct);
        }

        private void OnHold()
        {
            if (SelectedApplication != null)
            {
                LongPressed?.Invoke(this, SelectedApplication);
            }

        }

        private void SearchBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            OnSearch.Invoke(this, (sender as TextBox).Text);
        }

        private void MenuBase_OnMenuOpened(object sender, EventArgs e)
        {
            var selection = SelectedApplication;

            if (selection != null)
            {
                if (sender is ContextMenu menu)
                {
                    bool canHaveUserSave = !Utilities.IsZeros(selection.ControlHolder.ByteSpan) && selection.ControlHolder.Value.UserAccountSaveDataSize > 0;
                    bool canHaveDeviceSave = !Utilities.IsZeros(selection.ControlHolder.ByteSpan) && selection.ControlHolder.Value.DeviceSaveDataSize > 0;
                    bool canHaveBcatSave = !Utilities.IsZeros(selection.ControlHolder.ByteSpan) && selection.ControlHolder.Value.BcatDeliveryCacheStorageSize > 0;

                    ((menu.Items as AvaloniaList<object>)[2] as MenuItem).IsEnabled = canHaveUserSave;
                    ((menu.Items as AvaloniaList<object>)[3] as MenuItem).IsEnabled = canHaveDeviceSave;
                    ((menu.Items as AvaloniaList<object>)[4] as MenuItem).IsEnabled = canHaveBcatSave;
                }
            }
        }
    }
}
