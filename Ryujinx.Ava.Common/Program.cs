using Ryujinx.Ava.Common.Ui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            RenderTimer = new RenderTimer();
        }
    }
}
