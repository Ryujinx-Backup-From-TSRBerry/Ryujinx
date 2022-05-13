<<<<<<<< HEAD:Ryujinx.Tests/Cpu/CpuContext.cs
using ARMeilleure.Memory;
using ARMeilleure.State;
========
﻿using ARMeilleure.Memory;
>>>>>>>> 7758bd9ac (Refactor CPU interface):Ryujinx.Cpu/Jit/JitCpuContext.cs
using ARMeilleure.Translation;
using Ryujinx.Cpu;
using Ryujinx.Cpu.Jit;

<<<<<<<< HEAD:Ryujinx.Tests/Cpu/CpuContext.cs
namespace Ryujinx.Tests.Cpu
========
namespace Ryujinx.Cpu.Jit
>>>>>>>> 7758bd9ac (Refactor CPU interface):Ryujinx.Cpu/Jit/JitCpuContext.cs
{
    class JitCpuContext : ICpuContext
    {
        private readonly JitTickSource _tickSource;
        private readonly Translator _translator;

        public JitCpuContext(JitTickSource tickSource, IMemoryManager memory, bool for64Bit)
        {
            _tickSource = tickSource;
            _translator = new Translator(new JitMemoryAllocator(), memory, for64Bit);
            memory.UnmapEvent += UnmapHandler;
        }

        private void UnmapHandler(ulong address, ulong size)
        {
            _translator.InvalidateJitCacheRegion(address, size);
        }

        public IExecutionContext CreateExecutionContext()
        {
<<<<<<<< HEAD:Ryujinx.Tests/Cpu/CpuContext.cs
            return new ExecutionContext(new JitMemoryAllocator(), new TickSource(19200000));
========
            return new JitExecutionContext(new JitMemoryAllocator(), _tickSource);
>>>>>>>> 7758bd9ac (Refactor CPU interface):Ryujinx.Cpu/Jit/JitCpuContext.cs
        }

        public void Execute(IExecutionContext context, ulong address)
        {
            _translator.Execute(((JitExecutionContext)context).Impl, address);
        }

        public void InvalidateCacheRegion(ulong address, ulong size)
        {
            _translator.InvalidateJitCacheRegion(address, size);
        }
    }
}
