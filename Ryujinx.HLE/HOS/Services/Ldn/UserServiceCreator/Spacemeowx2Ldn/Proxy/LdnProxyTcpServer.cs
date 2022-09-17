﻿using System;
using NetCoreServer;
using Ryujinx.Common.Logging;
using System.Net;
using System.Net.Sockets;

namespace Ryujinx.HLE.HOS.Services.Ldn.UserServiceCreator.Spacemeowx2Ldn.Proxy
{
    internal class LdnProxyTcpServer : NetCoreServer.TcpServer, ILdnTcpSocket
    {
        private LanProtocol _protocol;

        public LdnProxyTcpServer(LanProtocol protocol, IPAddress address, int port) : base(address, port)
        {
            _protocol               = protocol;
            OptionReceiveBufferSize = LanProtocol.BufferSize;
            OptionSendBufferSize    = LanProtocol.BufferSize;
            OptionReuseAddress      = true;
            OptionNoDelay           = true;

            Logger.Debug?.PrintMsg(LogClass.ServiceLdn, $"LdnProxyTCPServer created a server for this address: {address}:{port}");
        }

        protected override TcpSession CreateSession()
        {
            return new LdnProxyTcpSession(this, _protocol);
        }

        protected override void OnError(SocketError error)
        {
            Logger.Error?.PrintMsg(LogClass.ServiceLdn, $"LdnProxyTCPServer caught an error with code {error}");
        }

        protected override void Dispose(bool disposingManagedResources)
        {
            Stop();
            base.Dispose(disposingManagedResources);
        }

        public bool ConnectAsync()
        {
            throw new InvalidOperationException("ConnectAsync was called.");
        }

        public bool Connect()
        {
            throw new InvalidOperationException("Connect was called.");
        }

        public void DisconnectAndStop()
        {
            Stop();
        }

        public bool SendPacketAsync(EndPoint endpoint, byte[] buffer)
        {
            throw new InvalidOperationException("SendPacketAsync was called.");
        }
    }
}