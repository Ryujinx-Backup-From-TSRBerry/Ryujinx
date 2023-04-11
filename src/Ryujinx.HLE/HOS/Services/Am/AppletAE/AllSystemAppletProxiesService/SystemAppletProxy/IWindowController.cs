using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class IWindowController : IpcService
    {
        private readonly ulong _pid;

        public IWindowController(ulong pid)
        {
            _pid = pid;
        }

        [CommandHipc(1)]
        // GetAppletResourceUserId() -> nn::applet::AppletResourceUserId
        public ResultCode GetAppletResourceUserId(ServiceCtx context)
        {
            long appletResourceUserId = context.Device.System.AppletState.AppletResourceUserIds.Add(_pid);

            context.ResponseData.Write(appletResourceUserId);

            Logger.Stub?.PrintStub(LogClass.ServiceAm, new { appletResourceUserId });

            return ResultCode.Success;
        }

        [CommandHipc(10)]
        // AcquireForegroundRights()
#pragma warning disable IDE0060
        public static ResultCode AcquireForegroundRights(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }
#pragma warning restore IDE0060
    }
}