using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Ryujinx.Audio.Backends.Android.AAudio
{
    [SupportedOSPlatform("android26.0")]
    internal class AAudioAPI
    {
        private const string LibraryName = "aaudio";

        public enum AAudioResult : int
        {
            Ok,

            ErrorBase = -900,
            ErrorDisconnected,
            ErrorIllegalArgument,
            ErrorInternal = ErrorIllegalArgument + 2,
            ErrorInvalidState,
            ErrorInvalidHandle = ErrorInvalidState + 3,
            ErrorUnimplemented = ErrorInvalidHandle + 2,
            ErrorUnavailable,
            ErrorNoFreeHandles,
            ErrorNoMemory,
            ErrorNull,
            ErrorTimeout,
            ErrorWouldBlock,
            ErrorInvalidFormat,
            ErrorOutOfRange,
            ErrorNOService,
            ErrorInvalidRate
        }

        public enum AAudioFormat : int
        {
            Invalid = -1,
            Unspecified,
            PcmInt16,
            PcmFloat,
            // API level 31 only
            PcmInt24,
            // API level 31 only
            PcmInt32,
        }

        public enum AAudioStreamState : int
        {
            Uninitialized = 0,
            Unknown,
            Open,
            Starting,
            Started,
            Pausing,
            Paused,
            Flushing,
            Flushed,
            Stopping,
            Stopped,
            Closing,
            Disconnected,
        }

        public enum AAudioPerformanceMode : int
        {
            ModeNone = 10,
            ModePowerSaving,
            ModeLowLatency,
        }

        public enum AAudioDataCallbackResult : int
        {
            Continue = 0,
            Stop
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate AAudioDataCallbackResult AAudioStream_DataCallback(IntPtr stream, IntPtr userData, IntPtr audioData, int numFrames);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AAudioStream_ErrorCallback(IntPtr stream, IntPtr userPtr, AAudioResult error);


        [DllImport(LibraryName)]
        public static extern AAudioResult AAudio_createStreamBuilder(ref IntPtr builder);

        [DllImport(LibraryName)]
        public static extern AAudioResult AAudioStreamBuilder_delete(IntPtr builder);

        [DllImport(LibraryName)]
        public static extern void AAudioStreamBuilder_setFormat(IntPtr builder, AAudioFormat format);

        [DllImport(LibraryName)]
        public static extern void AAudioStreamBuilder_setChannelCount(IntPtr builder, int channelCount);

        [DllImport(LibraryName)]
        public static extern void AAudioStreamBuilder_setPerformanceMode(IntPtr builder, AAudioPerformanceMode mode);

        [DllImport(LibraryName)]
        public static extern void AAudioStreamBuilder_setFramesPerDataCallback(IntPtr builder, int numFrames);

        [DllImport(LibraryName)]
        public static extern void AAudioStreamBuilder_setDataCallback(IntPtr builder, IntPtr dataCallback, IntPtr userData);

        [DllImport(LibraryName)]
        public static extern void AAudioStreamBuilder_setErrorCallback(IntPtr builder, AAudioStream_ErrorCallback callback, IntPtr userData);

        [DllImport(LibraryName)]
        public static extern AAudioResult AAudioStreamBuilder_openStream(IntPtr builder, out IntPtr stream);

        [DllImport(LibraryName)]
        public static extern int AAudioStream_setBufferSizeInFrames(IntPtr stream, int numFrames);

        [DllImport(LibraryName)]
        public static extern int AAudioStream_getFramesPerBurst(IntPtr stream);

        [DllImport(LibraryName)]
        public static extern int AAudioStream_getBufferCapacityInFrames(IntPtr stream);

        [DllImport(LibraryName)]
        public static extern AAudioResult AAudioStream_close(IntPtr stream);

        [DllImport(LibraryName)]
        public static extern AAudioResult AAudioStream_requestStart(IntPtr stream);

        [DllImport(LibraryName)]
        public static extern AAudioResult AAudioStream_requestPause(IntPtr stream);

        [DllImport(LibraryName)]
        public static extern AAudioResult AAudioStream_requestStop(IntPtr stream);

        [DllImport(LibraryName)]
        public static extern AAudioResult AAudioStream_waitForStateChange(IntPtr stream, AAudioStreamState inputState, out AAudioStreamState nextState, long timeoutNanoseconds);
    }
}
