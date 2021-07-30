namespace SIP.SIPSorcery
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    using global::SIPSorcery.SIP;

    internal class SIPChannelUDP : SIPChannel
    {
        private IPEndPoint listenInterface;

        public SIPChannelUDP(IPEndPoint listenInterface)
        {
            this.listenInterface = listenInterface;
        }

        public Action<object, EventArgs> Connected { get; internal set; }

        public Action<object, EventArgs> Disconnected { get; internal set; }

        internal void Start()
        {
            throw new NotImplementedException();
        }

        internal void Stop()
        {
            throw new NotImplementedException();
        }

        public override Task<SocketError> SendAsync(SIPEndPoint dstEndPoint, byte[] buffer, bool canInitiateConnection, string connectionIDHint = null)
        {
            throw new NotImplementedException();
        }

        public override Task<SocketError> SendSecureAsync(SIPEndPoint dstEndPoint, byte[] buffer, string serverCertificateName, bool canInitiateConnection, string connectionIDHint = null)
        {
            throw new NotImplementedException();
        }

        public override bool HasConnection(string connectionID)
        {
            throw new NotImplementedException();
        }

        public override bool HasConnection(SIPEndPoint remoteEndPoint)
        {
            throw new NotImplementedException();
        }

        public override bool HasConnection(Uri serverUri)
        {
            throw new NotImplementedException();
        }

        public override bool IsAddressFamilySupported(AddressFamily addresFamily)
        {
            throw new NotImplementedException();
        }

        public override bool IsProtocolSupported(SIPProtocolsEnum protocol)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}