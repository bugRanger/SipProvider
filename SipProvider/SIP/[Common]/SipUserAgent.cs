namespace SipProvider.SIP
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
        private readonly ISipProvider _transport;

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

        public event Action<RequestArgs> CallRequestReceived;
        public event Action<RequestArgs> TextRequestReceived;
        public event Action<RequestArgs> RequestReceived;

        #endregion Events

        #region Constructors

        public SipUserAgent(object parent, ISipProvider transport)
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

        public Task<ISipMessage> RequestAsync(ISipMessage message)
        {
            return _transport.RequestAsync(this, message);
        }

        public Task<SocketError> ResponseAsync(ISipMessage message, SipResponseCode code)
        {
            return _transport.ResponseAsync(this, message, code);
        }

        private void OnRequestReceived(RequestWithHandleArgs args)
        {
            if (!Remote.Equals(args.Remote))
                return;

            args.Handle = true;

            if (args.Message.IsText())
            {
                TextRequestReceived?.Invoke(args);
                return;
            }

            if (args.Message.IsCall())
            {
                CallRequestReceived?.Invoke(args);
                return;
            }

            RequestReceived?.Invoke(args);
        }

        #endregion Methods
    }
}
