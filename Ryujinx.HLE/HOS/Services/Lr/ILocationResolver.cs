﻿using LibHac.Fs.NcaUtils;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.FileSystem.Content;
using System.Text;

using static Ryujinx.HLE.Utilities.StringUtils;

namespace Ryujinx.HLE.HOS.Services.Lr
{
    class ILocationResolver : IpcService
    {
        private StorageId _storageId;

        public ILocationResolver(StorageId storageId)
        {
            _storageId = storageId;
        }

        [Command(0)]
        // ResolveProgramPath()
        public ResultCode ResolveProgramPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            if (ResolvePath(context, titleId, ContentType.Program))
            {
                return ResultCode.Success;
            }
            else
            {
                return ResultCode.ProgramLocationEntryNotFound;
            }
        }

        [Command(1)]
        // RedirectProgramPath()
        public ResultCode RedirectProgramPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 0, ContentType.Program);

            return ResultCode.Success;
        }

        [Command(2)]
        // ResolveApplicationControlPath()
        public ResultCode ResolveApplicationControlPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            if (ResolvePath(context, titleId, ContentType.Control))
            {
                return ResultCode.Success;
            }
            else
            {
                return ResultCode.AccessDenied;
            }
        }

        [Command(3)]
        // ResolveApplicationHtmlDocumentPath()
        public ResultCode ResolveApplicationHtmlDocumentPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            if (ResolvePath(context, titleId, ContentType.Manual))
            {
                return ResultCode.Success;
            }
            else
            {
                return ResultCode.AccessDenied;
            }
        }

        [Command(4)]
        // ResolveDataPath()
        public ResultCode ResolveDataPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            if (ResolvePath(context, titleId, ContentType.Data) || ResolvePath(context, titleId, ContentType.PublicData))
            {
                return ResultCode.Success;
            }
            else
            {
                return ResultCode.AccessDenied;
            }
        }

        [Command(5)]
        // RedirectApplicationControlPath()
        public ResultCode RedirectApplicationControlPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 1, ContentType.Control);

            return ResultCode.Success;
        }

        [Command(6)]
        // RedirectApplicationHtmlDocumentPath()
        public ResultCode RedirectApplicationHtmlDocumentPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 1, ContentType.Manual);

            return ResultCode.Success;
        }

        [Command(7)]
        // ResolveApplicationLegalInformationPath()
        public ResultCode ResolveApplicationLegalInformationPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            if (ResolvePath(context, titleId, ContentType.Manual))
            {
                return ResultCode.Success;
            }
            else
            {
                return ResultCode.AccessDenied;
            }
        }

        [Command(8)]
        // RedirectApplicationLegalInformationPath()
        public ResultCode RedirectApplicationLegalInformationPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 1, ContentType.Manual);

            return ResultCode.Success;
        }

        [Command(9)]
        // Refresh()
        public ResultCode Refresh(ServiceCtx context)
        {
            context.Device.System.ContentManager.RefreshEntries(_storageId, 1);

            return ResultCode.Success;
        }

        [Command(10)]
        // SetProgramNcaPath2()
        public ResultCode SetProgramNcaPath2(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 1, ContentType.Program);

            return ResultCode.Success;
        }

        [Command(11)]
        // ClearLocationResolver2()
        public ResultCode ClearLocationResolver2(ServiceCtx context)
        {
            context.Device.System.ContentManager.RefreshEntries(_storageId, 1);

            return ResultCode.Success;
        }

        [Command(12)]
        // DeleteProgramNcaPath()
        public ResultCode DeleteProgramNcaPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            DeleteContentPath(context, titleId, ContentType.Program);

            return ResultCode.Success;
        }

        [Command(13)]
        // DeleteControlNcaPath()
        public ResultCode DeleteControlNcaPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            DeleteContentPath(context, titleId, ContentType.Control);

            return ResultCode.Success;
        }

        [Command(14)]
        // DeleteDocHtmlNcaPath()
        public ResultCode DeleteDocHtmlNcaPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            DeleteContentPath(context, titleId, ContentType.Manual);

            return ResultCode.Success;
        }

        [Command(15)]
        // DeleteInfoHtmlNcaPath()
        public ResultCode DeleteInfoHtmlNcaPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            DeleteContentPath(context, titleId, ContentType.Manual);

            return ResultCode.Success;
        }

        private void RedirectPath(ServiceCtx context, long titleId, int flag, ContentType contentType)
        {
            string        contentPath = ReadUtf8String(context);
            LocationEntry newLocation = new LocationEntry(contentPath, flag, titleId, contentType);

            context.Device.System.ContentManager.RedirectLocation(newLocation, _storageId);
        }

        private bool ResolvePath(ServiceCtx context, long titleId,ContentType contentType)
        {
            ContentManager contentManager = context.Device.System.ContentManager;
            string         contentPath    = contentManager.GetInstalledContentPath(titleId, _storageId, ContentType.Program);

            if (!string.IsNullOrWhiteSpace(contentPath))
            {
                long position = context.Request.RecvListBuff[0].Position;
                long size     = context.Request.RecvListBuff[0].Size;

                byte[] contentPathBuffer = Encoding.UTF8.GetBytes(contentPath);

                context.Memory.WriteBytes(position, contentPathBuffer);
            }
            else
            {
                return false;
            }

            return true;
        }

        private void DeleteContentPath(ServiceCtx context, long titleId, ContentType contentType)
        {
            ContentManager contentManager = context.Device.System.ContentManager;
            string         contentPath    = contentManager.GetInstalledContentPath(titleId, _storageId, ContentType.Manual);

            contentManager.ClearEntry(titleId, ContentType.Manual, _storageId);
        }
    }
}
