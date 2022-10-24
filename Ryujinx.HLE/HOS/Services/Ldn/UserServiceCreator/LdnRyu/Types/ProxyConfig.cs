﻿using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ldn.UserServiceCreator.LdnRyu.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x8)]
    struct ProxyConfig
    {
        public uint ProxyIp;
        public uint ProxySubnetMask;
    }
}