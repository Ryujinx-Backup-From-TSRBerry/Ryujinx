using Avalonia;
using Avalonia.Platform;
using Ryujinx.Ava.Common.Ui.Vulkan;
using Ryujinx.Ava.Common.Ui.Vulkan.Surfaces;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using System;

namespace Ryujinx.Ava.Common.Ui.Backend.Vulkan
{
    internal class VulkanWindowSurface : BackendSurface, IVulkanPlatformSurface
    {
        private IntPtr _currentHandle;

        public float Scaling => (float)Handle.Scaling;

        public PixelSize SurfaceSize => Size;

        public VulkanWindowSurface(IPlatformNativeSurfaceHandle handle) : base(handle){}

        public bool IsCorrupted => _currentHandle != IntPtr.Zero && _currentHandle != Handle.Handle;

        public unsafe SurfaceKHR CreateSurface(VulkanInstance instance)
        {
            _currentHandle = Handle.Handle;

            if (OperatingSystem.IsWindows())
            {
                if (instance.Api.TryGetInstanceExtension(new Instance(instance.Handle), out KhrWin32Surface surfaceExtension))
                {
                    var createInfo = new Win32SurfaceCreateInfoKHR() { Hinstance = 0, Hwnd = Handle.Handle, SType = StructureType.Win32SurfaceCreateInfoKhr };

                    surfaceExtension.CreateWin32Surface(new Instance(instance.Handle), createInfo, null, out var surface).ThrowOnError();

                    return surface;
                }
            }
            else if (OperatingSystem.IsLinux())
            {
                if (instance.Api.TryGetInstanceExtension(new Instance(instance.Handle), out KhrXlibSurface surfaceExtension))
                {
                    var createInfo = new XlibSurfaceCreateInfoKHR()
                    {
                        SType = StructureType.XlibSurfaceCreateInfoKhr,
                        Dpy = (nint*)Display,
                        Window = Handle.Handle
                    };

                    surfaceExtension.CreateXlibSurface(new Instance(instance.Handle), createInfo, null, out var surface).ThrowOnError();

                    return surface;
                }
            }
            else if (OperatingSystem.IsAndroid())
            {
                if (instance.Api.TryGetInstanceExtension(new Instance(instance.Handle), out KhrAndroidSurface surfaceExtension))
                {
                    var createInfo = new AndroidSurfaceCreateInfoKHR()
                    {
                        SType = StructureType.XlibSurfaceCreateInfoKhr,
                        Window = (nint*)Handle.Handle
                    };

                    surfaceExtension.CreateAndroidSurface(new Instance(instance.Handle), createInfo, null, out var surface).ThrowOnError();

                    return surface;
                }
            }

            throw new PlatformNotSupportedException("The current platform does not support surface creation.");
        }
    }
}