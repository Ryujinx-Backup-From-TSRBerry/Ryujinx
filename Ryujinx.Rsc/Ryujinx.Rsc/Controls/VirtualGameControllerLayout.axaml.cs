using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Ryujinx.Common.Logging;
using Ryujinx.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Ryujinx.Rsc.Controls
{
    public partial class VirtualGameControllerLayout : UserControl
    {
        private static string MapPath => Path.Combine(new FileInfo(App.ConfigurationPath).Directory.FullName,
        "virtualpad.json");
        public AvaloniaVirtualControllerDriver Controller => AvaloniaVirtualControllerDriver.Instance;
        public bool IsEditMode { get; set; }

        private Point _translatedPoint;
        private IVirtualControl _draggedControl;
        private bool _isDragging;
        private bool _isDefault;
        private bool _isModified;

        public Action OnBackRequested;

        public VirtualGameControllerLayout()
        {
            InitializeComponent();
            BindButtons();

            IObservable<Rect> resizeObservable = this.GetObservable(BoundsProperty);

            resizeObservable.Subscribe(Resized);
        }

        protected virtual void Resized(Rect rect)
        {
            if (_isDefault)
            {
                SetDefaultPositions();
            }
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);

            if (IsEditMode)
            {
                AddHandler(PointerPressedEvent, LayoutPointerPressed, RoutingStrategies.Tunnel | RoutingStrategies.Direct);
                AddHandler(PointerReleasedEvent, LayoutPointerReleased, RoutingStrategies.Tunnel | RoutingStrategies.Direct);
                AddHandler(PointerMovedEvent, LayoutPointerMoved, RoutingStrategies.Tunnel | RoutingStrategies.Direct);
            }
            else
            {
                BackButton.IsVisible = false;
                SaveButton.IsVisible = false;
                ResetButton.IsVisible = false;
                DefaultButton.IsVisible = false;
            }

            try
            {
                if (!LoadSavedPoints())
                {
                    _isDefault = true;
                }
            }
            catch (Exception ex)
            {
                _isDefault = true;
                SetDefaultPositions();
                Logger.Warning?.Print(LogClass.Hid, "Failed to load saved map points for virtual controller");
            }
        }

        public bool LoadSavedPoints()
        {
            if (File.Exists(MapPath))
            {
                List<Vectord> mapPoints = JsonSerializer.Deserialize<List<Vectord>>(File.ReadAllText(MapPath));
                LoadMapPoints(mapPoints);
                return true;
            }

            return false;
        }

        private void LayoutPointerMoved(object sender, PointerEventArgs e)
        {
            if (IsEditMode)
            {
                if (_isDragging && _draggedControl != null)
                {
                    var point = e.GetCurrentPoint(this).Position - _translatedPoint;
                    Canvas.SetLeft(_draggedControl as Control, point.X);
                    Canvas.SetTop(_draggedControl as Control, point.Y);
                    _isModified = true;
                    e.Handled = true;
                }
            }
        }

        private void LayoutPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (IsEditMode)
            {
                if (_isDragging)
                {
                    e.Handled = true;
                }

                _isDragging = false;
                _translatedPoint = new Point();
                _draggedControl = null;
            }
        }

        private void LayoutPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (IsEditMode)
            {
                var point = e.GetCurrentPoint(this).Position;
                var child = this.GetVisualDescendants().FirstOrDefault(x => (x is IVirtualControl) && x.Bounds.Contains(point));
                if (child != null)
                {
                    _isDragging = true;
                    _translatedPoint = point - child.Bounds.Position;
                    _draggedControl = child as IVirtualControl;
                    e.Handled = true;
                }
            }
        }

        private void BindButtons()
        {
            StickLeft.StickInputId = StickInputId.Left;
            StickRight.StickInputId = StickInputId.Right;
            StickLeft.ButtonInputId = GamepadButtonInputId.LeftStick;
            StickRight.ButtonInputId = GamepadButtonInputId.RightStick;
            ButtonA.ButtonInputId = GamepadButtonInputId.A;
            ButtonB.ButtonInputId = GamepadButtonInputId.B;
            ButtonX.ButtonInputId = GamepadButtonInputId.X;
            ButtonY.ButtonInputId = GamepadButtonInputId.Y;
            DpadUp.ButtonInputId = GamepadButtonInputId.DpadUp;
            DPadDown.ButtonInputId = GamepadButtonInputId.DpadDown;
            DPadLeft.ButtonInputId = GamepadButtonInputId.DpadLeft;
            DPadRight.ButtonInputId = GamepadButtonInputId.DpadRight;
            Minus.ButtonInputId = GamepadButtonInputId.Minus;
            Plus.ButtonInputId = GamepadButtonInputId.Plus;
            LeftShoulder.ButtonInputId = GamepadButtonInputId.LeftShoulder;
            RightShoulder.ButtonInputId = GamepadButtonInputId.RightShoulder;
            LeftTrigger.ButtonInputId = GamepadButtonInputId.LeftTrigger;
            RightTrigger.ButtonInputId = GamepadButtonInputId.RightTrigger;

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

        private void LoadMapPoints(List<Vectord> mapPoints)
        {
            Canvas.SetLeft(StickLeft, mapPoints[0].X);
            Canvas.SetTop(StickLeft, mapPoints[0].Y);
            Canvas.SetLeft(StickRight, mapPoints[1].X);
            Canvas.SetTop(StickRight, mapPoints[1].Y);
            Canvas.SetLeft(ButtonA, mapPoints[2].X);
            Canvas.SetTop(ButtonA, mapPoints[2].Y);
            Canvas.SetLeft(ButtonB, mapPoints[3].X);
            Canvas.SetTop(ButtonB, mapPoints[3].Y);
            Canvas.SetLeft(ButtonX, mapPoints[4].X);
            Canvas.SetTop(ButtonX, mapPoints[4].Y);
            Canvas.SetLeft(ButtonY, mapPoints[5].X);
            Canvas.SetTop(ButtonY, mapPoints[5].Y);
            Canvas.SetLeft(DpadUp, mapPoints[6].X);
            Canvas.SetTop(DpadUp, mapPoints[6].Y);
            Canvas.SetLeft(DPadDown, mapPoints[7].X);
            Canvas.SetTop(DPadDown, mapPoints[7].Y);
            Canvas.SetLeft(DPadLeft, mapPoints[8].X);
            Canvas.SetTop(DPadLeft, mapPoints[8].Y);
            Canvas.SetLeft(DPadRight, mapPoints[9].X);
            Canvas.SetTop(DPadRight, mapPoints[9].Y);
            Canvas.SetLeft(Plus, mapPoints[10].X);
            Canvas.SetTop(Plus, mapPoints[10].Y);
            Canvas.SetLeft(Minus, mapPoints[11].X);
            Canvas.SetTop(Minus, mapPoints[11].Y);
            Canvas.SetLeft(LeftShoulder, mapPoints[12].X);
            Canvas.SetTop(LeftShoulder, mapPoints[12].Y);
            Canvas.SetLeft(RightShoulder, mapPoints[13].X);
            Canvas.SetTop(RightShoulder, mapPoints[13].Y);
            Canvas.SetLeft(LeftTrigger, mapPoints[14].X);
            Canvas.SetTop(LeftTrigger, mapPoints[14].Y);
            Canvas.SetLeft(RightTrigger, mapPoints[15].X);
            Canvas.SetTop(RightTrigger, mapPoints[15].Y);
        }

        private void Button_Input(object sender, IVirtualControl.VirualInputEventArgs e)
        {
            if (!IsEditMode && sender is IVirtualControl control)
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
            if (!IsEditMode && sender is IVirtualControl control)
            {
                if (string.IsNullOrWhiteSpace(e.Button))
                {
                    Controller.SetStickAxis(control.StickInputId, e.StickValue);
                }
                else
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
        }

        public void SetDefaultPositions()
        {
            var size = Bounds.Size;
            if (size.Width == 0 || size.Height == 0)
            {
                return;
            }
            
            double padding = 0.09;

            var buttonBoxSize = new Size(100, 100);
            var halfBox = buttonBoxSize / 2;

            double xPosition = 0, yPosition = 0;
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

            // Place Right Buttons
            xPosition = size.Width - padding * size.Width - halfBox.Width - ButtonX.Width / 2;
            yPosition = Canvas.GetTop(DpadUp) - padding * size.Height - buttonBoxSize.Height;
            Canvas.SetTop(ButtonX, yPosition);
            Canvas.SetLeft(ButtonX, xPosition);
            Canvas.SetTop(ButtonB, yPosition + buttonBoxSize.Height);
            Canvas.SetLeft(ButtonB, xPosition);

            xPosition = size.Width - padding * size.Width - ButtonX.Width / 2;
            yPosition = yPosition + halfBox.Height;
            Canvas.SetTop(ButtonA, yPosition);
            Canvas.SetLeft(ButtonA, xPosition);
            Canvas.SetTop(ButtonY, yPosition);
            Canvas.SetLeft(ButtonY, xPosition - buttonBoxSize.Width);
            
            // Place sticks
            xPosition = padding * size.Width;
            yPosition = Canvas.GetTop(ButtonY) + ButtonY.Height / 2 - StickLeft.Height / 2;
            Canvas.SetTop(StickLeft, yPosition);
            Canvas.SetLeft(StickLeft, xPosition + halfBox.Width - StickLeft.Width / 2);
            
            yPosition = Canvas.GetTop(DPadRight) + DPadRight.Height / 2 - StickRight.Height / 2;
            Canvas.SetTop(StickRight, yPosition);
            Canvas.SetLeft(StickRight, size.Width - xPosition - StickRight.Width / 2 - halfBox.Width);
            
            // Place + - buttons
            yPosition = Canvas.GetTop(ButtonX) - Minus.Height / 2;
            xPosition = Canvas.GetLeft(ButtonY) - Plus.Width / 2;
            Canvas.SetLeft(Plus, xPosition);
            Canvas.SetTop(Plus, yPosition);
            
            xPosition = Canvas.GetLeft(DPadRight) + Minus.Width / 2;
            Canvas.SetLeft(Minus, xPosition);
            Canvas.SetTop(Minus, yPosition);
            
            // Place shoulder buttons
            yPosition = Canvas.GetTop(Minus) - (LeftShoulder.Height / 2);
            xPosition = padding * size.Width;
            Canvas.SetLeft(LeftShoulder, xPosition);
            Canvas.SetTop(LeftShoulder, yPosition);
            
            xPosition = size.Width - padding * size.Width - RightShoulder.Width;
            Canvas.SetLeft(RightShoulder, xPosition);
            Canvas.SetTop(RightShoulder, yPosition);

            bool xAlign = false;
            yPosition = Canvas.GetTop(LeftShoulder) - LeftTrigger.Height - padding * LeftTrigger.Height;
            if (yPosition < padding * size.Height)
            {
                xAlign = true;
                yPosition = padding * size.Height;
            }
            xPosition = xAlign ? Canvas.GetLeft(LeftShoulder) + LeftShoulder.Width + padding / 2 * size.Width : size.Width - padding * size.Width - LeftTrigger.Width;
            Canvas.SetLeft(LeftTrigger, xPosition);
            Canvas.SetTop(LeftTrigger, yPosition);
            
            xPosition = xAlign ? Canvas.GetLeft(RightShoulder) - padding / 2 * size.Width - RightTrigger.Width : padding * size.Width;
            Canvas.SetLeft(RightTrigger, xPosition);
            Canvas.SetTop(RightTrigger, yPosition);
        }

        private void SavePositions()
        {
            List<Vectord> positions = new List<Vectord>();
            positions.Add(new Vectord(Canvas.GetLeft(StickLeft), Canvas.GetTop(StickLeft)));
            positions.Add(new Vectord(Canvas.GetLeft(StickRight), Canvas.GetTop(StickRight)));
            positions.Add(new Vectord(Canvas.GetLeft(ButtonA), Canvas.GetTop(ButtonA)));
            positions.Add(new Vectord(Canvas.GetLeft(ButtonB), Canvas.GetTop(ButtonB)));
            positions.Add(new Vectord(Canvas.GetLeft(ButtonX), Canvas.GetTop(ButtonX)));
            positions.Add(new Vectord(Canvas.GetLeft(ButtonY), Canvas.GetTop(ButtonY)));
            positions.Add(new Vectord(Canvas.GetLeft(DpadUp), Canvas.GetTop(DpadUp)));
            positions.Add(new Vectord(Canvas.GetLeft(DPadDown), Canvas.GetTop(DPadDown)));
            positions.Add(new Vectord(Canvas.GetLeft(DPadLeft), Canvas.GetTop(DPadLeft)));
            positions.Add(new Vectord(Canvas.GetLeft(DPadRight), Canvas.GetTop(DPadRight)));
            positions.Add(new Vectord(Canvas.GetLeft(Plus), Canvas.GetTop(Plus)));
            positions.Add(new Vectord(Canvas.GetLeft(Minus), Canvas.GetTop(Minus)));
            positions.Add(new Vectord(Canvas.GetLeft(LeftShoulder), Canvas.GetTop(LeftShoulder)));
            positions.Add(new Vectord(Canvas.GetLeft(RightShoulder), Canvas.GetTop(RightShoulder)));
            positions.Add(new Vectord(Canvas.GetLeft(LeftTrigger), Canvas.GetTop(LeftTrigger)));
            positions.Add(new Vectord(Canvas.GetLeft(RightTrigger), Canvas.GetTop(RightTrigger)));
            File.WriteAllText(MapPath, JsonSerializer.Serialize(positions));
        }
        
        private struct Vectord
        {
            public double X { get; set; }
            public double Y { get; set; }
            public Vectord(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        private void SaveButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (_isModified)
            {
                _isDefault = false;
                SavePositions();
            }
            else if (_isDefault)
            {
                File.Delete(MapPath);
            }

            _isModified = false;
        }

        public void Reload()
        {
            if (!LoadSavedPoints())
            {
                _isDefault = true;
                SetDefaultPositions();
            }
        }

        private void ResetButton_Clicked(object sender, RoutedEventArgs e)
        {
            Reload();

            _isModified = false;
        }

        private void DefaultButton_Clicked(object sender, RoutedEventArgs e)
        {
            _isDefault = true;
            SetDefaultPositions();
        }

        private void BackButton_Clicked(object sender, RoutedEventArgs e)
        {
            OnBackRequested?.Invoke();
        }
    }
}