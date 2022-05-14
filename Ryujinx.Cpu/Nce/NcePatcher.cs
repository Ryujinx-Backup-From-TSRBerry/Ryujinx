using Ryujinx.Common;
using Ryujinx.Memory;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Cpu.Nce
{
    static class NcePatcher
    {
        public static void Patch(
            IVirtualMemoryManagerTracked memoryManager,
            ulong textAddress,
            ulong textSize,
            ulong patchRegionAddress,
            ulong patchRegionSize)
        {
            ulong patchRegionStart = patchRegionAddress;

            ulong address = textAddress;
            while (address < textAddress + textSize)
            {
                uint inst = memoryManager.Read<uint>(address);

                if ((inst & ~(0xffffu << 5)) == 0xd4000001u) // svc #0
                {
                    uint svcId = (ushort)(inst >> 5);
                    PatchInstruction(address, WriteSvcPatch(memoryManager, ref patchRegionAddress, ref patchRegionSize, address, svcId));
                    System.Console.WriteLine($"SVC #{svcId} at 0x{address:X}");
                }
                else if ((inst & ~0x1f) == 0xd53bd060) // mrs x0, tpidrro_el0
                {
                    uint rd = inst & 0x1f;
                    PatchInstruction(address, WriteMrsTpidrroEl0Patch(memoryManager, ref patchRegionAddress, ref patchRegionSize, address, rd));
                    System.Console.WriteLine($"MRS x{rd}, tpidrro_el0 at 0x{address:X}");
                }
                else if ((inst & ~0x1f) == 0xd53bd040) // mrs x0, tpidr_el0
                {
                    uint rd = inst & 0x1f;
                    PatchInstruction(address, WriteMrsTpidrEl0Patch(memoryManager, ref patchRegionAddress, ref patchRegionSize, address, rd));
                    System.Console.WriteLine($"MRS x{rd}, tpidr_el0 at 0x{address:X}");
                }
                else if ((inst & ~0x1f) == 0xd51bd040) // msr tpidr_el0, x0
                {
                    uint rd = inst & 0x1f;
                    PatchInstruction(address, WriteMsrTpidrEl0Patch(memoryManager, ref patchRegionAddress, ref patchRegionSize, address, rd));
                    System.Console.WriteLine($"MSR tpidr_el0, x{rd} at 0x{address:X}");
                }

                address += 4;
            }

            ulong patchRegionConsumed = BitUtils.AlignUp(patchRegionAddress - patchRegionStart, 0x1000);
            if (patchRegionConsumed != 0)
            {
                memoryManager.Reprotect(patchRegionStart, patchRegionConsumed, MemoryPermission.ReadAndExecute);
            }
        }

        private static void PatchInstruction(ulong instructionAddress, ulong targetAddress)
        {

        }

        private static ulong WriteSvcPatch(
            IVirtualMemoryManagerTracked memoryManager,
            ref ulong patchRegionAddress,
            ref ulong patchRegionSize,
            ulong svcAddress,
            uint svcId)
        {
            uint[] code = GetCopy(NceAsmTable.SvcPatchCode);
            int movIndex = Array.IndexOf(code, 0xD2800000u);

            ulong targetAddress = GetPatchWriteAddress(ref patchRegionAddress, ref patchRegionSize, code.Length);

            code[movIndex] |= svcId << 5;
            code[code.Length - 1] |= GetImm26(targetAddress, svcAddress);

            WriteCode(memoryManager, targetAddress, code);

            return targetAddress;
        }

        private static ulong WriteMrsTpidrroEl0Patch(
            IVirtualMemoryManagerTracked memoryManager,
            ref ulong patchRegionAddress,
            ref ulong patchRegionSize,
            ulong mrsAddress,
            uint rd)
        {
            uint[] code = GetCopy(NceAsmTable.MrsTpidrroEl0PatchCode);

            ulong targetAddress = GetPatchWriteAddress(ref patchRegionAddress, ref patchRegionSize, code.Length);

            code[0] |= rd;
            code[1] |= rd | (rd << 5);
            code[2] |= GetImm26(targetAddress, mrsAddress);

            WriteCode(memoryManager, targetAddress, code);

            return targetAddress;
        }

        private static ulong WriteMrsTpidrEl0Patch(
            IVirtualMemoryManagerTracked memoryManager,
            ref ulong patchRegionAddress,
            ref ulong patchRegionSize,
            ulong mrsAddress,
            uint rd)
        {
            uint[] code = GetCopy(NceAsmTable.MrsTpidrEl0PatchCode);

            ulong targetAddress = GetPatchWriteAddress(ref patchRegionAddress, ref patchRegionSize, code.Length);

            code[0] |= rd;
            code[1] |= rd | (rd << 5);
            code[2] |= GetImm26(targetAddress, mrsAddress);

            WriteCode(memoryManager, targetAddress, code);

            return targetAddress;
        }

        private static ulong WriteMsrTpidrEl0Patch(
            IVirtualMemoryManagerTracked memoryManager,
            ref ulong patchRegionAddress,
            ref ulong patchRegionSize,
            ulong msrAddress,
            uint rd)
        {
            uint r2 = rd == 0 ? 1u : 0u;

            uint[] code = GetCopy(NceAsmTable.MsrTpidrEl0PatchCode);

            ulong targetAddress = GetPatchWriteAddress(ref patchRegionAddress, ref patchRegionSize, code.Length);

            code[0] |= r2;
            code[1] |= r2;
            code[2] |= rd | (r2 << 5);
            code[3] |= r2;
            code[4] |= GetImm26(targetAddress, msrAddress);

            WriteCode(memoryManager, targetAddress, code);

            return targetAddress;
        }

        private static uint GetImm26(ulong sourceAddress, ulong targetAddress)
        {
            long offset = (long)(targetAddress - sourceAddress);
            long offsetTrunc = (offset >> 2) & 0x3FFFFFF;

            if ((offsetTrunc << 38) >> 36 != offset)
            {
                throw new Exception($"Offset out of range: 0x{sourceAddress:X} -> 0x{targetAddress:X} (0x{offset:X})");
            }

            return (uint)offsetTrunc;
        }

        private static uint[] GetCopy(uint[] code)
        {
            uint[] codeCopy = new uint[code.Length];
            code.CopyTo(codeCopy, 0);

            return codeCopy;
        }

        private static ulong GetPatchWriteAddress(ref ulong patchRegionAddress, ref ulong patchRegionSize, int length)
        {
            ulong byteLength = (ulong)length * 4;
            ulong address = patchRegionAddress;
            patchRegionAddress += byteLength;
            patchRegionSize -= byteLength;

            return address;
        }

        private static void WriteCode(IVirtualMemoryManagerTracked memoryManager, ulong address, uint[] code)
        {
            memoryManager.Write(address, MemoryMarshal.Cast<uint, byte>(code.AsSpan()));
        }
    }
}