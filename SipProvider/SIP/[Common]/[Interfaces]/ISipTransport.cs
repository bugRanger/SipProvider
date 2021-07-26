namespace SipProvider.SIP
{
    using System;
    using System.Threading.Tasks;

    public interface ISipTransport
    {
        #region Properties

        int Port { get; }

        #endregion Properties

        #region Events

        event Action<RequestArgs> RequestReceived;

        #endregion Events

        #region Methods
        
        Task<ISipMessage> RegisterAsync(ISipUserAgent agent, ISipMessage message);

        Task<ISipMessage> InviteAsync(ISipUserAgent agent, ISipMessage message);

        Task<ISipMessage> ByeAsync(ISipUserAgent agent, ISipMessage message);

        #endregion Methods
    }
}
