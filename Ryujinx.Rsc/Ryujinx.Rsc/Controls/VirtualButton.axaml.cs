using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Ryujinx.Input;
using System;
using PathIcon = Avalonia.Controls.PathIcon;

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
                    case GamepadButtonInputId.DpadUp:
                        FaceLabel = new SymbolIcon() {Symbol = Symbol.ChevronUp};
                        break;
                    case GamepadButtonInputId.DpadDown:
                        FaceLabel = new SymbolIcon() {Symbol = Symbol.ChevronDown};
                        break;
                    case GamepadButtonInputId.DpadLeft:
                        FaceLabel = new SymbolIcon() {Symbol = Symbol.ChevronLeft};
                        break;
                    case GamepadButtonInputId.DpadRight:
                        FaceLabel = new SymbolIcon() {Symbol = Symbol.ChevronRight};
                        break;
                    case GamepadButtonInputId.Minus:
                        FaceLabel = new SymbolIcon() {Symbol = Symbol.Remove};
                        break;
                    case GamepadButtonInputId.Plus:
                        FaceLabel = new SymbolIcon() {Symbol = Symbol.Add};
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
