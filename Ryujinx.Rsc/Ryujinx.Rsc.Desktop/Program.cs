using System;
using Avalonia;
using Ryujinx.Common.Configuration;
using Ryujinx.Rsc.Backend;
using Ryujinx.Rsc.Controls;
using Ryujinx.Ui.Common.Configuration;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using System.Collections.Generic;
using System.IO;
using Avalonia.Rendering;
using Avalonia.Threading;
using Ryujinx.Ui.Common.Helper;


namespace Ryujinx.Rsc.Desktop
{
    class Program
    {
        public static RenderTimer RenderTimer { get; private set; }

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {

            RenderTimer = new RenderTimer();
            App.RenderTimer = RenderTimer;
            App.PreviewerDetached = true;
            App.BaseDirectory =  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ryujinx");
            App.FileSystemHelperFactory = () => new ExtendedFileSystemHelper();
            App.LoadConfiguration();
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
            RenderTimer.Dispose();
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseSkia()
                .With(new Vulkan.VulkanOptions()
                {
                    ApplicationName = "Ryujinx.Graphics.Vulkan",
                    VulkanVersion = new Version(1, 2),
                    DeviceExtensions = new List<string>
                    {
                        ExtConditionalRendering.ExtensionName,
                        ExtExtendedDynamicState.ExtensionName,
                        KhrDrawIndirectCount.ExtensionName,
                        "VK_EXT_custom_border_color",
                        "VK_EXT_fragment_shader_interlock",
                        "VK_EXT_index_type_uint8",
                        "VK_EXT_robustness2",
                        "VK_EXT_shader_subgroup_ballot",
                        "VK_EXT_subgroup_size_control",
                        "VK_NV_geometry_shader_passthrough"
                    },
                    MaxQueueCount = 2,
                    PreferDiscreteGpu = true,
                    UseDebug = !App.PreviewerDetached ? false : ConfigurationState.Instance.Logger.GraphicsDebugLevel.Value > GraphicsDebugLevel.None,
                })
                .With(new SkiaOptions()
                {
                    CustomGpuFactory = SkiaGpuFactory.CreateVulkanGpu
                })
                .AfterSetup(_ =>
                {
                    AvaloniaLocator.CurrentMutable
                        .Bind<IRenderTimer>().ToConstant(RenderTimer)
                        .Bind<IRenderLoop>().ToConstant(new RenderLoop(RenderTimer, Dispatcher.UIThread));
                })
                .LogToTrace();
    }
}
