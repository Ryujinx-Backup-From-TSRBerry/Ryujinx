using Avalonia;
using Avalonia.Skia;
using Ryujinx.Ava.Common.Ui.Vulkan;
using Ryujinx.Ava.Common.Ui.Backend.Vulkan;
using System;

namespace Ryujinx.Ava.Common.Ui.Backend
{
    public static class SkiaGpuFactory
    {
        public static ISkiaGpu CreateVulkanGpu(Func<Silk.NET.Vulkan.Vk> getApiFunction)
        {
            var skiaOptions = AvaloniaLocator.Current.GetService<SkiaOptions>() ?? new SkiaOptions();
            var platformInterface = AvaloniaLocator.Current.GetService<VulkanPlatformInterface>();

            VulkanSkiaGpu.GetApiFunction = getApiFunction;

            if (platformInterface == null)
            {
                VulkanPlatformInterface.TryInitialize();
            }

            var gpu = new VulkanSkiaGpu(skiaOptions.MaxGpuResourceSizeBytes);
            AvaloniaLocator.CurrentMutable.Bind<VulkanSkiaGpu>().ToConstant(gpu);

            return gpu;
        }
    }
}