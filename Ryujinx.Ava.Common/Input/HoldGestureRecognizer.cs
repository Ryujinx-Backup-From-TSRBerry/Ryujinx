using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System.Timers;

namespace Ryujinx.Ava.Common.Input
{
    public class HoldGestureRecognizer : StyledElement, IGestureRecognizer
    {
        private int _holdDelayMs = 500;
        private Point _startLocation;
        
        public static readonly RoutedEvent<HoldGestureEventArgs> HoldGestureEvent = RoutedEvent.Register<Control, HoldGestureEventArgs>("HoldGesture", RoutingStrategies.Direct);
        
        /// <summary>
        /// Defines the <see cref="HoldDelayMs"/> property.
        /// </summary>
        public static readonly DirectProperty<HoldGestureRecognizer, int> HoldDelayMsProperty =
            AvaloniaProperty.RegisterDirect<HoldGestureRecognizer, int>(
                nameof(HoldDelayMs),
                o => o.HoldDelayMs,
                (o, v) => o.HoldDelayMs = v);

        private IGestureRecognizerActionsDispatcher _actions;
        private IInputElement _target;
        private bool _isHolding;
        private Timer _holdTimer;

        /// <summary>
        /// Gets or sets a value indicating how long to hold for before event is triggered.
        /// </summary>
        public int HoldDelayMs
        {
            get => _holdDelayMs;
            set => SetAndRaise(HoldDelayMsProperty, ref _holdDelayMs, value);
        }
        
        public void Initialize(IInputElement target, IGestureRecognizerActionsDispatcher actions)
        {
            _target = target;
            _actions = actions;
            _holdTimer = new Timer(_holdDelayMs);
            _holdTimer.Elapsed += HoldTimer_Elapsed;
            _holdTimer.AutoReset = false;
        }

        private void HoldTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _holdTimer.Stop();
            if (_isHolding)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _target!.RaiseEvent(new HoldGestureEventArgs(_holdDelayMs));
                });
            }
        }

        public void PointerPressed(PointerPressedEventArgs e)
        {
            _isHolding = true;
            _startLocation = e.GetPosition(_target);
            _holdTimer.Interval = _holdDelayMs;
            _holdTimer.Start();
        }

        public void PointerReleased(PointerReleasedEventArgs e)
        {
            _isHolding = false;
            _holdTimer.Stop();
        }

        public void PointerMoved(PointerEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(_target).Position;
            var difference = currentPoint - _startLocation;
            if (new Vector(difference.X, difference.Y).Length > 5)
            {
                _holdTimer.Stop();
                _isHolding = false;
            }
        }

        public void PointerCaptureLost(IPointer pointer)
        {
            _holdTimer.Stop();
            _isHolding = false;
        }
    }

    public class HoldGestureEventArgs : RoutedEventArgs
    {
        public HoldGestureEventArgs(int holdDuration)
        {
            HoldDuration = holdDuration;
            RoutedEvent = HoldGestureRecognizer.HoldGestureEvent;
        }

        public int HoldDuration { get; }
    }
}