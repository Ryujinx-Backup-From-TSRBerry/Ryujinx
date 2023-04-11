namespace Ryujinx.HLE.HOS.Services.Apm
{
    // NOTE: This service doesn’t exist anymore after firmware 7.0.1. But some outdated homebrew still uses it.

    [Service("apm:p")] // 1.0.0-7.0.1
    class IManagerPrivileged : IpcService
    {
#pragma warning disable IDE0060
        public IManagerPrivileged(ServiceCtx context) { }
#pragma warning restore IDE0060

        [CommandHipc(0)]
        // OpenSession() -> object<nn::apm::ISession>
        public ResultCode OpenSession(ServiceCtx context)
        {
            MakeObject(context, new SessionServer(context));

            return ResultCode.Success;
        }
    }
}