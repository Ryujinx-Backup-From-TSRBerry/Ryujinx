using ARMeilleure.Memory;
using ARMeilleure.Signal;
using ARMeilleure.Translation;
using Ryujinx.Common;
using Ryujinx.Memory;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ryujinx.Cpu.Nce
{
    class NceCpuContext : ICpuContext
    {
        private delegate void ThreadStart(IntPtr nativeContextPtr);
        private delegate IntPtr GetTpidrEl0();
        private static MemoryBlock _codeBlock;
        private static ThreadStart _threadStart;
        private static GetTpidrEl0 _getTpidrEl0;

        private readonly ITickSource _tickSource;
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

            NativeSignalHandler.InitializeSignalHandler((IntPtr oldSignalHandlerSegfaultPtr, IntPtr signalHandlerPtr) =>
            {
                uint[] ehEntryCode = NcePatcher.GenerateExceptionHandlerEntry(oldSignalHandlerSegfaultPtr, signalHandlerPtr);
                codeBlock.Write(enEntryCodeOffset, MemoryMarshal.Cast<uint, byte>(ehEntryCode.AsSpan()));
                codeBlock.Reprotect(0, size, MemoryPermission.ReadAndExecute, true);
                return codeBlock.GetPointer(enEntryCodeOffset, ehEntryCodeSize);
            }, NceThreadPal.UnixSuspendSignal);

            _threadStart = Marshal.GetDelegateForFunctionPointer<ThreadStart>(codeBlock.GetPointer(0, threadStartCodeSize));
            _getTpidrEl0 = Marshal.GetDelegateForFunctionPointer<GetTpidrEl0>(codeBlock.GetPointer(getTpidrEl0CodeOffset, getTpidrEl0CodeSize));
            _codeBlock = codeBlock;
        }

        public NceCpuContext(ITickSource tickSource, IMemoryManager memory, bool for64Bit)
        {
            _tickSource = tickSource;
            _memoryManager = memory;
        }

        public IExecutionContext CreateExecutionContext(ExceptionCallbacks exceptionCallbacks)
        {
            return new NceExecutionContext(exceptionCallbacks);
        }

        public void Execute(IExecutionContext context, ulong address)
        {
            NceExecutionContext nec = (NceExecutionContext)context;
            NceNativeInterface.RegisterThread(nec, _tickSource);
            int tableIndex = NceThreadTable.Register(_getTpidrEl0(), nec.NativeContextPtr);

            nec.SetStartAddress(address);
            _threadStart(nec.NativeContextPtr);

            NceThreadTable.Unregister(tableIndex);
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
