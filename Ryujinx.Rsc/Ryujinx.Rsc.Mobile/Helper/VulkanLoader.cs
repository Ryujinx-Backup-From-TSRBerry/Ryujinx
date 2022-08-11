using Silk.NET.Core.Contexts;
using Silk.NET.Vulkan;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ryujinx.Rsc.Mobile.Helper
{
    public class VulkanLoader : IDisposable
    {
        private const string DllName = "libadrenotools.so";
        private const int RtldNow = 2;

        [DllImport("libdl.so")]
        private static extern IntPtr dlerror();

        [DllImport(DllName, EntryPoint = "adrenotools_open_libvulkan")]
        internal static extern IntPtr Open(int dlopenMode, int featureFlags,string tmpLibDir, string hookLibDir, string customDriverDir, string customDriverName, string fileRedirectDir);

        private IntPtr loadedLibrary = IntPtr.Zero;

        [DllImport("libvulkan_loader.so", EntryPoint = "get_device_proc_address")]
        internal static extern IntPtr GetDeviceProcAddress(IntPtr library, IntPtr device, string name);

        [DllImport("libvulkan_loader.so", EntryPoint = "get_instance_proc_address")]
        internal static extern IntPtr GetInstanceProcAddress(IntPtr library, IntPtr instance, string name);

        [DllImport("libvulkan_loader.so", EntryPoint = "close")]
        internal static extern IntPtr Close(IntPtr library);


        public void Dispose()
        {
            if (loadedLibrary == IntPtr.Zero)
            {
                Close(loadedLibrary);
                loadedLibrary = IntPtr.Zero;
            }
        }

        public VulkanLoader(string path, string publicPath, string privatePath, string nativeLibPath)
        {
            if (path.Contains(Path.DirectorySeparatorChar))
            {
                var driverName = new FileInfo(path).Name;
                var newDriverPath = Path.Combine(privatePath, "driver");
                Directory.CreateDirectory(newDriverPath);
                var newDriver = Path.Combine(newDriverPath, driverName);
                if(File.Exists(newDriver))
                {
                    File.Delete(newDriver);
                }
                File.Copy(path, newDriver);
                var redirectPath = Path.Combine(publicPath, "Driver", "redirect") + Path.DirectorySeparatorChar.ToString();
                Directory.CreateDirectory(redirectPath);

                loadedLibrary = Open(RtldNow, (int)(AdrenoToolsOptions.CustomDriver), null, nativeLibPath, newDriverPath + Path.DirectorySeparatorChar.ToString(), driverName, null);
            }
        }

        public Vk GetApi()
        {
            if(loadedLibrary == IntPtr.Zero)
            {
                return Vk.GetApi();
            }
            var ctx = new MultiNativeContext(new INativeContext[1]);
            var ret = new Vk(ctx);
            ctx.Contexts[0] = new LamdaNativeContext
            (
                x =>
                {
                    nint ptr = default;
                    ptr = GetDeviceProcAddress(loadedLibrary, ret.CurrentDevice.GetValueOrDefault().Handle, x);
                    if (ptr != default)
                    {
                        return ptr;
                    }

                    ptr = GetInstanceProcAddress(loadedLibrary, ret.CurrentInstance.GetValueOrDefault().Handle, x);

                    if (ptr == default)
                    {
                        ptr = GetInstanceProcAddress(loadedLibrary, IntPtr.Zero, x);
                    }

                    return ptr;
                }
            );
            return ret;
        }

        [Flags]
        enum AdrenoToolsOptions : int
        {
            CustomDriver = 1 << 0,
            FileRedirect = 1 << 1
        }
    }
}