namespace SipProvider.SIP
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public interface ISipUserAgent
    {
        #region Properties

        string Id { get; }

        string Login { get; }

        string Password { get; }

        string Username { get; }

        IPEndPoint Remote { get; }

        IPAddress LocalAddress { get; }

        int LocalPort { get; }

        #endregion Properties

        #region Events

        event Action<RequestArgs> CallRequestReceived;
        event Action<RequestArgs> TextRequestReceived;
        event Action<RequestArgs> RequestReceived;

        #endregion Events

        #region Methods

        Task<ISipMessage> RequestAsync(ISipMessage message);

        Task<SocketError> ResponseAsync(ISipMessage message, SipResponseCode code);

        #endregion Methods
    }
}
