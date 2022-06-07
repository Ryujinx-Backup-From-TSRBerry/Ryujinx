using System;
using Avalonia;
using Silk.NET.Vulkan;

namespace Ryujinx.Rsc.Vulkan.Surfaces
{
    public interface IVulkanPlatformSurface : IDisposable
    {
        float Scaling { get; }
        PixelSize SurfaceSize { get; }
        bool IsCorrupted { get; }

        SurfaceKHR CreateSurface(VulkanInstance instance);
    }
}
