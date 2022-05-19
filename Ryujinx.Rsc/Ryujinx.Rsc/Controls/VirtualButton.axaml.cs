using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Ryujinx.Input;
using System;

namespace Ryujinx.Rsc.Controls
{
    public partial class VirtualButton : UserControl, IVirtualControl
    {
        private object _faceLabel;
        private GamepadButtonInputId gamepadButtonInputId;

        public VirtualButton()
        {
            InitializeComponent();
        }

        public Point ControlPosition { get; set; }
        public Size ControlSize { get; set; }

        public object FaceLabel
        {
            get => _faceLabel; set
            {
                _faceLabel = value;

                Dispatcher.UIThread.Post(() =>
                {
                    Key.Content = _faceLabel;
                });
            }
        }

        public event EventHandler<IVirtualControl.VirualInputEventArgs> Input;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Key = this.FindControl<Label>("Key");
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            Input?.Invoke(this, new IVirtualControl.VirualInputEventArgs() { IsPressed = true, Button = Key.Content.ToString() });
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            Input?.Invoke(this, new IVirtualControl.VirualInputEventArgs() { IsPressed = false, Button = Key.Content.ToString() });
        }

        public StickInputId StickInputId { get; set; }
        public GamepadButtonInputId ButtonInputId
        {
            get => gamepadButtonInputId; set
            {
                gamepadButtonInputId = value;

                switch (gamepadButtonInputId)
                {
                    case GamepadButtonInputId.DpadUp:
                        FaceLabel = "Up";
                        break;
                    case GamepadButtonInputId.DpadDown:
                        FaceLabel = "Down";
                        break;
                    case GamepadButtonInputId.DpadLeft:
                        FaceLabel = "Left";
                        break;
                    case GamepadButtonInputId.DpadRight:
                        FaceLabel = "Right";
                        break;
                    case GamepadButtonInputId.Minus:
                        FaceLabel = "-";
                        break;
                    case GamepadButtonInputId.Plus:
                        FaceLabel = "+";
                        break;
                    default:
                        FaceLabel = gamepadButtonInputId.ToString();
                        break;
                }
            }
        }
        public bool IsStick => false;
    }
}
