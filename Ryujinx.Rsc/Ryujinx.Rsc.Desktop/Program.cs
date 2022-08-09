using System;
using Avalonia;
using Ryujinx.Common.Configuration;
using Ryujinx.Ui.Common.Configuration;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using System.Collections.Generic;
using System.IO;
using Avalonia.Rendering;
using Avalonia.Threading;
using Ryujinx.Ui.Common.Helper;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Ui.Backend;
using Silk.NET.Vulkan;

namespace Ryujinx.Rsc.Desktop
{
    class Program
    {

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            AppConfig.PreviewerDetached = true;
            App.BaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ryujinx");
            App.FileSystemHelperFactory = () => new ExtendedFileSystemHelper();
            App.LoadConfiguration();
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
            AppConfig.RenderTimer.Dispose();
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new X11PlatformOptions
                {
                    EnableMultiTouch = true,
                    EnableIme = true,
                    UseDBusFilePicker = false,
                    UseCompositor = false
                })
                .With(new Win32PlatformOptions()
                {
                    UseCompositor = false
                })
                .UseSkia()
                .With(new Ava.Common.Ui.Vulkan.VulkanOptions()
                {
                    ApplicationName = "Ryujinx.Graphics.Vulkan",
                    VulkanVersion = new Version(1, 2),
                    MaxQueueCount = 2,
                    PreferDiscreteGpu = true,
                    UseDebug = !AppConfig.PreviewerDetached ? false : ConfigurationState.Instance.Logger.GraphicsDebugLevel.Value > GraphicsDebugLevel.None,
                })
                .With(new SkiaOptions()
                {
                    CustomGpuFactory = () => { return SkiaGpuFactory.CreateVulkanGpu(Vk.GetApi); }
                })
                .AfterSetup(_ =>
                {
                    AvaloniaLocator.CurrentMutable
                        .Bind<IRenderTimer>().ToConstant(AppConfig.RenderTimer)
                        .Bind<IRenderLoop>().ToConstant(new RenderLoop(AppConfig.RenderTimer, Dispatcher.UIThread));
                })
                .LogToTrace();
    }
}
