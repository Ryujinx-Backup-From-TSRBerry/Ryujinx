using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;
using System;
using System.Diagnostics;
using System.Reflection;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.Instructions
{
    static class InstEmitSimdNativeArmHelper
    {
        public static void EmitScalarUnaryOpF(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n));
        }

        public static void EmitScalarUnaryOpFToGp(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdCvt op = (OpCodeSimdCvt)context.CurrOp;

            Operand n = GetVec(op.Rn);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            SetIntOrZR(context, op.Rd, op.RegisterSize == RegisterSize.Int32
                ? context.AddIntrinsicInt (inst, n)
                : context.AddIntrinsicLong(inst, n));
        }

        public static void EmitScalarBinaryOpF(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n, m));
        }

        public static void EmitScalarTernaryOpF(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);
            Operand a = GetVec(op.Ra);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n, m, a));
        }

        public static void EmitVectorUnaryOpF(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n));
        }

        public static void EmitVectorBinaryOp(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n, m));
        }

        public static void EmitVectorBinaryOpF(ArmEmitterContext context, Intrinsic inst)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand n = GetVec(op.Rn);
            Operand m = GetVec(op.Rm);

            if ((op.Size & 1) != 0)
            {
                inst |= Intrinsic.Arm64VDouble;
            }

            if (op.RegisterSize == RegisterSize.Simd128)
            {
                inst |= Intrinsic.Arm64V128;
            }

            context.Copy(GetVec(op.Rd), context.AddIntrinsic(inst, n, m));
        }
    }
}