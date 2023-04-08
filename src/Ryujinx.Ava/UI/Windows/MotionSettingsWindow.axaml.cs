using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.UI.Models;
using Ryujinx.Ava.UI.ViewModels;
using Ryujinx.Common.Configuration.Hid.Controller;
using System.Threading.Tasks;

namespace Ryujinx.Ava.UI.Windows
{
    public partial class MotionSettingsWindow : UserControl
    {
        private readonly InputConfiguration<GamepadInputId, StickInputId> _viewmodel;

        public MotionSettingsWindow()
        {
            InitializeComponent();
            DataContext = _viewmodel;
        }

        public MotionSettingsWindow(ControllerSettingsViewModel viewmodel)
        {
            var config = viewmodel.Configuration as InputConfiguration<GamepadInputId, StickInputId>;

            _viewmodel = new InputConfiguration<GamepadInputId, StickInputId>()
            {
                Slot = config.Slot,
                AltSlot = config.AltSlot,
                DsuServerHost = config.DsuServerHost,
                DsuServerPort = config.DsuServerPort,
                MirrorInput = config.MirrorInput,
                EnableMotion = config.EnableMotion,
                Sensitivity = config.Sensitivity,
                GyroDeadzone = config.GyroDeadzone,
                EnableCemuHookMotion = config.EnableCemuHookMotion
            };

            InitializeComponent();
            DataContext = _viewmodel;
        }

        public static async Task Show(ControllerSettingsViewModel viewmodel)
        {
            MotionSettingsWindow content = new(viewmodel);

            ContentDialog contentDialog = new()
            {
                Title = LocaleManager.Instance[LocaleKeys.ControllerMotionTitle],
                PrimaryButtonText = LocaleManager.Instance[LocaleKeys.ControllerSettingsSave],
                SecondaryButtonText = "",
                CloseButtonText = LocaleManager.Instance[LocaleKeys.ControllerSettingsClose],
                Content = content
            };
            contentDialog.PrimaryButtonClick += (sender, args) =>
            {
                var config = viewmodel.Configuration as InputConfiguration<GamepadInputId, StickInputId>;
                config.Slot = content._viewmodel.Slot;
                config.EnableMotion = content._viewmodel.EnableMotion;
                config.Sensitivity = content._viewmodel.Sensitivity;
                config.GyroDeadzone = content._viewmodel.GyroDeadzone;
                config.AltSlot = content._viewmodel.AltSlot;
                config.DsuServerHost = content._viewmodel.DsuServerHost;
                config.DsuServerPort = content._viewmodel.DsuServerPort;
                config.EnableCemuHookMotion = content._viewmodel.EnableCemuHookMotion;
                config.MirrorInput = content._viewmodel.MirrorInput;
            };

            await contentDialog.ShowAsync();
        }
    }
}