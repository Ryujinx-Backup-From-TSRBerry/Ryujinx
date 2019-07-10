﻿using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Nfc.Nfp
{
    [Service("nfp:user")]
    class IUserManager : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IUserManager(ServiceCtx context)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, GetUserInterface }
            };
        }

        public long GetUserInterface(ServiceCtx context)
        {
            MakeObject(context, new IUser());

            return 0;
        }
    }
}