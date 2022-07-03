using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Ryujinx.Input;
using System;

namespace Ryujinx.Rsc.Controls
{
    public partial class VirtualStick : UserControl, IVirtualControl
    {
        private int _id = -1;
        private bool _pressed;
        private GamepadButtonInputId buttonInputId;

        public event EventHandler<IVirtualControl.VirualInputEventArgs> Input;

        public VirtualStick()
        {
            InitializeComponent();
            IObservable<Rect> resizeObservable = this.GetObservable(BoundsProperty);

            resizeObservable.Subscribe(Resized);

            IObservable<bool> pressedObservable = Button.GetObservable(Button.IsPressedProperty);

            pressedObservable.Subscribe(PressedChanged);
        }

        private void PressedChanged(bool isPressed)
        {
            Input?.Invoke(this, new IVirtualControl.VirualInputEventArgs() { IsPressed = isPressed, Button = ButtonInputId.ToString() });
        }

        protected virtual void Resized(Rect rect)
        {
            SetCirclePosition();
            MoveThumb(Circle.Bounds.Center - Circle.Bounds.Position);
        }

        private void SetCirclePosition()
        {
            if (Circle != null)
            {
                var halfSize = Circle.Bounds.Size / 2;
                var center = Bounds.Center - Bounds.TopLeft;
                Canvas.SetLeft(Circle, center.X - halfSize.Width);
                Canvas.SetTop(Circle, center.Y - halfSize.Height);

                switch (StickInputId)
                {
                    case StickInputId.Left:
                        Canvas.SetLeft(Button, 0);
                        break;
                    case StickInputId.Right:
                        Canvas.SetLeft(Button, Bounds.Width - Button.Bounds.Width);
                        break;
                }
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (_id == -1)
            {
                _id = e.Pointer.Id;
                _pressed = true;
                MoveThumb(e.GetPosition(Circle));
                Button.IsVisible = false;
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (_pressed && e.Pointer.Id == _id)
            {
                MoveThumb(e.GetPosition(Circle));
                Button.IsVisible = false;
            }
        }

        private double ClampAxis(double value, double other)
        {
            if (Math.Sign(value) < 0)
            {
                return Math.Max(value, other);
            }

            return Math.Min(value, other);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (e.Pointer.Id == _id)
            {
                _pressed = false;
                MoveThumb(Circle.Bounds.Center - Circle.Bounds.Position);
                _id = -1;
                Button.IsVisible = true;
            }
        }

        private void MoveThumb(Point point)
        {
            SetCirclePosition();

            if (Thumb != null)
            {
                var halfWidth = Thumb.Bounds.Size / 2;
                var radius = Circle.Bounds.Size / 2;
                var x = point.X;
                var y = point.Y;
                var clamped = new Vector();
                var relativeToCenter = new Vector(x - radius.Width, y - radius.Height);
                if (relativeToCenter.Length != 0)
                {
                    var normalizedX = relativeToCenter.Normalize() * radius.Width;
                    var normalizedY = relativeToCenter.Normalize() * radius.Height;
                    clamped = new Vector(ClampAxis(relativeToCenter.X, normalizedX.X), ClampAxis(relativeToCenter.Y, normalizedY.Y));
                }
                Input?.Invoke(this, new IVirtualControl.VirualInputEventArgs() { StickValue = new Vector(clamped.X / radius.Width, clamped.Y * -1 / radius.Height) });
                clamped += (Bounds.Center - Bounds.Position);
                Canvas.SetLeft(Thumb, clamped.X - halfWidth.Width);
                Canvas.SetTop(Thumb, clamped.Y - halfWidth.Height);
            }
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            MoveThumb(Bounds.Center - Bounds.Position);
        }

        public StickInputId StickInputId { get; set; }
        public GamepadButtonInputId ButtonInputId
        {
            get => buttonInputId; set
            {
                buttonInputId = value;

                switch (StickInputId)
                {
                    case StickInputId.Left:
                        StickLabel.Content = "L";
                        break;
                    case StickInputId.Right:
                        StickLabel.Content = "R";
                        break;
                }
            }
        }

        public bool IsStick => true;
    }
}