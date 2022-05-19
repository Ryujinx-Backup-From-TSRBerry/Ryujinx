using Avalonia;
using Ryujinx.Input;
using System;

namespace Ryujinx.Rsc
{
    public class AvaloniaVirtualControllerDriver : IGamepadDriver
    {
        private static AvaloniaVirtualControllerDriver _instance;
        public static AvaloniaVirtualControllerDriver Instance{
            get
            {
                if (_instance == null)
                {
                    _instance = new AvaloniaVirtualControllerDriver();
                }

                return _instance;
            }
        }

        private Vector[] _stickInputs = new Vector[(int)StickInputId.Count];
        private bool[] _buttonInputs = new bool[(int)GamepadButtonInputId.Count];

        public void Dispose() { }

        public string DriverName { get; } = "VirtualController";
        public ReadOnlySpan<string> GamepadsIds => new[] { "0" };

        public event Action<string> OnGamepadConnected;
        public event Action<string> OnGamepadDisconnected;
        public IGamepad GetGamepad(string id)
        {
            return new AvaloniaVirtualController(this);
        }

        public void SetStickAxis(StickInputId stick, Vector axes)
        {
            _stickInputs[(int)stick] = axes;
        }

        public void SetButtonPressed(GamepadButtonInputId button)
        {
            _buttonInputs[(int)button] = true;
        }

        public void SetButtonReleased(GamepadButtonInputId button)
        {
            _buttonInputs[(int)button] = false;
        }

        public Vector GetStickAxes(StickInputId stick)
        {
            return _stickInputs[(int)stick];
        }

        public bool IsButtonPressed(GamepadButtonInputId button)
        {
            return _buttonInputs[(int)button];
        }
    }
}