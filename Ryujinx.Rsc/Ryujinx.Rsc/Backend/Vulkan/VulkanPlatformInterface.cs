using System;
using System.Collections.Concurrent;
using System.Linq;
using Avalonia;
using Ryujinx.Common.Configuration;
using Ryujinx.Ui.Common.Configuration;
using Ryujinx.Rsc.Vulkan.Surfaces;
using Silk.NET.Vulkan;
using Ryujinx.Graphics.Vulkan;

namespace Ryujinx.Rsc.Vulkan
{
    internal class VulkanPlatformInterface : IDisposable
    {
        private static VulkanOptions _options;

        private VulkanPlatformInterface(VulkanInstance instance)
        {
            Instance = instance;
            Api = instance.Api;
        }

        public VulkanPhysicalDevice PhysicalDevice { get; private set; }
        public VulkanInstance Instance { get; }
        public VulkanDevice Device { get; set; }
        public Vk Api { get; private set; }

        public void Dispose()
        {
            Device?.Dispose();
            Instance?.Dispose();
            Api?.Dispose();
        }

        private static VulkanPlatformInterface TryCreate()
        {
            try
            {
                _options = AvaloniaLocator.Current.GetService<VulkanOptions>() ?? new VulkanOptions();


            if (OperatingSystem.IsAndroid())
                Silk.NET.Core.Loader.SearchPathContainer.Platform = Silk.NET.Core.Loader.UnderlyingPlatform.Android;

                var instance = VulkanInstance.Create(_options);

                return new VulkanPlatformInterface(instance);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static bool TryInitialize()
        {
            var feature = TryCreate();
            if (feature != null)
            {
                AvaloniaLocator.CurrentMutable.Bind<VulkanPlatformInterface>().ToConstant(feature);
                return true;
            }

            return false;
        }

        public VulkanSurfaceRenderTarget CreateRenderTarget(IVulkanPlatformSurface platformSurface)
        {
            var surface = VulkanSurface.CreateSurface(Instance, platformSurface);
            try
            {
                if (Device == null)
                {
                    PhysicalDevice = VulkanPhysicalDevice.FindSuitablePhysicalDevice(Instance, surface, _options.PreferDiscreteGpu);
                    var device = VulkanInitialization.CreateDevice(Instance.Api,
                                                                   PhysicalDevice.InternalHandle,
                                                                   PhysicalDevice.QueueFamilyIndex,
                                                                   VulkanInitialization.GetSupportedExtensions(Instance.Api, PhysicalDevice.InternalHandle),
                                                                   PhysicalDevice.QueueCount);

                    Device = new VulkanDevice(device, PhysicalDevice, Instance.Api);
                }
            }
            catch (Exception _)
            {
                surface.Dispose();
            }

            return new VulkanSurfaceRenderTarget(this, surface);
        }
    }
}
