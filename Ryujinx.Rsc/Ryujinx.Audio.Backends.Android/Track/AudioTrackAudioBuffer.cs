namespace Ryujinx.Audio.Backends.Android.Track
{
    internal class AudioTrackAudioBuffer
    {
        public readonly ulong DriverIdentifier;
        public readonly ulong SampleCount;
        public readonly byte[] Data;
        public ulong SamplePlayed;

        public AudioTrackAudioBuffer(ulong driverIdentifier, byte[] data, ulong sampleCount)
        {
            DriverIdentifier = driverIdentifier;
            Data = data;
            SampleCount = sampleCount;
            SamplePlayed = 0;
        }
    }
}
