using Ryujinx.Memory;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Cpu.Nce
{
    static class NceThreadTable
    {
        private const int MaxThreads = 4096;

        private struct Entry
        {
            public IntPtr ThreadId;
            public IntPtr NativeContextPtr;

            public Entry(IntPtr threadId, IntPtr nativeContextPtr)
            {
                ThreadId = threadId;
                NativeContextPtr = nativeContextPtr;
            }
        }

        private static MemoryBlock _block;
        private static Queue<int> _freeSlots;

        public static IntPtr EntriesPointer => _block.Pointer + 8;

        static NceThreadTable()
        {
            _block = new MemoryBlock((ulong)Unsafe.SizeOf<Entry>() * MaxThreads + 8UL);
            _block.Write(0UL, 0UL);
            _freeSlots = new Queue<int>();
        }

        public static void Register(IntPtr threadId, IntPtr nativeContextPtr)
        {
            Span<Entry> entries = GetStorage();

            lock (_block)
            {
                if (_freeSlots.TryDequeue(out int freeSlot))
                {
                    entries[freeSlot] = new Entry(threadId, nativeContextPtr);
                }
                else
                {
                    int slot = (int)(GetThreadsCount()++);
                    if (slot == MaxThreads)
                    {
                        throw new Exception($"Number of active threads exceeds limit of {MaxThreads}.");
                    }

                    entries[slot] = new Entry(threadId, nativeContextPtr);
                }
            }
        }

        public static void Unregister(IntPtr nativeContextPtr)
        {
            Span<Entry> entries = GetStorage();

            lock (_block)
            {
                ref ulong threadsCount = ref GetThreadsCount();

                for (int i = 0; i < (int)threadsCount; i++)
                {
                    if (entries[i].NativeContextPtr == nativeContextPtr)
                    {
                        entries[i] = default;
                        _freeSlots.Enqueue(i);
                        break;
                    }
                }

                for (int i = (int)threadsCount - 1; i >= 0; i--)
                {
                    if (entries[i].NativeContextPtr != IntPtr.Zero)
                    {
                        break;
                    }

                    threadsCount--;
                }
            }
        }

        private static ref ulong GetThreadsCount()
        {
            return ref _block.GetRef<ulong>(0UL);
        }

        private static unsafe Span<Entry> GetStorage()
        {
            return new Span<Entry>((void*)_block.GetPointer(8UL, (ulong)Unsafe.SizeOf<Entry>() * MaxThreads), MaxThreads);
        }
    }
}