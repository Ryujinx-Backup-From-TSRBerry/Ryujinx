using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Cpu.Nce
{
    static class NceThreadPal
    {
        private const int SigUsr2 = 12;
        public const int UnixSuspendSignal = SigUsr2;

        [DllImport("libc", SetLastError = true)]
        private static extern IntPtr pthread_self();

        [DllImport("libpthread", SetLastError = true)]
        private static extern int pthread_kill(IntPtr thread, int sig);

        public static IntPtr GetCurrentThreadHandle()
        {
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                return pthread_self();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static void SuspendThread(IntPtr handle)
        {
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                int result = pthread_kill(handle, UnixSuspendSignal);
                if (result != 0)
                {
                    throw new Exception($"Thread kill returned error 0x{result:X}.");
                }
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}