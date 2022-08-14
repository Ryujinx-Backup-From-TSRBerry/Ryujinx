using Android.App;
using Android.Views;
using Avalonia;
using FluentAvalonia.Core;
using Ryujinx.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.Rsc.Mobile.Input
{
    public class AndroidControllerDriver : IGamepadDriver
    {
        private string[] _blacklistDevices = { "uinput-fpc" };
        private Dictionary<int, bool[]> _buttonInputs;

        private Dictionary<int, Vector[]> _stickInputs;
        public AndroidControllerDriver(MainActivity activity)
        {
            Activity = activity;
            Activity.KeyDispatched += Activity_KeyDispatched;
            Activity.MotionDispatched += Activity_MotionDispatched;
            _buttonInputs = new Dictionary<int, bool[]>();
            _stickInputs = new Dictionary<int, Vector[]>();
            RefreshControllers();
        }

        private void Activity_MotionDispatched(object sender, MotionEvent e)
        {
            if (!_stickInputs.ContainsKey(e.DeviceId))
            {
                RefreshControllers();
            }

            switch (e.Action)
            {
                case MotionEventActions.Move:
                    var action = e.Action;
                    var leftStickX = e.GetAxisValue(Axis.X);
                    var leftStickY = e.GetAxisValue(Axis.Y);
                    var rightStickX = e.GetAxisValue(Axis.Z);
                    var rightStickY = e.GetAxisValue(Axis.Rz);
                    SetStickAxis(StickInputId.Left, new Vector(leftStickX, leftStickY), e.DeviceId);
                    SetStickAxis(StickInputId.Right, new Vector(rightStickX, rightStickY), e.DeviceId);
                    break;
            }
        }

        private void Activity_KeyDispatched(object sender, KeyEvent e)
        {
            if (!_buttonInputs.ContainsKey(e.DeviceId))
            {
                RefreshControllers();
            }

            switch(e.Action)
            {
                case KeyEventActions.Down:
                    SetButtonPressed(GetGamepadButtonInputId(e.KeyCode), e.DeviceId);
                    break;
                case KeyEventActions.Up:
                    SetButtonReleased(GetGamepadButtonInputId(e.KeyCode), e.DeviceId);
                    break;
            }
        }

        private void RefreshControllers()
        {
            var oldIds = _buttonInputs.Keys;
            _buttonInputs.Clear();
            _stickInputs.Clear();

            var ids = InputDevice.GetDeviceIds();
            foreach(var id in ids)
            {
                var device = InputDevice.GetDevice(id);
                if (device.Sources.HasFlag(InputSourceType.Gamepad) && device.Sources.HasFlag(InputSourceType.Joystick) && !_blacklistDevices.Contains(device.Name))
                {
                    _buttonInputs.Add(id, new bool[(int)GamepadButtonInputId.Count]);
                    _stickInputs.Add(id, new Vector[(int)StickInputId.Count]);
                }
            }

            var disconnected = oldIds.Where(x => !_buttonInputs.ContainsKey(x));
            foreach(var id in disconnected)
            {
                OnGamepadDisconnected?.Invoke(id.ToString());
            }

            var connectedIds = _buttonInputs.Keys.ToArray();
            foreach(var id in connectedIds)
            {
                OnGamepadConnected?.Invoke(id.ToString());
            }
        }

        private GamepadButtonInputId GetGamepadButtonInputId(Keycode keycode)
        {
            switch (keycode)
            {
                case Keycode.ButtonA:
                    return GamepadButtonInputId.A;
                case Keycode.ButtonB:
                    return GamepadButtonInputId.B;
                case Keycode.ButtonX:
                    return GamepadButtonInputId.X;
                case Keycode.ButtonY:
                    return GamepadButtonInputId.Y;
                case Keycode.ButtonL1:
                    return GamepadButtonInputId.LeftShoulder;
                case Keycode.ButtonL2:
                    return GamepadButtonInputId.LeftTrigger;
                case Keycode.ButtonR1:
                    return GamepadButtonInputId.RightShoulder;
                case Keycode.ButtonR2:
                    return GamepadButtonInputId.RightTrigger;
                case Keycode.ButtonThumbl:
                    return GamepadButtonInputId.LeftStick;
                case Keycode.ButtonThumbr:
                    return GamepadButtonInputId.RightStick;
                case Keycode.DpadUp:
                    return GamepadButtonInputId.DpadUp;
                case Keycode.DpadDown:
                    return GamepadButtonInputId.DpadDown;
                case Keycode.DpadLeft:
                    return GamepadButtonInputId.DpadLeft;
                case Keycode.DpadRight:
                    return GamepadButtonInputId.DpadRight;
                case Keycode.ButtonStart:
                    return GamepadButtonInputId.Plus;
                case Keycode.ButtonSelect:
                    return GamepadButtonInputId.Minus;
                case Keycode.Home:
                    return GamepadButtonInputId.Guide;
                default:
                    return GamepadButtonInputId.Unbound;
            }
        }

        public void Dispose()
        {
            Activity.KeyDispatched -= Activity_KeyDispatched;
            Activity.MotionDispatched -= Activity_MotionDispatched;
        }

        public string DriverName { get; } = "AndroidController";
        public ReadOnlySpan<string> GamepadsIds => new[] { "0" };

        public MainActivity Activity { get; }

        public event Action<string> OnGamepadConnected;
        public event Action<string> OnGamepadDisconnected;

        public IGamepad GetGamepad(string id)
        {
            return new AndroidController(this, id);
        }

        public void SetStickAxis(StickInputId stick, Vector axes, int deviceId)
        {
            _stickInputs[deviceId][(int)stick] = axes;
        }

        public void SetButtonPressed(GamepadButtonInputId button, int deviceId)
        {
            _buttonInputs[deviceId][(int)button] = true;
        }

        public void SetButtonReleased(GamepadButtonInputId button, int deviceId)
        {
            _buttonInputs[deviceId][(int)button] = false;
        }

        public Vector GetStickAxes(StickInputId stick, string deviceId)
        {
            if (_buttonInputs.Count == 0)
            {
                return default;
            }
            int id = deviceId == "default" ? _stickInputs.Keys.First() : int.Parse(deviceId);
            return _stickInputs[id][(int)stick];
        }

        public bool IsButtonPressed(GamepadButtonInputId button, string deviceId)
        {
            if(_buttonInputs.Count == 0)
            {
                return false;
            }
            int id = deviceId == "default" ? _buttonInputs.Keys.First() : int.Parse(deviceId);
            return _buttonInputs[id][(int)button];
        }
    }
}