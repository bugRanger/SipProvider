namespace SipProvider.SIP
{
    using System;
    using System.Net;
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

        event Action<RequestArgs> RequestReceived;

        #endregion Events

        #region Methods

        Task<ISipMessage> RegisterAsync(ISipMessage request);

        Task<ISipMessage> InviteAsync(ISipMessage request);

        Task<ISipMessage> ByeAsync(ISipMessage request);

        #endregion Methods
    }
}
