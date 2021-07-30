namespace Network.SIP
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    using NLog;

    public delegate void CallRequestReceivedArgs();

    public class SipUserAgent : ISipUserAgent, IDisposable
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly ISipTransportProvider _provider;
        private readonly ISipUserAgentHandler _handler;

        #endregion Fields

        #region Properties

        public string Id { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string Username { get; set; }

        public IPEndPoint Remote { get; set; }

        public IPAddress LocalAddress { get; set; }

        public int LocalPort => _provider.Port;

        #endregion Properties

        #region Constructors

        public SipUserAgent(object parent, ISipTransportProvider provider, ISipUserAgentHandler handler)
        {
            //Parent = parent;
            //ObjectId = SystemIdHelper.Safe(_transport.ToString());
            //_logger = LogManager.GetLogger(SystemIdHelper.GetSystemId(this));
            _logger = LogManager.GetCurrentClassLogger();

            _provider = provider;
            _provider.RequestReceived += OnRequestReceived;

            _handler = handler;
        }

        public void Dispose()
        {
            _provider.RequestReceived -= OnRequestReceived;
        }

        #endregion Constructors

        #region Methods

        public Task<ISipMessage> RequestAsync(ISipMessage message)
        {
            return _provider.RequestAsync(this, message);
        }

        public Task<SocketError> ResponseAsync(ISipMessage message, SipResponseCode code)
        {
            return _provider.ResponseAsync(this, message, code);
        }

        private void OnRequestReceived(RequestWithHandleArgs args)
        {
            if (!Remote.Equals(args.Remote))
                return;

            args.Handle = true;

            SipResponseCode responseCode;

            if (args.Message.IsText())
            {
                responseCode = _handler.IncomingMessage(this, args);
            }
            else
            {
                if (args.Message.IsCall())
                {
                    responseCode = _handler.IncomingCall(this, args);
                }
                else
                {
                    responseCode = _handler.IncomingRequest(this, args);
                }
            }

            ResponseAsync(args.Message, responseCode);
        }

        #endregion Methods
    }
}
