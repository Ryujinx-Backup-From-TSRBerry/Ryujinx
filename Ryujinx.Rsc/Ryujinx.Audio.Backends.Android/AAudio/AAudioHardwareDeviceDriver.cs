using Ryujinx.Audio.Common;
using Ryujinx.Audio.Integration;
using Ryujinx.Common.Logging;
using Ryujinx.Memory;
using System;
using System.Collections.Concurrent;
using System.Threading;
using static Ryujinx.Audio.Backends.Android.AAudio.AAudioAPI;
using static Ryujinx.Audio.Integration.IHardwareDeviceDriver;

namespace Ryujinx.Audio.Backends.Android.AAudio
{
    public class AAudioHardwareDeviceDriver : IHardwareDeviceDriver
    {
        private readonly ManualResetEvent _updateRequiredEvent;
        private readonly ManualResetEvent _pauseEvent;
        private readonly ConcurrentDictionary<AAudioHardwareDeviceSession, byte> _sessions;

        public AAudioHardwareDeviceDriver()
        {
            _updateRequiredEvent = new ManualResetEvent(false);
            _pauseEvent = new ManualResetEvent(true);
            _sessions = new ConcurrentDictionary<AAudioHardwareDeviceSession, byte>();
        }

        public static bool IsSupported => IsSupportedInternal();

        private static bool IsSupportedInternal()
        {
            AAudioResult result = OpenStream(out IntPtr device, SampleFormat.PcmInt16, Constants.TargetSampleRate, Constants.ChannelCountMax, IntPtr.Zero);

            if (result == AAudioResult.Ok && device != IntPtr.Zero)
            {
                AAudioStream_close(device);

                return true;
            }

            Logger.Warning?.Print(LogClass.Audio, $"AAudio returned {result}! AAudio will be marked as unsupported.");

            return false;
        }

        public ManualResetEvent GetUpdateRequiredEvent()
        {
            return _updateRequiredEvent;
        }

        public ManualResetEvent GetPauseEvent()
        {
            return _pauseEvent;
        }

        public IHardwareDeviceSession OpenDeviceSession(Direction direction, IVirtualMemoryManager memoryManager, SampleFormat sampleFormat, uint sampleRate, uint channelCount, float volume)
        {
            if (channelCount == 0)
            {
                channelCount = 2;
            }

            if (sampleRate == 0)
            {
                sampleRate = Constants.TargetSampleRate;
            }

            if (direction != Direction.Output)
            {
                throw new NotImplementedException("Input direction is currently not implemented on SDL2 backend!");
            }

            AAudioHardwareDeviceSession session = new AAudioHardwareDeviceSession(this, memoryManager, sampleFormat, sampleRate, channelCount, volume);

            _sessions.TryAdd(session, 0);

            return session;
        }

        internal bool Unregister(AAudioHardwareDeviceSession session)
        {
            return _sessions.TryRemove(session, out _);
        }

        private static AAudioFormat GetAAudioFormat(SampleFormat format)
        {
            return format switch
            {
                SampleFormat.PcmInt16 => AAudioFormat.PcmInt16,
                // TODO: Add Android API level checks for those two entires (31)
                // SampleFormat.PcmInt24 => AAudioFormat.PcmInt24,
                // SampleFormat.PcmInt32 => AAudioFormat.PcmInt32,
                SampleFormat.PcmFloat => AAudioFormat.PcmFloat,
                _ => throw new ArgumentException($"Unsupported sample format {format}"),
            };
        }

        internal static AAudioResult OpenStream(out IntPtr streamHandle, SampleFormat requestedSampleFormat, uint requestedSampleRate, uint requestedChannelCount, IntPtr callbackDelegatePtr)
        {
            streamHandle = IntPtr.Zero;

            IntPtr builder = IntPtr.Zero;

            AAudioResult result = AAudio_createStreamBuilder(ref builder);

            if (result != AAudioResult.Ok)
            {
                return result;
            }

            AAudioStreamBuilder_setFormat(builder, GetAAudioFormat(requestedSampleFormat));
            AAudioStreamBuilder_setChannelCount(builder, (int)requestedChannelCount);
            //AAudioStreamBuilder_setFramesPerDataCallback(builder, (int)sampleCount);
            AAudioStreamBuilder_setPerformanceMode(builder, AAudioPerformanceMode.ModeLowLatency);

            if (callbackDelegatePtr != IntPtr.Zero)
            {
                AAudioStreamBuilder_setDataCallback(builder, callbackDelegatePtr, IntPtr.Zero);
            }

            result = AAudioStreamBuilder_openStream(builder, out streamHandle);

            AAudioResult resultDestryBuilder = AAudioStreamBuilder_delete(builder);

            // FIXME: If we have an error here, should we do more?
            if (resultDestryBuilder != AAudioResult.Ok)
            {
                Logger.Error?.Print(LogClass.Audio, $"AAudioStreamBuilder_delete failed with error {resultDestryBuilder}");
            }

            if (result == AAudioResult.Ok)
            {
                int realBufferSize = AAudioStream_setBufferSizeInFrames(streamHandle, AAudioStream_getFramesPerBurst(streamHandle) * 2);

                if (realBufferSize < 0)
                {
                    result = (AAudioResult)realBufferSize;

                    Logger.Error?.Print(LogClass.Audio, $"AAudioStream_setBufferSizeInFrames failed with error {result}");

                    AAudioStream_close(streamHandle);

                    streamHandle = IntPtr.Zero;
                }
            }

            return result;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (AAudioHardwareDeviceSession session in _sessions.Keys)
                {
                    session.Dispose();
                }

                _pauseEvent.Dispose();
            }
        }

        public bool SupportsSampleRate(uint sampleRate)
        {
            return true;
        }

        public bool SupportsSampleFormat(SampleFormat sampleFormat)
        {
            return sampleFormat == SampleFormat.PcmInt16 || sampleFormat == SampleFormat.PcmFloat;
        }

        public bool SupportsChannelCount(uint channelCount)
        {
            return true;
        }

        public bool SupportsDirection(Direction direction)
        {
            // TODO: add direction input when supported.
            return direction == Direction.Output;
        }
    }
}