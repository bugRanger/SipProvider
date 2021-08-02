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

        public Task<bool> CallAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> WriteAsync() 
        {
            throw new NotImplementedException();
        }

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
                    responseCode = SipResponseCode.Forbidden;
                }
            }

            ResponseAsync(args.Message, responseCode);
        }

        private void HandleCall() 
        {
            //var inviteTransaction = new UASInviteTransaction(_transport, @event.Request, _serverEp);
            //inviteTransaction.UASInviteTransactionCancelled += transaction => _logger.Debug(() => $"Transaction {transaction} cancelled");
            //inviteTransaction.UASInviteTransactionFailed += (transaction, reason) => _logger.Warn(() => $"Transaction {transaction} failed: {reason}");

            //var message = new SipMessage(@event.Request);
            //var call = @event.Client.FindCall(message);
            //if (call != null)
            //{
            //    inviteTransaction.SendFinalResponse(SIPResponse.GetResponse(@event.Request, SIPResponseStatusCodesEnum.BusyHere, null));
            //    return;
            //}

            //var receivedOffer = SDP.ParseSDPDescription(@event.Request.Body);
            //var acceptedFormat = SDPAudioVideoMediaFormat.Empty;

            //SDPMediaAnnouncement acceptedMedia = null;

            //foreach (var rtpmap in @event.Client.SupportedRtpmaps.Values)
            //{
            //    foreach (var media in receivedOffer.Media)
            //    {
            //        acceptedFormat = media.MediaFormats.Values.FirstOrDefault(fmt => fmt.Rtpmap == rtpmap);
            //        if (string.IsNullOrWhiteSpace(acceptedFormat.Rtpmap))
            //            continue;

            //        acceptedMedia = media;
            //        break;
            //    }

            //    if (acceptedFormat.Rtpmap != null)
            //    {
            //        break;
            //    }
            //}

            //if (acceptedMedia == null || acceptedFormat.IsEmpty())
            //{
            //    inviteTransaction.SendFinalResponse(SIPResponse.GetResponse(@event.Request, SIPResponseStatusCodesEnum.NotAcceptable, "Not accepted format"));
            //    return;
            //}

            //uint acceptedSsrc = acceptedMedia.SsrcAttributes.FirstOrDefault()?.SSRC ?? 0;
            //if (acceptedSsrc == 0)
            //{
            //    inviteTransaction.SendFinalResponse(SIPResponse.GetResponse(@event.Request, SIPResponseStatusCodesEnum.NotAcceptable, "Not accepted ssrc"));
            //    return;
            //}

            //var session = _rtpSessionFactory.Take(acceptedSsrc);
            //if (session == null)
            //{
            //    inviteTransaction.SendFinalResponse(SIPResponse.GetResponse(@event.Request, SIPResponseStatusCodesEnum.BusyHere, "All rtp channels are busy"));
            //    return;
            //}

            //var version = GetVersion();
            //var offer = new SDP(_localEp.Address)
            //{
            //    AnnouncementVersion = version,
            //    SessionId = version.ToString(),
            //    Username = @event.Client.Username,
            //    SessionName = DEFAULT_SESSION_NAME,
            //    Connection = new SDPConnectionInformation(_localEp.Address),
            //    Media = { new SDPMediaAnnouncement(acceptedFormat.Kind, session.Port, new List<SDPAudioVideoMediaFormat> { acceptedFormat }) },
            //};

            //call = @event.Client.NewCall(message);

            //if (call.IsRinging)
            //{
            //    var code = inviteTransaction.SendProvisionalResponse(SIPResponse.GetResponse(@event.Request, SIPResponseStatusCodesEnum.Ringing, null)).Result;
            //    if (code != SocketError.Success)
            //    {
            //        CallFree(call, false);
            //        inviteTransaction.SendFinalResponse(SIPResponse.GetResponse(@event.Request, SIPResponseStatusCodesEnum.BusyHere, "All rtp channels are busy"));
            //        return;
            //    }
            //}

            //inviteTransaction.SendFinalResponse(inviteTransaction.GetOkResponse(SDP.SDP_MIME_CONTENTTYPE, offer.ToString()));
        }

        #endregion Methods
    }
}
