namespace Ryujinx.Audio.Backends.Android.AAudio
{
    internal class AAudioAudioBuffer
    {
        public readonly ulong DriverIdentifier;
        public readonly ulong SampleCount;
        public ulong SamplePlayed;

        public AAudioAudioBuffer(ulong driverIdentifier, ulong sampleCount)
        {
            DriverIdentifier = driverIdentifier;
            SampleCount = sampleCount;
            SamplePlayed = 0;
        }
    }
}
