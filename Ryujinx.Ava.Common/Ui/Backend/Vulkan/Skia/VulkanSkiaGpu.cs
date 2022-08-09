using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Skia;
using Avalonia.X11;
using Ryujinx.Ava.Common.Ui.Vulkan;
using Silk.NET.Vulkan;
using SkiaSharp;

namespace Ryujinx.Ava.Common.Ui.Backend.Vulkan
{
    public class VulkanSkiaGpu : ISkiaGpu
    {
        private readonly VulkanPlatformInterface _vulkan;
        private readonly long? _maxResourceBytes;
        private GRVkBackendContext _grVkBackend;
        private bool _initialized;

        public GRContext GrContext { get; private set; }
        public static Func<Vk> GetApiFunction { get; set; }

        public VulkanSkiaGpu(long? maxResourceBytes)
        {
            _vulkan = AvaloniaLocator.Current.GetService<VulkanPlatformInterface>();
            _maxResourceBytes = maxResourceBytes;
        }

        private void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            GRVkGetProcedureAddressDelegate getProc = (string name, IntPtr instanceHandle, IntPtr deviceHandle) =>
            {
                IntPtr addr = IntPtr.Zero;

                if (deviceHandle != IntPtr.Zero)
                {
                    addr = _vulkan.Device.Api.GetDeviceProcAddr(new Device(deviceHandle), name);

                    if (addr != IntPtr.Zero)
                    {
                        return addr;
                    }

                    addr = _vulkan.Device.Api.GetDeviceProcAddr(new Device(_vulkan.Device.Handle), name);

                    if (addr != IntPtr.Zero)
                    {
                        return addr;
                    }
                }

                addr = _vulkan.Device.Api.GetInstanceProcAddr(new Instance(_vulkan.Instance.Handle), name);

                if (addr == IntPtr.Zero)
                {
                    addr = _vulkan.Device.Api.GetInstanceProcAddr(new Instance(instanceHandle), name);
                }

                return addr;
            };

            _grVkBackend = new GRVkBackendContext()
            {
                VkInstance = _vulkan.Device.Handle,
                VkPhysicalDevice = _vulkan.PhysicalDevice.Handle,
                VkDevice = _vulkan.Device.Handle,
                VkQueue = _vulkan.Device.Queue.Handle,
                GraphicsQueueIndex = _vulkan.PhysicalDevice.QueueFamilyIndex,
                GetProcedureAddress = getProc
            };
            GrContext = GRContext.CreateVulkan(_grVkBackend);
            if (_maxResourceBytes.HasValue)
            {
                GrContext.SetResourceCacheLimit(_maxResourceBytes.Value);
            }
        }

        public ISkiaGpuRenderTarget TryCreateRenderTarget(IEnumerable<object> surfaces)
        {
            foreach (var surface in surfaces)
            {
                VulkanWindowSurface window;

                if (surface is IPlatformNativeSurfaceHandle handle)
                {
                    window = new VulkanWindowSurface(handle);
                }
                else
                {
                    continue;
                }

                VulkanRenderTarget vulkanRenderTarget = new VulkanRenderTarget(_vulkan, window);

                Initialize();

                vulkanRenderTarget.GrContext = GrContext;

                return vulkanRenderTarget;
            }

            return null;
        }

        public ISkiaSurface TryCreateSurface(PixelSize size, ISkiaGpuRenderSession session)
        {
            return null;
        }
    }
}
