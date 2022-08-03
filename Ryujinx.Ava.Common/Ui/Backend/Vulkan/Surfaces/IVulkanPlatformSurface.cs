using System;
using Avalonia;
using Silk.NET.Vulkan;

namespace Ryujinx.Ava.Common.Ui.Vulkan.Surfaces
{
    public interface IVulkanPlatformSurface : IDisposable
    {
        float Scaling { get; }
        PixelSize SurfaceSize { get; }
        bool IsCorrupted { get; }
        SurfaceKHR CreateSurface(VulkanInstance instance);
    }
}
