using System;
using System.Net;

namespace SipProvider.SIP
{
    public class SipUserAgent : ISipUserAgent
    {
        #region Fields

        private readonly ISipProvider _provider;
        private int _creq;

        #endregion Fields

        #region Properties

        public string Id { get; }

        public string Login { get; }

        public string Password { get; }

        public string Username { get; }

        public IPEndPoint Remote { get; }

        #endregion Properties

        #region Events

        public event Action<ISipMessage> InboundRequestReceived;

        public event Action<ISipMessage> CallRequestReceived;

        #endregion Events

        #region Constructors

        public SipUserAgent(ISipProvider provider)
        {
            _provider = provider;
            _creq = 1;
        }

        #endregion Constructors

        #region Methods

        public void Register(ISipMessage request) { }

        public void Invite(ISipMessage request) { }

        public void Bye(ISipMessage request) { }

        #endregion Methods
    }
}
