using Ryujinx.Ava.Common.Ui.Controls;
using Ryujinx.Ui.Common.Configuration;
using System;
using System.Linq;
using Avalonia;
using FluentAvalonia;
using FluentAvalonia.Styling;
using Avalonia.Styling;
using Avalonia.Markup.Xaml;
using System.IO;
using Ryujinx.Common.Logging;

namespace Ryujinx.Ava.Common
{
    public static class AppConfig
    {
        public static bool PreviewerDetached { get; set; }
        public static string ConfigurationPath { get; set; }
        public static RenderTimer RenderTimer { get; private set; }
        public static double ActualScaleFactor { get; set; }

        static AppConfig()
        {
            if (!OperatingSystem.IsAndroid())
            {
                RenderTimer = new RenderTimer();
            }
        }

        public static void ApplyConfiguredTheme(Application app)
        {
            try
            {
                string baseStyle = ConfigurationState.Instance.Ui.BaseStyle;
                string themePath = ConfigurationState.Instance.Ui.CustomThemePath;
                bool enableCustomTheme = ConfigurationState.Instance.Ui.EnableCustomTheme;

                const string BaseStyleUrl = "avares://Ryujinx.Ava.Common/Assets/Styles/Base{0}.xaml";

                if (string.IsNullOrWhiteSpace(baseStyle))
                {
                    ConfigurationState.Instance.Ui.BaseStyle.Value = "Dark";

                    baseStyle = ConfigurationState.Instance.Ui.BaseStyle;
                }

                var theme = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();

                theme.RequestedTheme = baseStyle;

                var currentStyles = app.Styles;

                // Remove all styles except the base style.
                if (currentStyles.Count > 1)
                {
                    currentStyles.RemoveRange(1, currentStyles.Count - 1);
                }

                IStyle newStyles = null;

                // Load requested style, and fallback to Dark theme if loading failed.
                try
                {
                    newStyles = (Styles)AvaloniaXamlLoader.Load(new Uri(string.Format(BaseStyleUrl, baseStyle), UriKind.Absolute));
                }
                catch (XamlLoadException)
                {
                    newStyles = (Styles)AvaloniaXamlLoader.Load(new Uri(string.Format(BaseStyleUrl, "Dark"), UriKind.Absolute));
                }

                currentStyles.Add(newStyles);

                if (enableCustomTheme)
                {
                    if (!string.IsNullOrWhiteSpace(themePath))
                    {
                        try
                        {
                            var themeContent = File.ReadAllText(themePath);
                            var customStyle = AvaloniaRuntimeXamlLoader.Parse<IStyle>(themeContent);

                            currentStyles.Add(customStyle);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error?.Print(LogClass.Application, $"Failed to Apply Custom Theme. Error: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception)
            {
                Logger.Warning?.Print(LogClass.Application, "Failed to Apply Theme. A restart is needed to apply the selected theme");
            }
        }
    }
}
