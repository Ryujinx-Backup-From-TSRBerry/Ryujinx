using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Ryujinx.Ava.Common.Ui.Controls;
using Ryujinx.Ava.Common.Ui.Models;
using Ryujinx.Ava.Common.Ui.ViewModels;
using Ryujinx.HLE.FileSystem;

namespace Ryujinx.Ava.Common.Ui.Windows
{
    public partial class AvatarWindow : UserControl
    {
        private NavigationDialogHost _parent;
        private TempProfile _profile;

        public AvatarWindow(ContentManager contentManager)
        {
            ContentManager = contentManager;

            DataContext = ViewModel;

            InitializeComponent();
        }

        public AvatarWindow()
        {
            InitializeComponent();

            AddHandler(Frame.NavigatedToEvent, (s, e) =>
            {
                NavigatedTo(e);
            }, RoutingStrategies.Direct);
        }

        private void NavigatedTo(NavigationEventArgs arg)
        {
            if (AppConfig.PreviewerDetached)
            {
                if (arg.NavigationMode == NavigationMode.New)
                {
                    (_parent, _profile) = ((NavigationDialogHost, TempProfile))arg.Parameter;
                    ContentManager = _parent.ContentManager;
                    if (AppConfig.PreviewerDetached)
                    {
                        ViewModel = new AvatarProfileViewModel(() => ViewModel.ReloadImages());
                    }

                    DataContext = ViewModel;
                }
            }
        }

        public ContentManager ContentManager { get; private set; }

        internal AvatarProfileViewModel ViewModel { get; set; }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Dispose();

            _parent.GoBack();
        }

        private void ChooseButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedIndex > -1)
            {
                _profile.Image = ViewModel.SelectedImage;

                ViewModel.Dispose();

                _parent.GoBack();
            }
        }
    }
}