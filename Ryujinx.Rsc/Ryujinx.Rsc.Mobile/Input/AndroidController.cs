using Ryujinx.Common.Configuration.Hid;
using Ryujinx.Input;
using System;
using System.Numerics;

namespace Ryujinx.Rsc.Mobile.Input
{
    public class AndroidController : IGamepad
    {
        private readonly AndroidControllerDriver _driver;

        public AndroidController(AndroidControllerDriver driver, string id)
        {
            _driver = driver;
            Id = id;
        }

        public void Dispose() { }

        public GamepadFeaturesFlag Features { get; }
        public string Id { get; }

        private int _id;

        public string Name { get; }
        public bool IsConnected { get; }

        public bool IsPressed(GamepadButtonInputId inputId)
        {
            return _driver.IsButtonPressed(inputId, Id);
        }

        public (float, float) GetStick(StickInputId inputId)
        {
            var input = _driver.GetStickAxes(inputId, Id);

            return ((float)input.X, (float)input.Y);
        }

        public Vector3 GetMotionData(MotionInputId inputId)
        {
            return new Vector3();
        }

        public void SetTriggerThreshold(float triggerThreshold)
        {
            //throw new System.NotImplementedException();
        }

        public void SetConfiguration(InputConfig configuration)
        {
            //throw new System.NotImplementedException();
        }

        public void Rumble(float lowFrequency, float highFrequency, uint durationMs)
        {
            //throw new System.NotImplementedException();
        }

        public GamepadStateSnapshot GetMappedStateSnapshot()
        {
            GamepadStateSnapshot result = default;

            foreach (var button in Enum.GetValues<GamepadButtonInputId>())
            {
                // Do not touch state of button already pressed
                if (button != GamepadButtonInputId.Count && !result.IsPressed(button))
                {
                    result.SetPressed(button, IsPressed(button));
                }
            }

            (float leftStickX, float leftStickY) = GetStick(StickInputId.Left);
            (float rightStickX, float rightStickY) = GetStick(StickInputId.Right);

            result.SetStick(StickInputId.Left, leftStickX, leftStickY);
            result.SetStick(StickInputId.Right, rightStickX, rightStickY);

            return result;
        }

        public GamepadStateSnapshot GetStateSnapshot()
        {
            return new GamepadStateSnapshot();
        }
    }
}