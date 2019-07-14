namespace Ryujinx.HLE.HOS.Services.Apm
{
    [Service("apm")]
    [Service("apm:p")]
    class IManager : IpcService
    {
        public IManager(ServiceCtx context) { }

        [Command(0)]
        // OpenSession() -> object<nn::apm::ISession>
        public ResultCode OpenSession(ServiceCtx context)
        {
            MakeObject(context, new ISession());

            return ResultCode.Success;
        }
    }
}