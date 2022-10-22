using Ryujinx.HLE.HOS.Services.Ldn.UserServiceCreator;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ldn.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x22)]
    struct Ssid
    {
        public byte Length;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = LdnConst.SsidLengthMax + 1)]
        public byte[] Name;
    }
}