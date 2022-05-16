namespace Ryujinx.HLE.HOS.Kernel.Process
{
    struct ProcessCreationInfo
    {
        public string Name { get; }

        public int Version { get; }
        public ulong TitleId { get; }

        public ulong CodeAddress { get; }
        public int CodePagesCount { get; }

        public ProcessCreationFlags Flags { get; }
        public int ResourceLimitHandle { get; }
        public int SystemResourcePagesCount { get; }

        public ProcessCreationInfo(
            string name,
            int version,
            ulong titleId,
            ulong codeAddress,
            int codePagesCount,
            ProcessCreationFlags flags,
            int resourceLimitHandle,
            int systemResourcePagesCount)
        {
            if ((flags & ProcessCreationFlags.AddressSpaceMask) == ProcessCreationFlags.AddressSpace64Bit)
            {
                flags &= ~ProcessCreationFlags.AddressSpaceMask;
                flags |= ProcessCreationFlags.AddressSpace64BitDeprecated;
            }
            Name = name;
            Version = version;
            TitleId = titleId;
            CodeAddress = codeAddress;
            CodePagesCount = codePagesCount;
            Flags = flags;
            ResourceLimitHandle = resourceLimitHandle;
            SystemResourcePagesCount = systemResourcePagesCount;
        }
    }
}