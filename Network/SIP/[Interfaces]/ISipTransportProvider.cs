namespace Network.SIP
{
    using System;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    using Network.SDP;

    public interface ISipTransportProvider
    {
        #region Properties

        int Port { get; }

        #endregion Properties

        #region Events

        event Action<RequestWithHandleArgs> RequestReceived;

        #endregion Events

        #region Methods

        Task<ISipMessage> RequestAsync(ISipUserAgent agent, ISipMessage message);

        Task<SocketError> ResponseAsync(ISipUserAgent agent, ISipMessage message, SipResponseCode code);

        SdpMessage CreateSdp();

        #endregion Methods
    }
}
