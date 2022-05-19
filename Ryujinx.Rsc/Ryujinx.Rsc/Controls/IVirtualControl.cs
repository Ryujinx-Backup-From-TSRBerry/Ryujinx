using Avalonia;
using Ryujinx.Input;
using System;

namespace Ryujinx.Rsc.Controls
{
    public interface IVirtualControl
    {
        public StickInputId StickInputId { get; set; }
        public GamepadButtonInputId ButtonInputId { get; set; }
        public bool IsStick { get; }

        public event EventHandler<VirualInputEventArgs> Input;

        public class VirualInputEventArgs : EventArgs
        {
            public Vector StickValue { get; set; }
            public bool IsPressed { get; set; }
            public string Button { get; set; }
        }
    }
}