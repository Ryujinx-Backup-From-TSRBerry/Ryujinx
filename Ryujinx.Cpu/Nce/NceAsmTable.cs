namespace Ryujinx.Cpu.Nce
{
    static class NceAsmTable
    {
        public static uint[] ThreadStartCode = new uint[]
        {
            0x910003E1u, // mov x1, sp
            0xD53BD042u, // mrs x2, tpidr_el0
            0xF9019001u, // str x1, [x0, #0x320]
            0xF9019402u, // str x2, [x0, #0x328]
            0xD51BD040u, // msr tpidr_el0, x0
            0xA9410C02u, // ldp x2, x3, [x0, #0x10]
            0xA9421404u, // ldp x4, x5, [x0, #0x20]
            0xA9431C06u, // ldp x6, x7, [x0, #0x30]
            0xA9442408u, // ldp x8, x9, [x0, #0x40]
            0xA9452C0Au, // ldp x10, x11, [x0, #0x50]
            0xA946340Cu, // ldp x12, x13, [x0, #0x60]
            0xA9473C0Eu, // ldp x14, x15, [x0, #0x70]
            0xA9484410u, // ldp x16, x17, [x0, #0x80]
            0xA9494C12u, // ldp x18, x19, [x0, #0x90]
            0xA94A5414u, // ldp x20, x21, [x0, #0xA0]
            0xA94B5C16u, // ldp x22, x23, [x0, #0xB0]
            0xA94C6418u, // ldp x24, x25, [x0, #0xC0]
            0xA94D6C1Au, // ldp x26, x27, [x0, #0xD0]
            0xA94E741Cu, // ldp x28, x29, [x0, #0xE0]
            0xAD480400u, // ldp q0, q1, [x0, #0x100]
            0xAD490C02u, // ldp q2, q3, [x0, #0x120]
            0xAD4A1404u, // ldp q4, q5, [x0, #0x140]
            0xAD4B1C06u, // ldp q6, q7, [x0, #0x160]
            0xAD4C2408u, // ldp q8, q9, [x0, #0x180]
            0xAD4D2C0Au, // ldp q10, q11, [x0, #0x1A0]
            0xAD4E340Cu, // ldp q12, q13, [x0, #0x1C0]
            0xAD4F3C0Eu, // ldp q14, q15, [x0, #0x1E0]
            0xAD504410u, // ldp q16, q17, [x0, #0x200]
            0xAD514C12u, // ldp q18, q19, [x0, #0x220]
            0xAD525414u, // ldp q20, q21, [x0, #0x240]
            0xAD535C16u, // ldp q22, q23, [x0, #0x260]
            0xAD546418u, // ldp q24, q25, [x0, #0x280]
            0xAD556C1Au, // ldp q26, q27, [x0, #0x2A0]
            0xAD56741Cu, // ldp q28, q29, [x0, #0x2C0]
            0xAD577C1Eu, // ldp q30, q31, [x0, #0x2E0]
            0xA94F041Eu, // ldp x30, x1, [x0, #0xF0]
            0x9100003Fu, // mov sp, x1
            0xA9400400u, // ldp x0, x1, [x0, #0x0]
            0xD61F03C0u  // br x30
        };

        public static uint[] SvcPatchCode = new uint[]
        {
            0xF81F83F3u, // str x19, [sp, #-8]
            0xD53BD053u, // mrs x19, tpidr_el0
            0xA9000660u, // stp x0, x1, [x19, #0x0]
            0xA9010E62u, // stp x2, x3, [x19, #0x10]
            0xA9021664u, // stp x4, x5, [x19, #0x20]
            0xA9031E66u, // stp x6, x7, [x19, #0x30]
            0xA9042668u, // stp x8, x9, [x19, #0x40]
            0xA9052E6Au, // stp x10, x11, [x19, #0x50]
            0xA906366Cu, // stp x12, x13, [x19, #0x60]
            0xA9073E6Eu, // stp x14, x15, [x19, #0x70]
            0xA9084670u, // stp x16, x17, [x19, #0x80]
            0xF85F83E0u, // ldr x0, [sp, #-8]
            0xA9090272u, // stp x18, x0, [x19, #0x90]
            0xA90A5674u, // stp x20, x21, [x19, #0xA0]
            0xA90B5E76u, // stp x22, x23, [x19, #0xB0]
            0xA90C6678u, // stp x24, x25, [x19, #0xC0]
            0xA90D6E7Au, // stp x26, x27, [x19, #0xD0]
            0xA90E767Cu, // stp x28, x29, [x19, #0xE0]
            0x910003E0u, // mov x0, sp
            0xA90F027Eu, // stp x30, x0, [x19, #0xF0]
            0xAD080660u, // stp q0, q1, [x19, #0x100]
            0xAD090E62u, // stp q2, q3, [x19, #0x120]
            0xAD0A1664u, // stp q4, q5, [x19, #0x140]
            0xAD0B1E66u, // stp q6, q7, [x19, #0x160]
            0xAD0C2668u, // stp q8, q9, [x19, #0x180]
            0xAD0D2E6Au, // stp q10, q11, [x19, #0x1A0]
            0xAD0E366Cu, // stp q12, q13, [x19, #0x1C0]
            0xAD0F3E6Eu, // stp q14, q15, [x19, #0x1E0]
            0xAD104670u, // stp q16, q17, [x19, #0x200]
            0xAD114E72u, // stp q18, q19, [x19, #0x220]
            0xAD125674u, // stp q20, q21, [x19, #0x240]
            0xAD135E76u, // stp q22, q23, [x19, #0x260]
            0xAD146678u, // stp q24, q25, [x19, #0x280]
            0xAD156E7Au, // stp q26, q27, [x19, #0x2A0]
            0xAD16767Cu, // stp q28, q29, [x19, #0x2C0]
            0xAD177E7Eu, // stp q30, q31, [x19, #0x2E0]
            0xF9419260u, // ldr x0, [x19, #0x320]
            0x9100001Fu, // mov sp, x0
            0xF9419660u, // ldr x0, [x19, #0x328]
            0xD51BD040u, // msr tpidr_el0, x0
            0xD2800000u, // mov x0, #0
            0xF941AA68u, // ldr x8, [x19, #0x350]
            0xD63F0100u, // blr x8
            0xD51BD053u, // msr tpidr_el0, x19
            0xA94F027Eu, // ldp x30, x0, [x19, #0xF0]
            0x9100001Fu, // mov sp, x0
            0xA9400660u, // ldp x0, x1, [x19, #0x0]
            0xA9410E62u, // ldp x2, x3, [x19, #0x10]
            0xA9421664u, // ldp x4, x5, [x19, #0x20]
            0xA9431E66u, // ldp x6, x7, [x19, #0x30]
            0xA9442668u, // ldp x8, x9, [x19, #0x40]
            0xA9452E6Au, // ldp x10, x11, [x19, #0x50]
            0xA946366Cu, // ldp x12, x13, [x19, #0x60]
            0xA9473E6Eu, // ldp x14, x15, [x19, #0x70]
            0xA9484670u, // ldp x16, x17, [x19, #0x80]
            0xF9404A72u, // ldr x18, [x19, #0x90]
            0xA94A5674u, // ldp x20, x21, [x19, #0xA0]
            0xA94B5E76u, // ldp x22, x23, [x19, #0xB0]
            0xA94C6678u, // ldp x24, x25, [x19, #0xC0]
            0xA94D6E7Au, // ldp x26, x27, [x19, #0xD0]
            0xA94E767Cu, // ldp x28, x29, [x19, #0xE0]
            0xAD480660u, // ldp q0, q1, [x19, #0x100]
            0xAD490E62u, // ldp q2, q3, [x19, #0x120]
            0xAD4A1664u, // ldp q4, q5, [x19, #0x140]
            0xAD4B1E66u, // ldp q6, q7, [x19, #0x160]
            0xAD4C2668u, // ldp q8, q9, [x19, #0x180]
            0xAD4D2E6Au, // ldp q10, q11, [x19, #0x1A0]
            0xAD4E366Cu, // ldp q12, q13, [x19, #0x1C0]
            0xAD4F3E6Eu, // ldp q14, q15, [x19, #0x1E0]
            0xAD504670u, // ldp q16, q17, [x19, #0x200]
            0xAD514E72u, // ldp q18, q19, [x19, #0x220]
            0xAD525674u, // ldp q20, q21, [x19, #0x240]
            0xAD535E76u, // ldp q22, q23, [x19, #0x260]
            0xAD546678u, // ldp q24, q25, [x19, #0x280]
            0xAD556E7Au, // ldp q26, q27, [x19, #0x2A0]
            0xAD56767Cu, // ldp q28, q29, [x19, #0x2C0]
            0xAD577E7Eu, // ldp q30, q31, [x19, #0x2E0]
            0xF9404E73u, // ldr x19, [x19, #0x98]
            0x14000000u  // self: b self
        };

        public static uint[] MrsTpidrroEl0PatchCode = new uint[]
        {
            0xD53BD040u, // mrs x0, tpidr_el0
            0xF9418400u, // ldr x0, [x0, 0x308]
            0x14000000u  // self: b self
        };

        public static uint[] MrsTpidrEl0PatchCode = new uint[]
        {
            0xD53BD040u, // mrs x0, tpidr_el0
            0xF9418000u, // ldr x0, [x0, 0x300]
            0x14000000u  // self: b self
        };

        public static uint[] MsrTpidrEl0PatchCode = new uint[]
        {
            0xF81F83E0u, // str x0, [sp, #-8]
            0xD53BD040u, // mrs x0, tpidr_el0
            0xF9018000u, // str x0, [x0, 0x300]
            0xF85F83E0u, // ldr x0, [sp, #-8]
            0x14000000u  // self: b self
        };
    }
}