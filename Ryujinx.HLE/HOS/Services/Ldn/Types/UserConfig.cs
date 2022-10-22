using Ryujinx.HLE.HOS.Services.Ldn.UserServiceCreator;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ldn.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x30)]
    struct UserConfig
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = LdnConst.UserNameBytesMax + 1)]
        public byte[] UserName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public byte[] Unknown1;
    }
}