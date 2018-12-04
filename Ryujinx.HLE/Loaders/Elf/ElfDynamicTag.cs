using System.Diagnostics.CodeAnalysis;

namespace Ryujinx.HLE.Loaders.Elf
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    enum ElfDynamicTag
    {
        DT_NULL            = 0,
        DT_NEEDED          = 1,
        DT_PLTRELSZ        = 2,
        DT_PLTGOT          = 3,
        DT_HASH            = 4,
        DT_STRTAB          = 5,
        DT_SYMTAB          = 6,
        DT_RELA            = 7,
        DT_RELASZ          = 8,
        DT_RELAENT         = 9,
        DT_STRSZ           = 10,
        DT_SYMENT          = 11,
        DT_INIT            = 12,
        DT_FINI            = 13,
        DT_SONAME          = 14,
        DT_RPATH           = 15,
        DT_SYMBOLIC        = 16,
        DT_REL             = 17,
        DT_RELSZ           = 18,
        DT_RELENT          = 19,
        DT_PLTREL          = 20,
        DT_DEBUG           = 21,
        DT_TEXTREL         = 22,
        DT_JMPREL          = 23,
        DT_BIND_NOW        = 24,
        DT_INIT_ARRAY      = 25,
        DT_FINI_ARRAY      = 26,
        DT_INIT_ARRAYSZ    = 27,
        DT_FINI_ARRAYSZ    = 28,
        DT_RUNPATH         = 29,
        DT_FLAGS           = 30,
        DT_ENCODING        = 32,
        DT_PREINIT_ARRAY   = 32,
        DT_PREINIT_ARRAYSZ = 33,
        DT_GNU_PRELINKED   = 0x6ffffdf5,
        DT_GNU_CONFLICTSZ  = 0x6ffffdf6,
        DT_GNU_LIBLISTSZ   = 0x6ffffdf7,
        DT_CHECKSUM        = 0x6ffffdf8,
        DT_PLTPADSZ        = 0x6ffffdf9,
        DT_MOVEENT         = 0x6ffffdfa,
        DT_MOVESZ          = 0x6ffffdfb,
        DT_FEATURE_1       = 0x6ffffdfc,
        DT_POSFLAG_1       = 0x6ffffdfd,
        DT_SYMINSZ         = 0x6ffffdfe,
        DT_SYMINENT        = 0x6ffffdff,
        DT_GNU_HASH        = 0x6ffffef5,
        DT_TLSDESC_PLT     = 0x6ffffef6,
        DT_TLSDESC_GOT     = 0x6ffffef7,
        DT_GNU_CONFLICT    = 0x6ffffef8,
        DT_GNU_LIBLIST     = 0x6ffffef9,
        DT_CONFIG          = 0x6ffffefa,
        DT_DEPAUDIT        = 0x6ffffefb,
        DT_AUDIT           = 0x6ffffefc,
        DT_PLTPAD          = 0x6ffffefd,
        DT_MOVETAB         = 0x6ffffefe,
        DT_SYMINFO         = 0x6ffffeff,
        DT_VERSYM          = 0x6ffffff0,
        DT_RELACOUNT       = 0x6ffffff9,
        DT_RELCOUNT        = 0x6ffffffa,
        DT_FLAGS_1         = 0x6ffffffb,
        DT_VERDEF          = 0x6ffffffc,
        DT_VERDEFNUM       = 0x6ffffffd,
        DT_VERNEED         = 0x6ffffffe,
        DT_VERNEEDNUM      = 0x6fffffff,
        DT_AUXILIARY       = 0x7ffffffd,
        DT_FILTER          = 0x7fffffff
    }
}