using Android.App;
using Android.Content;
using Ryujinx.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ryujinx.Rsc.Mobile.Helper
{
    public class FileSystemResultEventArgs : EventArgs
    {
        public FileSystemResultEventArgs(Intent? data, Result resultCode)
        {
            Data = data;
            ResultCode = resultCode;
        }

        public Intent? Data{ get; }
        public Result ResultCode { get; }
    }
}