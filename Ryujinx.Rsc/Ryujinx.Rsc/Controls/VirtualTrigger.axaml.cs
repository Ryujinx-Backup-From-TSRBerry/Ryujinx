using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Ryujinx.Input;
using System;

namespace Ryujinx.Rsc.Controls
{
    public partial class VirtualTrigger : UserControl, IVirtualControl
    {
        private object _faceLabel;
        private GamepadButtonInputId gamepadButtonInputId;

        public VirtualTrigger()
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

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            Input?.Invoke(this, new IVirtualControl.VirualInputEventArgs() { IsPressed = true, Button = Key.Content.ToString() });
            Contact.Classes.Add("pressed");
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            Input?.Invoke(this, new IVirtualControl.VirualInputEventArgs() { IsPressed = false, Button = Key.Content.ToString() });
            Contact.Classes.Remove("pressed");
        }

        public StickInputId StickInputId { get; set; }
        public GamepadButtonInputId ButtonInputId
        {
            get => gamepadButtonInputId;
            set
            {
                gamepadButtonInputId = value;

                switch (gamepadButtonInputId)
                {
                    case GamepadButtonInputId.LeftTrigger:
                        FaceLabel = "ZL";
                        Border.Classes.Add("left");
                        Border.Classes.Remove("right");
                        break;
                    case GamepadButtonInputId.RightTrigger:
                        FaceLabel = "ZR";
                        Border.Classes.Remove("left");
                        Border.Classes.Add("right");
                        break;
                }
            }
        }
        public bool IsStick => false;
    }
}
