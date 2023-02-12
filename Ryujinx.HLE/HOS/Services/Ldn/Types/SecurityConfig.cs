using Ryujinx.HLE.HOS.Services.Ldn.UserServiceCreator;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ldn.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x44)]
    struct SecurityConfig
    {
        public SecurityMode  SecurityMode;
        public ushort        PassphraseSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = LdnConst.PassphraseLengthMax)]
        public byte[]       Passphrase;
    }
}