using System;
using Silk.NET.Vulkan;

namespace Ryujinx.Ava.Common.Ui.Vulkan
{
    public class VulkanQueue
    {
        public VulkanQueue(VulkanDevice device, Queue apiHandle)
        {
            Device = device;
            InternalHandle = apiHandle;
        }

        public VulkanDevice Device { get; }
        public IntPtr Handle => InternalHandle.Handle;
        public Queue InternalHandle { get; }
    }
}
