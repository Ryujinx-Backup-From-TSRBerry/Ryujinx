using Android.App;
using Android.Content.PM;
using Avalonia.Android;
using Avalonia;
using Ryujinx.Common.Configuration;
using Ryujinx.Rsc.Controls;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using System.Collections.Generic;
using System;
using Ryujinx.Ui.Common.Configuration;
using Android.Views;

using WindowManagerFlags = Android.Views.WindowManagerFlags;
using Avalonia.Rendering;
using Avalonia.Threading;
using Android.Runtime;
using Android.Content;
using Android.OS;
using Ryujinx.Rsc.Mobile.Helper;
using Ryujinx.Ava.Common.Ui.Backend;
using Ryujinx.Ava.Common;

namespace Ryujinx.Rsc.Mobile
{
    [Activity(Label = "Ryujinx.Rsc.Mobile", Theme = "@style/MyTheme.NoActionBar", Icon = "@drawable/ryujinx", WindowSoftInputMode=SoftInput.AdjustResize, LaunchMode = LaunchMode.SingleInstance, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class MainActivity : AvaloniaActivity<App>
    {
        public event EventHandler<FileSystemResultEventArgs> FileSystemResult;
        private AndroidFileSystemHelper _fileSystemHelper;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            _fileSystemHelper = new AndroidFileSystemHelper(this);

            App.FileSystemHelperFactory = () => _fileSystemHelper;
            
            base.OnCreate(savedInstanceState);
        }

        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        {
            App.OrientationRequested += App_GameStateChanged;

            builder.UseSkia()
                .With(new Ava.Common.Ui.Vulkan.VulkanOptions()
                {
                    ApplicationName = "Ryujinx.Graphics.Vulkan",
                    VulkanVersion = new Version(1, 2),
                    MaxQueueCount = 2,
                    PreferDiscreteGpu = true,
                    UseDebug = ConfigurationState.Instance.Logger.GraphicsDebugLevel.Value > GraphicsDebugLevel.None,
                })
                .With(new AndroidPlatformOptions()
                {
                    UseGpu = false,
                })
                .With(new SkiaOptions()
                {
                    CustomGpuFactory = SkiaGpuFactory.CreateVulkanGpu
                })
                .AfterSetup(_ =>
                {
                    AvaloniaLocator.CurrentMutable
                        .Bind<IRenderTimer>().ToConstant(AppConfig.RenderTimer)
                        .Bind<IRenderLoop>().ToConstant(new RenderLoop(AppConfig.RenderTimer, Dispatcher.UIThread));
                });
                
            return base.CustomizeAppBuilder(builder);
        }

        private void App_GameStateChanged(object? sender, OrientationRequestedArgs e)
        {
            switch (e.Orientation)
            {
                case Orientation.Normal:
                    RunOnUiThread(() =>
                    {
                        var flags = Window.Attributes.Flags;
                        flags &= ~WindowManagerFlags.Fullscreen;
                        Window?.SetFlags(flags, WindowManagerFlags.Fullscreen);
                        RequestedOrientation = ScreenOrientation.FullUser;
                    });
                    break;
                case Orientation.Portrait:
                    RunOnUiThread(() =>
                    {
                        Window?.AddFlags(WindowManagerFlags.Fullscreen);
                        RequestedOrientation = ScreenOrientation.UserPortrait;
                    });
                    break;
                case Orientation.Landscape:
                    RunOnUiThread(() =>
                    {
                        Window?.AddFlags(WindowManagerFlags.Fullscreen);
                        RequestedOrientation = ScreenOrientation.UserLandscape;
                    });
                    break;
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == AndroidFileSystemHelper.FileRequestCode)
            {
                FileSystemResult?.Invoke(this, new FileSystemResultEventArgs(data, resultCode));
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            AppConfig.RenderTimer.Dispose();
            _fileSystemHelper.Dispose();
        }
    }
}
