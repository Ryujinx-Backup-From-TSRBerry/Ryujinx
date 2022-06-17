using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Ryujinx.Input;
using System;

namespace Ryujinx.Rsc.Controls
{
    public partial class VirtualStick : UserControl, IVirtualControl
    {
        private int _id = -1;
        private bool _pressed;

        public event EventHandler<IVirtualControl.VirualInputEventArgs> Input;

        public VirtualStick()
        {
            InitializeComponent();
            IObservable<Rect> resizeObservable = this.GetObservable(BoundsProperty);

            resizeObservable.Subscribe(Resized);
        }
        
        protected virtual void Resized(Rect rect)
        {
            MoveThumb(Bounds.Center - Bounds.Position);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (_id == -1)
            {
                _id = e.Pointer.Id;
                _pressed = true;
                MoveThumb(e.GetPosition(Layout));
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (_pressed && e.Pointer.Id == _id)
            {
                MoveThumb(e.GetPosition(Layout));
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
                MoveThumb(Bounds.Center - Bounds.Position);
                _id = -1;
            }
        }

        private void MoveThumb(Point point)
        {
            if (Thumb != null)
            {
                var halfWidth = Thumb.Bounds.Width / 2;
                var radius = Bounds.Width / 2;
                var x = point.X;
                var y = point.Y;
                var clamped = new Vector();
                var relativeToCenter = new Vector(x - radius, y - radius);
                if (relativeToCenter.Length != 0)
                {
                    var normalized = relativeToCenter.Normalize() * radius;
                    clamped = new Vector(ClampAxis(relativeToCenter.X, normalized.X), ClampAxis(relativeToCenter.Y, normalized.Y));
                }
                Input?.Invoke(this, new IVirtualControl.VirualInputEventArgs() { StickValue = new Vector(clamped.X, clamped.Y * -1 ) / radius});
                clamped += (Bounds.Center - Bounds.Position);
                Canvas.SetLeft(Thumb, clamped.X - halfWidth);
                Canvas.SetTop(Thumb, clamped.Y - halfWidth);
            }
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            MoveThumb(Bounds.Center - Bounds.Position);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Thumb = this.FindControl<Ellipse>("Thumb");
            Layout = this.FindControl<Canvas>("Layout");
        }

        public StickInputId StickInputId { get; set; }
        public GamepadButtonInputId ButtonInputId { get; set; }
        public bool IsStick => true;
    }
}