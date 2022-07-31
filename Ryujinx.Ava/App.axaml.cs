using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using FluentAvalonia.Styling;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Common.Ui.Controls;
using Ryujinx.Ava.Ui.Windows;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Ui.Common.Configuration;
using System;
using System.Diagnostics;
using System.IO;

namespace Ryujinx.Ava
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();

            if (AppConfig.PreviewerDetached)
            {
                AppConfig.ApplyConfiguredTheme(this);

                ConfigurationState.Instance.Ui.BaseStyle.Event += ThemeChanged_Event;
                ConfigurationState.Instance.Ui.CustomThemePath.Event += ThemeChanged_Event;
                ConfigurationState.Instance.Ui.EnableCustomTheme.Event += CustomThemeChanged_Event;
            }
        }

        private void CustomThemeChanged_Event(object sender, ReactiveEventArgs<bool> e)
        {
            AppConfig.ApplyConfiguredTheme(this);
        }

        private void ShowRestartDialog()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var result = await ContentDialogHelper.CreateConfirmationDialog(
                        LocaleManager.Instance["DialogThemeRestartMessage"],
                        LocaleManager.Instance["DialogThemeRestartSubMessage"],
                        LocaleManager.Instance["InputDialogYes"],
                        LocaleManager.Instance["InputDialogNo"],
                        LocaleManager.Instance["DialogRestartRequiredMessage"]);

                    if (result == UserResult.Yes)
                    {
                        var path = Process.GetCurrentProcess().MainModule.FileName;
                        var info = new ProcessStartInfo() { FileName = path, UseShellExecute = false };
                        var proc = Process.Start(info);
                        desktop.Shutdown();
                        Environment.Exit(0);
                    }
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void ThemeChanged_Event(object sender, ReactiveEventArgs<string> e)
        {
            AppConfig.ApplyConfiguredTheme(this);
        }
    }
}