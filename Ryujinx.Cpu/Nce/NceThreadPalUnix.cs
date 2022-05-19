using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Cpu.Nce
{
    static class NceThreadPalUnix
    {
        [DllImport("libc", SetLastError = true)]
        private static extern IntPtr pthread_self();

        [DllImport("libpthread", SetLastError = true)]
        private static extern int pthread_kill(IntPtr thread, int sig);

        public static IntPtr GetCurrentThreadHandle()
        {
            return pthread_self();
        }

        public static void SuspendThread(IntPtr handle)
        {
            int result = pthread_kill(handle, NceThreadPal.UnixSuspendSignal);
            if (result != 0)
            {
                throw new Exception($"Thread kill returned error 0x{result:X}.");
            }
        }
    }
}