namespace SipProvider.SIP
{
    using System;
    using System.Net;

    public interface ISipUserAgent
    {
        #region Properties

        string Id { get; }

        string Login { get; }

        string Password { get; }

        string Username { get; }

        IPEndPoint Remote { get; }

        #endregion Properties

        #region Events

        event Action<ISipMessage> InboundRequestReceived;

        event Action<ISipMessage> CallRequestReceived;

        #endregion Events

        #region Methods

        void Register(ISipMessage request);

        void Invite(ISipMessage request);

        void Bye(ISipMessage request);

        #endregion Methods
    }
}
