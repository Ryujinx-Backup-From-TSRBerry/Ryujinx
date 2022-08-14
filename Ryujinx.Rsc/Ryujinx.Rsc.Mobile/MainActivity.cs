using Android.App;
using Android.Content.PM;
using Avalonia.Android;
using Avalonia;
using Ryujinx.Common.Configuration;
using Ryujinx.Rsc.Controls;
using System;
using Ryujinx.Ui.Common.Configuration;
using Android.Views;
using WindowManagerFlags = Android.Views.WindowManagerFlags;
using Android.Runtime;
using Android.Content;
using Android.OS;
using Ryujinx.Rsc.Mobile.Helper;
using Ryujinx.Ava.Common.Ui.Backend;
using Ryujinx.Ava.Common;
using Silk.NET.Vulkan;
using System.IO;
using Ryujinx.Rsc.Mobile.Input;

namespace Ryujinx.Rsc.Mobile
{
    [Activity(Label = "Ryujinx.Rsc.Mobile", Theme = "@style/MyTheme.NoActionBar", Icon = "@drawable/ryujinx", WindowSoftInputMode=SoftInput.AdjustResize, LaunchMode = LaunchMode.SingleInstance, ConfigurationChanges =
        ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class MainActivity : AvaloniaActivity<App>
    {
        public event EventHandler<FileSystemResultEventArgs> FileSystemResult;
        private AndroidFileSystemHelper _fileSystemHelper;
        private VulkanLoader _loader;

        internal event EventHandler<KeyEvent> KeyDispatched;
        internal event EventHandler<MotionEvent> MotionDispatched;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            _fileSystemHelper = new AndroidFileSystemHelper(this);

            App.FileSystemHelperFactory = () => _fileSystemHelper;

            var libFolder = ApplicationInfo.NativeLibraryDir;
            var publicFolder = ApplicationContext.GetExternalFilesDir(null) + Path.DirectorySeparatorChar.ToString();
            var privateFolder = ApplicationContext.FilesDir + Path.DirectorySeparatorChar.ToString();
            var driver = "libvulkan.so";

            string preferredDriverSetting = Path.Combine(publicFolder, "Drivers", "selected");
            if (File.Exists(preferredDriverSetting))
            {
                driver = File.ReadAllText(preferredDriverSetting);
            }

            _loader = new VulkanLoader(driver, publicFolder, privateFolder, libFolder);

            App.GetNativeGamepadDriver = () => { return new AndroidControllerDriver(this); };

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
                    UseCompositor = true,
                })
                .With(new SkiaOptions()
                {
                    CustomGpuFactory = () => { return SkiaGpuFactory.CreateVulkanGpu(_loader.GetApi); }
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

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Android.App.Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == AndroidFileSystemHelper.FileRequestCode)
            {
                FileSystemResult?.Invoke(this, new FileSystemResultEventArgs(data, resultCode));
            }
        }

        protected override void OnDestroy()
        {
            _loader?.Dispose();
            base.OnDestroy();
            AppConfig.RenderTimer?.Dispose();
            _fileSystemHelper.Dispose();
        }

        public override void OnBackPressed()
        {
            if (!App.HandleBackPress())
            {
                FinishAndRemoveTask();
            }
        }

        public override bool DispatchGenericMotionEvent(MotionEvent ev)
        {
            if (ev.Source.HasFlag(InputSourceType.Joystick))
            {
                MotionDispatched?.Invoke(this, ev);
            }
            return base.DispatchGenericMotionEvent(ev);
        }

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            if (e.Source.HasFlag(InputSourceType.Gamepad) || e.Source.HasFlag(InputSourceType.Dpad) || e.Source.HasFlag(InputSourceType.Joystick))
            {
                KeyDispatched?.Invoke(this, e);
            }
            return base.DispatchKeyEvent(e);
        }
    }
}
