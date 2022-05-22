using Android.Media;
using Ryujinx.Audio.Backends.Common;
using Ryujinx.Audio.Common;
using Ryujinx.Memory;
using System;
using System.Collections.Generic;
using System.Threading;
using Encoding = Android.Media.Encoding;
using AndroidAudioManager = Android.Media.AudioManager;

namespace Ryujinx.Audio.Backends.Android.Track
{
    class AudioTrackHardwareDeviceSession : HardwareDeviceSessionOutputBase
    {
        private AudioTrackHardwareDeviceDriver _driver;
        private bool _isWorkerActive;
        private bool _isActive;
        private AudioTrack _audioTrack;
        private Queue<AudioTrackAudioBuffer> _queuedBuffers;
        private ulong _playedSampleCount;
        private Thread _workerThread;
        private ManualResetEvent _updateRequiredEvent;

        private object _queueLock = new object();
        private object _trackLock = new object();

        public AudioTrackHardwareDeviceSession(AudioTrackHardwareDeviceDriver driver, IVirtualMemoryManager memoryManager, SampleFormat requestedSampleFormat, uint requestedSampleRate, uint requestedChannelCount, float requestedVolume) : base(memoryManager, requestedSampleFormat, requestedSampleRate, requestedChannelCount)
        {
            _driver = driver;
            _queuedBuffers = new Queue<AudioTrackAudioBuffer>();
            _isActive = false;
            _playedSampleCount = 0;
            _isWorkerActive = true;
            _updateRequiredEvent = driver.GetUpdateRequiredEvent();

            AudioAttributes.Builder audioAttributeBuilder = new AudioAttributes.Builder()
                                                .SetContentType(AudioContentType.Music)
                                                .SetUsage(AudioUsageKind.Game)
                                                .SetFlags(AudioFlags.LowLatency);

            if (OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                audioAttributeBuilder.SetAllowedCapturePolicy(CapturePolicies.ByAll);
            }

            AudioAttributes audioAttribute = audioAttributeBuilder.Build();

            ChannelOut channelOut = GetChannelOut(requestedChannelCount);
            Encoding encoding = GetEncoding(requestedSampleFormat);

            AudioFormat audioFormat = new AudioFormat.Builder()
                                                .SetEncoding(encoding)
                                                .SetSampleRate((int)requestedSampleRate)
                                                .SetChannelMask(channelOut)
                                                .Build();

            int minBufferSize = AudioTrack.GetMinBufferSize((int)requestedSampleRate, channelOut, encoding);

            _audioTrack = new AudioTrack(audioAttribute, audioFormat, minBufferSize, AudioTrackMode.Stream, AndroidAudioManager.AudioSessionIdGenerate);

            _workerThread = new Thread(Update);
            _workerThread.Name = $"HardwareDeviceSession.Android.Track";
            _workerThread.Start();

            SetVolume(requestedVolume);
        }

        private ChannelOut GetChannelOut(uint channelCount)
        {
            switch (channelCount)
            {
                case 1:
                    return ChannelOut.Mono;
                case 2:
                    return ChannelOut.Stereo;
                case 4:
                    return ChannelOut.Quad;
                case 6:
                    return ChannelOut.Surround;
                default:
                    throw new ArgumentException(channelCount.ToString());
            }
        }

        private Encoding GetEncoding(SampleFormat format)
        {
            switch (format)
            {
                case SampleFormat.PcmInt8:
                    return Encoding.Pcm8bit;
                case SampleFormat.PcmInt16:
                    return Encoding.Pcm16bit;
                case SampleFormat.PcmInt32:
                    return Encoding.Pcm32bit;
                case SampleFormat.PcmFloat:
                    return Encoding.PcmFloat;
                default:
                    throw new ArgumentException(format.ToString());
            }
        }



        public override void PrepareToClose()
        {
            _isWorkerActive = false;
            _workerThread.Join();
        }

        private void StartIfNotPlaying()
        {
            lock (_trackLock)
            {
                if (_audioTrack.PlayState != PlayState.Playing)
                {
                    _audioTrack.Play();
                }
            }
        }

        public override void QueueBuffer(AudioBuffer buffer)
        {
            lock (_queueLock)
            {
                AudioTrackAudioBuffer driverBuffer = new AudioTrackAudioBuffer(buffer.DataPointer, buffer.Data, GetSampleCount(buffer));

                _queuedBuffers.Enqueue(driverBuffer);

                if (_isActive)
                {
                    StartIfNotPlaying();
                }
            }
        }

        public override void SetVolume(float volume)
        {
            lock (_trackLock)
            {
                _audioTrack.SetVolume(volume);
            }
        }

        public override float GetVolume()
        {
            // TODO
            return 1.0f;
        }

        public override void Start()
        {
            lock (_trackLock)
            {
                _isActive = true;

                StartIfNotPlaying();
            }
        }

        public override void Stop()
        {
            lock (_trackLock)
            {
                _audioTrack.Pause();

                _isActive = false;
            }
        }

        public override void UnregisterBuffer(AudioBuffer buffer) { }

        public override bool WasBufferFullyConsumed(AudioBuffer buffer)
        {
            lock (_queueLock)
            {
                if (!_queuedBuffers.TryPeek(out AudioTrackAudioBuffer driverBuffer))
                {
                    return true;
                }

                return driverBuffer.DriverIdentifier != buffer.DataPointer;
            }
        }

        public override ulong GetPlayedSampleCount()
        {
            lock (_queueLock)
            {
                return _playedSampleCount;
            }
        }

        public void Update(object ignored)
        {
            while (_isWorkerActive)
            {
                bool needUpdate = false;

                AudioTrackAudioBuffer buffer;
                bool hasBuffer;

                lock (_queueLock)
                {
                    hasBuffer = _queuedBuffers.TryPeek(out buffer);
                }

                while (hasBuffer)
                {
                    StartIfNotPlaying();

                    // As per doc, AudioTrack is thread safe on blocking write.
                    _audioTrack.Write(buffer.Data, 0, buffer.Data.Length, WriteMode.Blocking);

                    lock (_queueLock)
                    {
                        _playedSampleCount += buffer.SampleCount;

                        _queuedBuffers.TryDequeue(out _);
                    }

                    needUpdate = true;

                    lock (_queueLock)
                    {
                        hasBuffer = _queuedBuffers.TryPeek(out buffer);
                    }
                }

                if (needUpdate)
                {
                    _updateRequiredEvent.Set();
                }

                // No work
                Thread.Sleep(5);
            }


        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _driver.Unregister(this))
            {
                PrepareToClose();
                Stop();

                lock (_trackLock)
                {
                    _audioTrack.Stop();
                    _audioTrack.Dispose();
                }
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
