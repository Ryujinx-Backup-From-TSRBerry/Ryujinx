using ARMeilleure.Memory;

namespace Ryujinx.Cpu.Nce
{
    public class NceEngine : ICpuEngine
    {
        private readonly NceTickSource _tickSource;
        public ITickSource TickSource => _tickSource;

        public NceEngine(ulong tickFrequency)
        {
            _tickSource = new NceTickSource(tickFrequency);
        }

        public ICpuContext CreateCpuContext(IMemoryManager memoryManager, bool for64Bit)
        {
            return new NceCpuContext(_tickSource, memoryManager, for64Bit);
        }
    }
}