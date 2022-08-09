using Silk.NET.Core.Contexts;
using Silk.NET.Vulkan;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Rsc.Mobile.Helper
{
    public class VulkanLoader : IDisposable
    {
        private const string DllName = "libvulkan_loader.so";

        //Native functions
        [DllImport(DllName, EntryPoint = "open")]
        internal static extern IntPtr Open(string name);

        [DllImport(DllName, EntryPoint = "get_device_proc_address")]
        internal static extern IntPtr GetDeviceProcAddress(IntPtr library, IntPtr device, string name);

        [DllImport(DllName, EntryPoint = "get_instance_proc_address")]
        internal static extern IntPtr GetInstanceProcAddress(IntPtr library, IntPtr instance, string name);

        [DllImport(DllName, EntryPoint = "close")]
        internal static extern IntPtr Close(IntPtr library);

        private IntPtr loadedLibrary = IntPtr.Zero;

        private delegate IntPtr GetDeviceProcAddressDelegate(IntPtr device, string name);
        private delegate IntPtr GetInstanceProcAddressDelegate(IntPtr instance, string name);

        public void Dispose()
        {
            if (loadedLibrary == IntPtr.Zero)
            {
                Close(loadedLibrary);
                loadedLibrary = IntPtr.Zero;
            }
        }

        public VulkanLoader(string path)
        {
            loadedLibrary = Open(path);

            if (loadedLibrary == IntPtr.Zero)
            {
                throw new Exception($"Failed to load {path}");
            }
        }

        public Vk GetApi()
        {
            var ctx = new MultiNativeContext(new INativeContext[1]);
            var ret = new Vk(ctx);
            ctx.Contexts[0] = new LamdaNativeContext
            (
                x =>
                {
                    if (x.EndsWith("ProcAddr"))
                    {
                        return default;
                    }

                    nint ptr = default;
                    ptr = GetDeviceProcAddress(loadedLibrary, ret.CurrentDevice.GetValueOrDefault().Handle, x);
                    if (ptr != default)
                    {
                        return ptr;
                    }

                    ptr = GetInstanceProcAddress(loadedLibrary, ret.CurrentInstance.GetValueOrDefault().Handle, x);
                    return ptr;
                }
            );
            return ret;
        }
    }
}