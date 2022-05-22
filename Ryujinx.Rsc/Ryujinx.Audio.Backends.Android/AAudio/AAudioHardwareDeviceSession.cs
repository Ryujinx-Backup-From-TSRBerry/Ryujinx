using Ryujinx.Audio.Backends.Common;
using Ryujinx.Audio.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Memory;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using static Ryujinx.Audio.Backends.Android.AAudio.AAudioAPI;

namespace Ryujinx.Audio.Backends.Android.AAudio
{
    class AAudioHardwareDeviceSession : HardwareDeviceSessionOutputBase
    {
        private AAudioHardwareDeviceDriver _driver;
        private ConcurrentQueue<AAudioAudioBuffer> _queuedBuffers;
        private DynamicRingBuffer _ringBuffer;
        private ulong _playedSampleCount;
        private ManualResetEvent _updateRequiredEvent;
        private IntPtr _outputStream;
        private AAudioStream_DataCallback _callbackDelegate;
        private IntPtr _callbackDelegatePtr;
        private int _bytesPerFrame;
        private bool _started;
        private float _volume;

        public AAudioHardwareDeviceSession(AAudioHardwareDeviceDriver driver, IVirtualMemoryManager memoryManager, SampleFormat requestedSampleFormat, uint requestedSampleRate, uint requestedChannelCount, float requestedVolume) : base(memoryManager, requestedSampleFormat, requestedSampleRate, requestedChannelCount)
        {
            _driver = driver;
            _updateRequiredEvent = _driver.GetUpdateRequiredEvent();
            _queuedBuffers = new ConcurrentQueue<AAudioAudioBuffer>();
            _ringBuffer = new DynamicRingBuffer();
            _callbackDelegate = Update;
            _callbackDelegatePtr = Marshal.GetFunctionPointerForDelegate(_callbackDelegate);
            _bytesPerFrame = BackendHelper.GetSampleSize(RequestedSampleFormat) * (int)RequestedChannelCount;
            _started = false;
            _volume = requestedVolume;
        }

        private void EnsureAudioStreamSetup(AudioBuffer buffer)
        {
            uint bufferSampleCount = (uint)GetSampleCount(buffer);
            bool needAudioSetup = _outputStream == IntPtr.Zero;

            if (needAudioSetup)
            {
                AAudioResult result = AAudioHardwareDeviceDriver.OpenStream(out IntPtr newOutputStream, RequestedSampleFormat, RequestedSampleRate, RequestedChannelCount, _callbackDelegatePtr);

                if (result != AAudioResult.Ok || newOutputStream == IntPtr.Zero)
                {
                    // No stream in place, this is unexpected.
                    throw new InvalidOperationException($"OpenStream failed with error {result}");
                }
                else
                {
                    if (_outputStream != IntPtr.Zero)
                    {
                        CheckedResult(AAudioStream_close(_outputStream));
                    }

                    _outputStream = newOutputStream;

                    if (_started)
                    {
                        // Crash here
                        CheckedResult(AAudioStream_requestStart(_outputStream));
                    }
                }
            }
        }

        private unsafe AAudioDataCallbackResult Update(IntPtr streamHandle, IntPtr userData, IntPtr stream, int numFrames)
        {
            Logger.Info?.Print(LogClass.Audio, $"HERE COMES NOTHING");

            Span<byte> streamSpan = new Span<byte>((void*)stream, numFrames * _bytesPerFrame);

            int maxFrameCount = numFrames / (int)RequestedChannelCount;
            int bufferedFrames = _ringBuffer.Length / _bytesPerFrame;

            int frameCount = Math.Min(bufferedFrames, maxFrameCount);

            if (frameCount == 0)
            {
                // AAudio left the responsibility to the user to clear the buffer.
                streamSpan.Fill(0);

                return AAudioDataCallbackResult.Continue;
            }

            byte[] samples = new byte[frameCount * _bytesPerFrame];

            _ringBuffer.Read(samples, 0, samples.Length);

            fixed (byte* p = samples)
            {
                IntPtr pStreamSrc = (IntPtr)p;

                // Zero the dest buffer
                streamSpan.Fill(0);

                // Apply volume to written data
                // TODO
                // SDL_MixAudioFormat(stream, pStreamSrc, _nativeSampleFormat, (uint)samples.Length, (int)(_volume * SDL_MIX_MAXVOLUME));
            }

            ulong sampleCount = GetSampleCount(samples.Length);

            ulong availaibleSampleCount = sampleCount;

            bool needUpdate = false;

            while (availaibleSampleCount > 0 && _queuedBuffers.TryPeek(out AAudioAudioBuffer driverBuffer))
            {
                ulong sampleStillNeeded = driverBuffer.SampleCount - Interlocked.Read(ref driverBuffer.SamplePlayed);
                ulong playedAudioBufferSampleCount = Math.Min(sampleStillNeeded, availaibleSampleCount);

                ulong currentSamplePlayed = Interlocked.Add(ref driverBuffer.SamplePlayed, playedAudioBufferSampleCount);
                availaibleSampleCount -= playedAudioBufferSampleCount;

                if (currentSamplePlayed == driverBuffer.SampleCount)
                {
                    _queuedBuffers.TryDequeue(out _);

                    needUpdate = true;
                }

                Interlocked.Add(ref _playedSampleCount, playedAudioBufferSampleCount);
            }

            // Notify the output if needed.
            if (needUpdate)
            {
                _updateRequiredEvent.Set();
            }

            return AAudioDataCallbackResult.Continue;
        }

        public override ulong GetPlayedSampleCount()
        {
            return Interlocked.Read(ref _playedSampleCount);
        }

        public override float GetVolume()
        {
            return _volume;
        }

        public override void PrepareToClose() { }

        public override void QueueBuffer(AudioBuffer buffer)
        {
            EnsureAudioStreamSetup(buffer);

            AAudioAudioBuffer driverBuffer = new AAudioAudioBuffer(buffer.DataPointer, GetSampleCount(buffer));

            _ringBuffer.Write(buffer.Data, 0, buffer.Data.Length);

            _queuedBuffers.Enqueue(driverBuffer);
        }

        public override void SetVolume(float volume)
        {
            _volume = volume;
        }

        private static void CheckedResult(AAudioResult result)
        {
            if (result != AAudioResult.Ok)
            {
                throw new Exception(result.ToString());
            }
        }

        public override void Start()
        {
            if (!_started)
            {
                if (_outputStream != IntPtr.Zero)
                {
                    // TODO: Check errors?
                    CheckedResult(AAudioStream_requestStart(_outputStream));
                }

                _started = true;
            }
        }

        public override void Stop()
        {
            if (_started)
            {
                if (_outputStream != IntPtr.Zero)
                {
                    // TODO: Check errors?
                    CheckedResult(AAudioStream_requestPause(_outputStream));
                }

                _started = false;
            }
        }

        public override void UnregisterBuffer(AudioBuffer buffer) { }

        public override bool WasBufferFullyConsumed(AudioBuffer buffer)
        {
            if (!_queuedBuffers.TryPeek(out AAudioAudioBuffer driverBuffer))
            {
                return true;
            }

            return driverBuffer.DriverIdentifier != buffer.DataPointer;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _driver.Unregister(this))
            {
                PrepareToClose();

                if (_outputStream != IntPtr.Zero)
                {
                    // TODO: Check errors?
                    CheckedResult(AAudioStream_requestStop(_outputStream));

                    Thread.Sleep(10);

                    CheckedResult(AAudioStream_close(_outputStream));
                }
            }
        }

        public override void Dispose()
        {
            Dispose(true);
        }
    }
}
