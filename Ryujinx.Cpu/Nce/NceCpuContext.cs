using ARMeilleure.Memory;
using ARMeilleure.Translation;
using Ryujinx.Common;
using Ryujinx.Memory;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Cpu.Nce
{
    class NceCpuContext : ICpuContext
    {
        private delegate void ThreadStart(IntPtr nativeContextPtr);
        private static ThreadStart _threadStart;

        private readonly NceTickSource _tickSource;
        private readonly IMemoryManager _memoryManager;
        private readonly Translator _translator;

        static NceCpuContext()
        {
            int size = BitUtils.AlignUp(NceAsmTable.ThreadStartCode.Length, 0x1000);
            MemoryBlock codeBlock = new MemoryBlock((ulong)size);
            codeBlock.Write(0, MemoryMarshal.Cast<uint, byte>(NceAsmTable.ThreadStartCode.AsSpan()));
            codeBlock.Reprotect(0, codeBlock.Size, MemoryPermission.ReadAndExecute, true);
            _threadStart = Marshal.GetDelegateForFunctionPointer<ThreadStart>(codeBlock.Pointer);
        }

        public NceCpuContext(NceTickSource tickSource, IMemoryManager memory, bool for64Bit)
        {
            _tickSource = tickSource;
            _memoryManager = memory;
            _translator = new Translator(new NceMemoryAllocator(), memory, for64Bit);
            memory.UnmapEvent += UnmapHandler;
        }

        private void UnmapHandler(ulong address, ulong size)
        {
            _translator.InvalidateJitCacheRegion(address, size);
        }

        public IExecutionContext CreateExecutionContext()
        {
            return new NceExecutionContext(new NceMemoryAllocator(), _tickSource);
        }

        public void Execute(IExecutionContext context, ulong address)
        {
            _translator.Execute(((NceExecutionContext)context).Impl, address);
        }

        public void InvalidateCacheRegion(ulong address, ulong size)
        {
            _translator.InvalidateJitCacheRegion(address, size);
        }

        public void PatchCodeForNce(ulong textAddress, ulong textSize, ulong patchRegionAddress, ulong patchRegionSize)
        {
            NcePatcher.Patch((IVirtualMemoryManagerTracked)_memoryManager, textAddress, textSize, patchRegionAddress, patchRegionSize);
        }
    }
}
