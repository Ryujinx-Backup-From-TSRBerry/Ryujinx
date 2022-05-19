using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace Ryujinx.Rsc.Controls
{
    public partial class VirtualGameControllerLayout : UserControl
    {
        public AvaloniaVirtualControllerDriver Controller => AvaloniaVirtualControllerDriver.Instance;

        public VirtualGameControllerLayout()
        {
            InitializeComponent();
            BindButtons();

            IObservable<Rect> resizeObservable = this.GetObservable(BoundsProperty);

            resizeObservable.Subscribe(Resized);
        }

        protected virtual void Resized(Rect rect)
        {
            SetPositions();
        }

        private void BindButtons()
        {
            StickLeft.StickInputId = Input.StickInputId.Left;
            StickRight.StickInputId = Input.StickInputId.Right;
            ButtonA.ButtonInputId = Input.GamepadButtonInputId.A;
            ButtonB.ButtonInputId = Input.GamepadButtonInputId.B;
            ButtonX.ButtonInputId = Input.GamepadButtonInputId.X;
            ButtonY.ButtonInputId = Input.GamepadButtonInputId.Y;
            DpadUp.ButtonInputId = Input.GamepadButtonInputId.DpadUp;
            DPadDown.ButtonInputId = Input.GamepadButtonInputId.DpadDown;
            DPadLeft.ButtonInputId = Input.GamepadButtonInputId.DpadLeft;
            DPadRight.ButtonInputId = Input.GamepadButtonInputId.DpadRight;
            Minus.ButtonInputId = Input.GamepadButtonInputId.Minus;
            Plus.ButtonInputId = Input.GamepadButtonInputId.Plus;

            StickLeft.Input += Stick_Input;
            StickRight.Input += Stick_Input;
            ButtonA.Input += Button_Input;
            ButtonB.Input += Button_Input;
            ButtonX.Input += Button_Input;
            ButtonY.Input += Button_Input;
            DpadUp.Input += Button_Input;
            DPadDown.Input += Button_Input;
            DPadLeft.Input += Button_Input;
            DPadRight.Input += Button_Input;
            Minus.Input += Button_Input;
            Plus.Input += Button_Input;
        }

        private void Button_Input(object sender, IVirtualControl.VirualInputEventArgs e)
        {
            if (sender is IVirtualControl control)
            {
                if (e.IsPressed)
                {
                    Controller.SetButtonPressed(control.ButtonInputId);
                }
                else
                {
                    Controller.SetButtonReleased(control.ButtonInputId);
                }
            }
        }

        private void Stick_Input(object sender, IVirtualControl.VirualInputEventArgs e)
        {
            if(sender is IVirtualControl control)
            {
                Controller.SetStickAxis(control.StickInputId, e.StickValue);
            }
        }

        public void SetPositions()
        {
            var size = Bounds.Size;
            if (size.Width == 0 || size.Height == 0)
            {
                return;
            }

            double triggerSpace = 0.2;
            double padding = 0.1;

            var buttonBoxSize = new Size(120, 120);
            var halfBox = buttonBoxSize / 2;

            double xPosition = 0, yPosition = 0;
            // Place sticks
            xPosition = padding * size.Width;
            yPosition = (padding + triggerSpace) * size.Height;
            Canvas.SetTop(StickLeft, yPosition);
            Canvas.SetLeft(StickLeft, xPosition + halfBox.Width - StickLeft.Width / 2);
            Canvas.SetTop(StickRight, size.Height - StickRight.Height - size.Height * padding);
            Canvas.SetLeft(StickRight, size.Width - xPosition - StickRight.Width / 2 - halfBox.Width);

            // Place Right Buttons
            xPosition = size.Width - padding * size.Width - halfBox.Width - ButtonX.Width / 2;
            yPosition = (padding + triggerSpace) * size.Height - ButtonX.Width / 2;
            Canvas.SetTop(ButtonX, yPosition);
            Canvas.SetLeft(ButtonX, xPosition);
            Canvas.SetTop(ButtonB, yPosition + buttonBoxSize.Height);
            Canvas.SetLeft(ButtonB, xPosition);

            xPosition = size.Width - padding * size.Width - ButtonX.Width / 2;
            yPosition = (padding + triggerSpace) * size.Height - ButtonA.Height / 2 + halfBox.Height;
            Canvas.SetTop(ButtonA, yPosition);
            Canvas.SetLeft(ButtonA, xPosition);
            Canvas.SetTop(ButtonY, yPosition);
            Canvas.SetLeft(ButtonY, xPosition - buttonBoxSize.Width);

            // Place Left Buttons
            xPosition = padding * size.Width + halfBox.Width - DpadUp.Width / 2;
            yPosition = size.Height - padding * size.Height - DPadDown.Height / 2;
            Canvas.SetTop(DPadDown, yPosition);
            Canvas.SetLeft(DpadUp, xPosition);
            Canvas.SetTop(DpadUp, yPosition - buttonBoxSize.Height);
            Canvas.SetLeft(DPadDown, xPosition);

            xPosition = xPosition - halfBox.Width;
            yPosition -= halfBox.Height;
            Canvas.SetTop(DPadLeft, yPosition);
            Canvas.SetLeft(DPadLeft, xPosition);
            Canvas.SetTop(DPadRight, yPosition);
            Canvas.SetLeft(DPadRight, xPosition + buttonBoxSize.Width);
            
            // Place + - buttons
            yPosition = Canvas.GetTop(ButtonX) - Minus.Height / 2;
            xPosition = Canvas.GetLeft(ButtonY) - Plus.Width / 2;
            Canvas.SetLeft(Plus, xPosition);
            Canvas.SetTop(Plus, yPosition);
            
            xPosition = Canvas.GetLeft(StickLeft) + StickLeft.Width + Minus.Width / 2;
            Canvas.SetLeft(Minus, xPosition);
            Canvas.SetTop(Minus, yPosition);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
        }
    }
}