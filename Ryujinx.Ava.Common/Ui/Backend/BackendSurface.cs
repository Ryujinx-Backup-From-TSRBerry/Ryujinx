using Avalonia;
using Avalonia.Platform;
using System;
using System.Runtime.InteropServices;
using static Ryujinx.Ava.Common.Ui.Backend.Interop;

namespace Ryujinx.Ava.Common.Ui.Backend
{
    public abstract class BackendSurface : IDisposable
    {
        protected IntPtr Display => _display;

        private IntPtr _display = IntPtr.Zero;

        [DllImport("libX11.so.6")]
        public static extern IntPtr XOpenDisplay(IntPtr display);

        [DllImport("libX11.so.6")]
        public static extern int XCloseDisplay(IntPtr display);

        private PixelSize _currentSize;
        public IPlatformNativeSurfaceHandle Handle { get; protected set; }

        public bool IsDisposed { get; private set; }

        public BackendSurface(IPlatformNativeSurfaceHandle handle)
        {
            Handle = handle;

            if (OperatingSystem.IsLinux())
            {
                _display = XOpenDisplay(IntPtr.Zero);
            }
        }

        public PixelSize Size
        {
            get
            {
                return Handle.Size;
            }
        }

        public PixelSize CurrentSize => _currentSize;

        public virtual void Dispose()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BackendSurface));
            }

            IsDisposed = true;

            if (_display != IntPtr.Zero)
            {
                XCloseDisplay(_display);
            }
        }
    }
}