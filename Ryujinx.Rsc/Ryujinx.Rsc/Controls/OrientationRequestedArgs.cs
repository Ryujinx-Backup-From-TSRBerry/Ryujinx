using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ryujinx.Rsc.Controls
{
    public class OrientationRequestedArgs : EventArgs
    {
        public OrientationRequestedArgs(Orientation orientation)
        {
            Orientation = orientation;

        }
        public Orientation Orientation { get; }
    }

    public enum Orientation
    {
        Normal,
        Portrait,
        Landscape,
    }
}