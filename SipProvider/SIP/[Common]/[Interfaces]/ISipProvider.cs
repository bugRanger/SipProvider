namespace SipProvider.SIP
{
    using System;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public interface ISipProvider
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

        #endregion Methods
    }
}
