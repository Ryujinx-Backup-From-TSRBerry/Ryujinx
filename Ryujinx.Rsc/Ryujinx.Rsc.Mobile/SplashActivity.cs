using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using ARMeilleure.Translation.PTC;
using Ryujinx.Common.Logging;
using System.IO;
using Application = Android.App.Application;
using AndroidEnv = Android.OS.Environment;
using static Android.Content.Context;
using Ryujinx.Audio.Integration;
using Ryujinx.Ui.Common.Configuration;
using Ryujinx.Audio.Backends.Dummy;
using Ryujinx.Audio.Backends.Android.AAudio;
using Ryujinx.Audio.Backends.Android.Track;
using Ryujinx.Ava.Common;

namespace Ryujinx.Rsc.Mobile
{
    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnResume()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) == (int)Permission.Granted)
            {
                Load();

                base.OnResume();

                StartActivity(new Intent(Application.Context, typeof(MainActivity)));
            }
            else
            {
                RequestPermission();

                base.OnResume();
            }
        }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        private void RequestPermission()
        {
            ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.WriteExternalStorage }, 1);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if ((grantResults.Length == 1) && (grantResults[0] == Permission.Granted))
            {
                Load();

                StartActivity(new Intent(Application.Context, typeof(MainActivity)));
            }
            else
            {
                Finish();
            }
        }

        private void Load()
        {
            if (!AppConfig.PreviewerDetached)
            {
                AppConfig.PreviewerDetached = true;

                var appPath = GetExternalFilesDir(null).AbsolutePath;

                Directory.CreateDirectory(appPath);
                App.BaseDirectory = appPath;
                App.CreateAudioHardwareDeviceDriver = CreateAudioHardwareDeviceDriver;

                
                Ryujinx.Common.Logging.Logger.AddTarget(new AsyncLogTargetWrapper(
                    new Logger(),
                    1000,
                    AsyncLogTargetOverflowAction.Block
                ));

                Ryujinx.Common.Logging.Logger.AddTarget(new AsyncLogTargetWrapper(
                    new FileLogTarget(App.BaseDirectory, "file"),
                    1000,
                    AsyncLogTargetOverflowAction.Block
                ));

                App.LoadConfiguration();

                System.AppDomain.CurrentDomain.UnhandledException += (object sender, System.UnhandledExceptionEventArgs e) => ProcessUnhandledException(e.ExceptionObject as System.Exception);

                AndroidEnvironment.UnhandledExceptionRaiser += (s, e) => ProcessUnhandledException(e.Exception as System.Exception);
            }
        }

        private IHardwareDeviceDriver CreateAudioHardwareDeviceDriver(AudioBackend arg)
        {
            if (arg != AudioBackend.Dummy)
            {
                // FIXME: This is the legacy driver that is supported everywhere but terrible.
                // BODY: Because AAudio is broken (Waiting on mono CFI) we cannot use it.
                if (AudioTrackHardwareDeviceDriver.IsSupported)
                {
                    return new AudioTrackHardwareDeviceDriver();
                }

                /*if (AAudioHardwareDeviceDriver.IsSupported)
                {
                    return new AAudioHardwareDeviceDriver();
                }*/
            }

            return new DummyHardwareDeviceDriver();
        }

        private static void ProcessUnhandledException(System.Exception exception)
        {
            string message = $"Unhandled exception caught: {exception}";

            Ryujinx.Common.Logging.Logger.Error?.PrintMsg(LogClass.Application, message);

            if (Ryujinx.Common.Logging.Logger.Error == null)
            {
                Ryujinx.Common.Logging.Logger.Notice.PrintMsg(LogClass.Application, message);
            }
            
            Ptc.Close();
            PtcProfiler.Stop();
        }
    }
}
