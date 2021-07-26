namespace SipProvider.SIP
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    using NLog;

    public class SipUserAgent : ISipUserAgent, IDisposable
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly ISipTransport _transport;

        #endregion Fields

        #region Properties

        public string Id { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string Username { get; set; }

        public IPEndPoint Remote { get; set; }

        public IPAddress LocalAddress { get; set; }

        public int LocalPort => _transport.Port;

        #endregion Properties

        #region Events

        public event Action<RequestArgs> RequestReceived;

        #endregion Events

        #region Constructors

        public SipUserAgent(object parent, ISipTransport transport)
        {
            //Parent = parent;
            //ObjectId = SystemIdHelper.Safe(_transport.ToString());
            //_logger = LogManager.GetLogger(SystemIdHelper.GetSystemId(this));
            _logger = LogManager.GetCurrentClassLogger();

            _transport = transport;
            _transport.RequestReceived += OnRequestReceived;
        }

        public void Dispose()
        {
            _transport.RequestReceived -= OnRequestReceived;
        }

        #endregion Constructors

        #region Methods

        public Task<ISipMessage> RegisterAsync(ISipMessage request)
        {
            return _transport.RegisterAsync(this, request);
        }

        public Task<ISipMessage> InviteAsync(ISipMessage request)
        {
            return _transport.InviteAsync(this, request);
        }

        public Task<ISipMessage> ByeAsync(ISipMessage request)
        {
            return _transport.ByeAsync(this, request);
        }

        private void OnRequestReceived(RequestArgs args)
        {
            if (!Remote.Equals(args.Remote))
                return;

            RequestReceived?.Invoke(args);
            args.Handle = true;
        }

        #endregion Methods
    }
}
