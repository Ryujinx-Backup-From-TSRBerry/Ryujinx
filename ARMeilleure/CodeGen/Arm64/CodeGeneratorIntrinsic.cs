using ARMeilleure.CodeGen.Linking;
using ARMeilleure.CodeGen.Optimizations;
using ARMeilleure.CodeGen.RegisterAllocators;
using ARMeilleure.CodeGen.Unwinding;
using ARMeilleure.Common;
using ARMeilleure.Diagnostics;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using static ARMeilleure.IntermediateRepresentation.Operand;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.CodeGen.Arm64
{
    static class CodeGeneratorIntrinsic
    {
        public static void GenerateOperation(CodeGenContext context, Operation operation)
        {
            Intrinsic intrin = operation.Intrinsic;

            IntrinsicInfo info = IntrinsicTable.GetInfo(intrin & ~(Intrinsic.Arm64VTypeMask | Intrinsic.Arm64VSizeMask));

            switch (info.Type)
            {
                case IntrinsicType.ftypeRnRd:
                case IntrinsicType.szRnRd:
                    GenerateScalarUnaryFP(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0));
                    break;
                case IntrinsicType.sfftypeRnRd:
                    GenerateScalarUnaryFPToGP(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0));
                    break;
                case IntrinsicType.ftypeRmRnRd:
                    GenerateScalarBinaryFP(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0),
                        operation.GetSource(1));
                    break;
                case IntrinsicType.ftypeRmRaRnRd:
                    GenerateScalarTernaryFP(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0),
                        operation.GetSource(1),
                        operation.GetSource(2));
                    break;
                case IntrinsicType.QszRnRd:
                    GenerateVectorUnaryFP(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0));
                    break;
                case IntrinsicType.QRmRnRd:
                    GenerateVectorBinary(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0),
                        operation.GetSource(1));
                    break;
                case IntrinsicType.QszRmRnRd:
                    GenerateVectorBinaryFP(
                        context,
                        (uint)(intrin & Intrinsic.Arm64VTypeMask) >> (int)Intrinsic.Arm64VTypeShift,
                        (uint)(intrin & Intrinsic.Arm64VSizeMask) >> (int)Intrinsic.Arm64VSizeShift,
                        info.Inst,
                        operation.Destination,
                        operation.GetSource(0),
                        operation.GetSource(1));
                    break;
                default:
                    throw new NotImplementedException(info.Type.ToString());
            }
        }

        private static void GenerateScalarUnaryFP(
            CodeGenContext context,
            uint sz,
            uint instruction,
            Operand rd,
            Operand rn)
        {
            instruction |= (sz << 22);

            context.Assembler.WriteInstruction(instruction, rd, rn);
        }

        private static void GenerateScalarUnaryFPToGP(
            CodeGenContext context,
            uint sz,
            uint instruction,
            Operand rd,
            Operand rn)
        {
            instruction |= (sz << 22);

            context.Assembler.WriteInstructionAuto(instruction, rd, rn);
        }

        private static void GenerateScalarBinaryFP(
            CodeGenContext context,
            uint sz,
            uint instruction,
            Operand rd,
            Operand rn,
            Operand rm)
        {
            instruction |= (sz << 22);

            context.Assembler.WriteInstructionRm16(instruction, rd, rn, rm);
        }

        private static void GenerateScalarTernaryFP(
            CodeGenContext context,
            uint sz,
            uint instruction,
            Operand rd,
            Operand rn,
            Operand rm,
            Operand ra)
        {
            instruction |= (sz << 22);

            context.Assembler.WriteInstruction(instruction, rd, rn, rm, ra);
        }

        private static void GenerateVectorUnaryFP(
            CodeGenContext context,
            uint q,
            uint sz,
            uint instruction,
            Operand rd,
            Operand rn)
        {
            instruction |= (q << 30) | (sz << 22);

            context.Assembler.WriteInstruction(instruction, rd, rn);
        }

        private static void GenerateVectorBinary(
            CodeGenContext context,
            uint q,
            uint instruction,
            Operand rd,
            Operand rn,
            Operand rm)
        {
            instruction |= (q << 30);

            context.Assembler.WriteInstructionRm16(instruction, rd, rn, rm);
        }

        private static void GenerateVectorBinaryFP(
            CodeGenContext context,
            uint q,
            uint sz,
            uint instruction,
            Operand rd,
            Operand rn,
            Operand rm)
        {
            instruction |= (q << 30) | (sz << 22);

            context.Assembler.WriteInstructionRm16(instruction, rd, rn, rm);
        }
    }
}