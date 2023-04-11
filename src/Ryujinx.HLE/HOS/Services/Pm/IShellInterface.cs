﻿namespace Ryujinx.HLE.HOS.Services.Pm
{
    [Service("pm:shell")]
    class IShellInterface : IpcService
    {
#pragma warning disable IDE0060
        public IShellInterface(ServiceCtx context) { }
#pragma warning restore IDE0060

        [CommandHipc(6)]
        // GetApplicationPid() -> u64
        public static ResultCode GetApplicationPid(ServiceCtx context)
        {
            // FIXME: This is wrong but needed to make hb loader works
            // TODO: Change this when we will have a way to process via a PM like interface.
            ulong pid = context.Process.Pid;

            context.ResponseData.Write(pid);

            return ResultCode.Success;
        }
    }
}