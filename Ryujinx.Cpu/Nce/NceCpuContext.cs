using ARMeilleure.Memory;
using ARMeilleure.Signal;
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
        private delegate IntPtr GetTpidrEl0();
        private static MemoryBlock _codeBlock;
        private static ThreadStart _threadStart;
        private static GetTpidrEl0 _getTpidrEl0;

        private readonly NceTickSource _tickSource;
        private readonly IMemoryManager _memoryManager;

        static NceCpuContext()
        {
            ulong threadStartCodeSize = (ulong)NceAsmTable.ThreadStartCode.Length * 4;
            ulong enEntryCodeOffset = threadStartCodeSize;
            ulong ehEntryCodeSize = (ulong)NceAsmTable.ExceptionHandlerEntryCode.Length * 4;
            ulong getTpidrEl0CodeOffset = threadStartCodeSize + ehEntryCodeSize;
            ulong getTpidrEl0CodeSize = (ulong)NceAsmTable.GetTpidrEl0Code.Length * 4;

            ulong size = BitUtils.AlignUp(threadStartCodeSize + ehEntryCodeSize + getTpidrEl0CodeSize, 0x1000);

            MemoryBlock codeBlock = new MemoryBlock(size);

            codeBlock.Write(0, MemoryMarshal.Cast<uint, byte>(NceAsmTable.ThreadStartCode.AsSpan()));
            codeBlock.Write(getTpidrEl0CodeOffset, MemoryMarshal.Cast<uint, byte>(NceAsmTable.GetTpidrEl0Code.AsSpan()));

            NativeSignalHandler.InitializeJitCache(new JitMemoryAllocator());

            NativeSignalHandler.InitializeSignalHandler((IntPtr signalHandlerPtr) =>
            {
                uint[] ehEntryCode = NcePatcher.GenerateExceptionHandlerEntry(signalHandlerPtr);
                codeBlock.Write(enEntryCodeOffset, MemoryMarshal.Cast<uint, byte>(ehEntryCode.AsSpan()));
                codeBlock.Reprotect(0, size, MemoryPermission.ReadAndExecute, true);
                return codeBlock.GetPointer(enEntryCodeOffset, ehEntryCodeSize);
            }, NceThreadPal.UnixSuspendSignal);

            _threadStart = Marshal.GetDelegateForFunctionPointer<ThreadStart>(codeBlock.GetPointer(0, threadStartCodeSize));
            _getTpidrEl0 = Marshal.GetDelegateForFunctionPointer<GetTpidrEl0>(codeBlock.GetPointer(getTpidrEl0CodeOffset, getTpidrEl0CodeSize));
            _codeBlock = codeBlock;
        }

        public NceCpuContext(NceTickSource tickSource, IMemoryManager memory, bool for64Bit)
        {
            _tickSource = tickSource;
            _memoryManager = memory;
        }

        public IExecutionContext CreateExecutionContext()
        {
            return new NceExecutionContext();
        }

        public void Execute(IExecutionContext context, ulong address)
        {
            NceExecutionContext nec = (NceExecutionContext)context;
            NceNativeInterface.RegisterThread(nec, _tickSource);
            NceThreadTable.Register(_getTpidrEl0(), nec.NativeContextPtr);

            // System.Console.WriteLine($"going to start {System.Threading.Thread.CurrentThread.Name} at 0x{address:X} 0x{_codeBlock.Pointer.ToInt64():X} 0x{nec.NativeContextPtr.ToInt64():X}");
            nec.SetStartAddress(address);
            _threadStart(nec.NativeContextPtr);
            // System.Console.WriteLine($"thread {System.Threading.Thread.CurrentThread.Name} exited successfully");

            NceThreadTable.Unregister(nec.NativeContextPtr);
        }

        public void InvalidateCacheRegion(ulong address, ulong size)
        {
        }

        public void PatchCodeForNce(ulong textAddress, ulong textSize, ulong patchRegionAddress, ulong patchRegionSize)
        {
            NcePatcher.Patch((IVirtualMemoryManagerTracked)_memoryManager, textAddress, textSize, patchRegionAddress, patchRegionSize);
        }
    }
}
