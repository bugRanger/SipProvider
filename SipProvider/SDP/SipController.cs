namespace Network.SIP
{
    using System;
    using System.Text;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Collections.Concurrent;

    using RTP;

    public class SipController 
    {
        #region Constants

        // TODO AIS timings quite tight, I hope it cannot be an issue
        private const int ADMISSIBLE_LAG = 20;

        private const string DEFAULT_SESSION_NAME = "-";

        #endregion Constants

        #region Fields

        private readonly Logger _logger;
        private readonly IPEndPoint _listenInterface;
        private readonly SIPTransport _transport;
        private readonly SipUdpChannel _channel;
        private readonly IRtpController _rtpController;
        private readonly ITimelineEnvironment _timeline;
        private readonly SupervisorThread<BaseEvent> _supervisor;
        private readonly ConcurrentDictionary<string, ISipClient> _clients;
        private readonly Dictionary<string, SIPDialogue> _dialogs;
        private readonly Dictionary<string, ISipCall> _calls;

        private SIPEndPoint _serverEp;
        private SIPEndPoint _localEp;
        private SIPURI _serverUri;
        private SIPURI _localUri;
        private int _cseq;

        #endregion Fields

        #region Properties

        public object Parent { get; }

        public object ObjectId { get; }

        #endregion Properties

        #region Constructors

        public SipController(object parent, IPEndPoint listenInterface, IRtpController rtpController, ITimelineEnvironment timeline)
        {
            Parent = parent;
            ObjectId = SystemIdHelper.Safe(listenInterface.ToString());
            _logger = LogManager.GetLogger(SystemIdHelper.GetSystemId(this));

            _listenInterface = listenInterface;
            _rtpController = rtpController;
            _timeline = timeline;

            _clients = new ConcurrentDictionary<string, ISipClient>();
            _calls = new Dictionary<string, ISipCall>();
            _dialogs = new Dictionary<string, SIPDialogue>();

            _transport = new SIPTransport();
            _transport.SIPTransportRequestReceived += OnRequestReceived;
            _transport.SIPTransportResponseReceived += OnResponseReceived;

            _channel = new SipUdpChannel(_listenInterface);
            _channel.Connected += (s, e) => _transport.AddSIPChannel(_channel);
            _channel.Disconnected += (s, e) => _transport.RemoveSIPChannel(_channel);

            _supervisor = new SupervisorThread<BaseEvent>(ADMISSIBLE_LAG, m => m.Handle(this));
            _supervisor.Tick += SupervisorOnTick;
            _supervisor.Trace += ev => _logger.Trace(ev);
            _supervisor.Error += ex => _logger.Error(ex);

            _cseq = 1;
        }

        #endregion Constructors

        #region Methods

        public void Start(IPAddress local, IPEndPoint remote)
        {
            _localEp = new SIPEndPoint(SIPProtocolsEnum.udp, local, _listenInterface.Port);
            _localUri = new SIPURI(SIPSchemesEnum.sip, local, _listenInterface.Port);

            _serverEp = new SIPEndPoint(SIPProtocolsEnum.udp, remote.Address, remote.Port);
            _serverUri = new SIPURI(SIPSchemesEnum.sip, _serverEp.Address, _serverEp.Port);

            _channel.Start();
            _supervisor.Start();
        }

        public void Stop()
        {
            _supervisor.Stop();
            _channel.Stop();
        }


        public void Subscribe(ISipClient client)
        {
            _clients.TryAdd(client.Id, client);
        }

        public void Unsubscribe(ISipClient client)
        {
            _clients.TryRemove(client.Id, out _);
        }


        public void Register(ISipClient client, ISipMessage request, Action<ISipMessage, bool> callback)
        {
            Enqueue(new RegisterRequestEvent(client, request, callback));
        }

        public void Invite(ISipClient client, ISipMessage request, Action<ISipCall, bool> callback)
        {
            Enqueue(new InviteRequestEvent(client, request, callback));
        }

        public void Bye(ISipCall call)
        {
            Enqueue(new ByeRequestEvent(call));
        }


        internal void Handle(RegisterRequestEvent @event)
        {
            var request = SIPRequest.GetRequest(
                SIPMethodsEnum.REGISTER,
                new SIPURI(SIPSchemesEnum.sip, _serverEp.Address, _serverEp.Port),
                new SIPToHeader(null, new SIPURI(@event.Request.TargetId, _serverUri.Host, null, SIPSchemesEnum.sip), null),
                new SIPFromHeader(null, new SIPURI(@event.Request.SourceId, _serverUri.Host, null, SIPSchemesEnum.sip), CallProperties.CreateNewTag()));

            request.Header.Contact = new List<SIPContactHeader>
            {
                new SIPContactHeader(null, new SIPURI(@event.Client.Id, _localUri.Host, null, _localUri.Scheme))
            };

            if (@event.Request.Headers.Count > 0)
            {
                request.Header.UnknownHeaders.AddRange(@event.Request.Headers);
            }

            SendNonInviteRequest(@event.Client, request)
                .ContinueWith(task => Enqueue(new RegisterResponseEvent(task, @event.Callback)));
        }

        internal void Handle(RegisterResponseEvent @event)
        {
            bool success =
                @event.Task.IsCompleted &&
                @event.Task.Result.TransactionFinalResponse.Status == SIPResponseStatusCodesEnum.Ok;

            SipMessage message = null;

            if (success)
            {
                message = new SipMessage(@event.Task.Result.TransactionFinalResponse);
            }

            @event.Callback(message, success);
        }

        internal void Handle(InviteRequestEvent @event)
        {
            var call = @event.Client.FindCall(@event.Request);
            if (call != null)
            {
                @event.Callback(call, true);
                return;
            }

            var session = _rtpController.Take();
            if (session == null)
            {
                @event.Callback(call, false);
                return;
            }

            var request = SIPRequest.GetRequest(
                SIPMethodsEnum.INVITE,
                new SIPURI(@event.Request.TargetId, _serverUri.Host, null, SIPSchemesEnum.sip),
                new SIPToHeader(null, new SIPURI(@event.Request.TargetId, _serverUri.Host, null, SIPSchemesEnum.sip), null),
                new SIPFromHeader(null, new SIPURI(@event.Request.SourceId, _localUri.Host, null, SIPSchemesEnum.sip), CallProperties.CreateNewTag()));

            request.Header.Contact = new List<SIPContactHeader>
            {
                new SIPContactHeader(null, new SIPURI(@event.Client.Id, _localUri.Host, null, _localUri.Scheme))
            };

            if (@event.Request.Headers.Any())
            {
                request.Header.UnknownHeaders.AddRange(@event.Request.Headers);
            }

            var rtpmaps = @event.Client.SupportedRtpmaps.Select(s => new SDPAudioVideoMediaFormat(SDPMediaTypesEnum.audio, s.Key, s.Value)).ToList();

            var version = GetVersion();
            var offer = new SDP(_localEp.Address)
            {
                AnnouncementVersion = version,
                SessionId = version.ToString(),
                Username = @event.Client.Username,
                SessionName = DEFAULT_SESSION_NAME,
                Connection = new SDPConnectionInformation(_localEp.Address),
                Media =
                {
                    new SDPMediaAnnouncement(SDPMediaTypesEnum.audio, session.Port, rtpmaps)
                    {
                        ExtraMediaAttributes = @event.Request.MediaAttributes.ToList()
                    }
                },
            };

            var offerRaw = offer.ToString();

            request.Header.ContentType = SDP.SDP_MIME_CONTENTTYPE;
            request.Header.ContentLength = offerRaw.Length;
            request.Body = offerRaw;

            call = @event.Client.NewCall(@event.Request);
            call.Bind(session);

            @event.Callback(call, true);

            SendInviteRequest(@event.Client, request)
                .ContinueWith(task => Enqueue(new InviteResponseEvent(call, task)));
        }

        internal void Handle(InviteResponseEvent @event)
        {
            if (!@event.InviteTransaction.IsCompleted)
            {
                CallFree(@event.Call, false);
                return;
            }

            var transation = @event.InviteTransaction.Result;
            if (transation.TransactionFinalResponse.Status != SIPResponseStatusCodesEnum.Ok)
            {
                CallFree(@event.Call, false);
                return;
            }

            var sdp = SDP.ParseSDPDescription(transation.TransactionFinalResponse.Body);
            var media = sdp.Media.FirstOrDefault();
            if (media == null)
            {
                CallFree(@event.Call, false);
                return;
            }

            var format = media.MediaFormats.Values.FirstOrDefault();
            if (format.IsEmpty())
            {
                CallFree(@event.Call, false);
                return;
            }

            var dialog = new SIPDialogue(transation);
            var dialogId = GetDialogId(dialog);

            _dialogs[dialogId] = dialog;
            _calls[dialogId] = @event.Call;

            @event.Call.Bind(new SipCallSettings
            {
                Remote = new IPEndPoint(_serverEp.Address, media.Port),
                DialogId = dialogId,
                FormatId = format.ID,
                FormatProfile = format.Rtpmap,
            });

            @event.Call.Open();
        }

        internal void Handle(ByeRequestEvent @event)
        {
            CallFree(@event.Call, true);
        }

        internal void Handle(InviteEvent @event)
        {
            var inviteTransaction = new UASInviteTransaction(_transport, @event.Request, _serverEp);
            inviteTransaction.UASInviteTransactionCancelled += transaction => _logger.Debug(() => $"Transaction {transaction} cancelled");
            inviteTransaction.UASInviteTransactionFailed += (transaction, reason) => _logger.Warn(() => $"Transaction {transaction} failed: {reason}");

            var message = new SipMessage(@event.Request);
            var call = @event.Client.FindCall(message);
            if (call != null)
            {
                inviteTransaction.SendFinalResponse(SIPResponse.GetResponse(@event.Request, SIPResponseStatusCodesEnum.BusyHere, null));
                return;
            }

            var receivedOffer = SDP.ParseSDPDescription(@event.Request.Body);
            var acceptedFormat = SDPAudioVideoMediaFormat.Empty;

            SDPMediaAnnouncement acceptedMedia = null;

            foreach (var rtpmap in @event.Client.SupportedRtpmaps.Values)
            {
                foreach (var media in receivedOffer.Media)
                {
                    acceptedFormat = media.MediaFormats.Values.FirstOrDefault(fmt => fmt.Rtpmap == rtpmap);
                    if (string.IsNullOrWhiteSpace(acceptedFormat.Rtpmap))
                        continue;

                    acceptedMedia = media;
                    break;
                }

                if (acceptedFormat.Rtpmap != null)
                {
                    break;
                }
            }

            if (acceptedMedia == null || acceptedFormat.IsEmpty())
            {
                inviteTransaction.SendFinalResponse(SIPResponse.GetResponse(@event.Request, SIPResponseStatusCodesEnum.NotAcceptable, "Not accepted format"));
                return;
            }

            var session = _rtpController.Take();
            if (session == null)
            {
                inviteTransaction.SendFinalResponse(SIPResponse.GetResponse(@event.Request, SIPResponseStatusCodesEnum.BusyHere, "All rtp channels are busy"));
                return;
            }

            var version = GetVersion();
            var offer = new SDP(_localEp.Address)
            {
                AnnouncementVersion = version,
                SessionId = version.ToString(),
                Username = @event.Client.Username,
                SessionName = DEFAULT_SESSION_NAME,
                Connection = new SDPConnectionInformation(_localEp.Address),
                Media = { new SDPMediaAnnouncement(acceptedFormat.Kind, session.Port, new List<SDPAudioVideoMediaFormat> { acceptedFormat }) },
            };

            var dialog = new SIPDialogue(inviteTransaction);
            var dialogId = GetDialogId(dialog);

            call = @event.Client.NewCall(message);
            call.Bind(session);
            call.Bind(new SipCallSettings
            {
                Remote = new IPEndPoint(_serverEp.Address, acceptedMedia.Port),
                DialogId = dialogId,
                FormatId = acceptedFormat.ID,
                FormatProfile = acceptedFormat.Rtpmap,
            });

            _dialogs[dialogId] = dialog;
            _calls[dialogId] = call;

            if (call.IsRinging)
            {
                var code = inviteTransaction.SendProvisionalResponse(SIPResponse.GetResponse(@event.Request, SIPResponseStatusCodesEnum.Ringing, null)).Result;
                if (code != SocketError.Success)
                {
                    CallFree(call, false);
                    inviteTransaction.SendFinalResponse(SIPResponse.GetResponse(@event.Request, SIPResponseStatusCodesEnum.BusyHere, "All rtp channels are busy"));
                    return;
                }
            }

            inviteTransaction.SendFinalResponse(inviteTransaction.GetOkResponse(SDP.SDP_MIME_CONTENTTYPE, offer.ToString()));
        }

        internal void Handle(ByeEvent @event)
        {
            if (!_calls.TryGetValue(CalcDialogId(@event.Request, true), out ISipCall call) &&
                !_calls.TryGetValue(CalcDialogId(@event.Request, false), out call))
            {
                LogResult(_transport.SendResponseAsync(SIPResponse.GetResponse(@event.Request, SIPResponseStatusCodesEnum.CallLegTransactionDoesNotExist, null)));
                return;
            }

            CallFree(call, false);
            LogResult(_transport.SendResponseAsync(SIPResponse.GetResponse(@event.Request, SIPResponseStatusCodesEnum.Ok, null)));
        }

        internal void Handle(MessageEvent @event)
        {
            SIPResponseStatusCodesEnum code = @event.Client.HandleMessage(new SipMessage(@event.Request))
                                                                              ? SIPResponseStatusCodesEnum.Ok
                                                                              : SIPResponseStatusCodesEnum.NotFound;

            LogResult(_transport.SendResponseAsync(SIPResponse.GetResponse(@event.Request, code, null)));
        }


        private Task<SIPNonInviteTransaction> SendNonInviteRequest(ISipClient client, SIPRequest request)
        {
            var task = new TaskCompletionSource<SIPNonInviteTransaction>();

            var registrationTransaction = new SIPNonInviteTransaction(_transport, request, _serverEp);

            request.Header.UserAgent = client.Username;
            request.Header.CSeq = _cseq++;

            registrationTransaction.NonInviteTransactionFinalResponseReceived += (localEp, remoteEp, transaction, response) =>
            {
                _logger.Debug(() => "Received response: " + response);

                if (response.Status == SIPResponseStatusCodesEnum.Ok)
                {
                    _logger.Debug(() => $"Succeed: {localEp} {remoteEp} {transaction} {response}");
                    task.SetResult(registrationTransaction);
                }
                else if (response.Status == SIPResponseStatusCodesEnum.Unauthorised)
                {
                    var authHeader = response.Header.AuthenticationHeader;
                    if (authHeader != null)
                    {
                        var requestWithAuth = request.Copy();
                        requestWithAuth.SetSendFromHints(request.LocalSIPEndPoint);
                        requestWithAuth.Header.Vias.TopViaHeader.Branch = CallProperties.CreateBranchId();
                        requestWithAuth.Header.From.FromTag = CallProperties.CreateNewTag();
                        requestWithAuth.Header.To.ToTag = null;
                        requestWithAuth.Header.CallId = CallProperties.CreateNewCallId();

                        var digest = authHeader.SIPDigest;
                        digest.SetCredentials(client.Login, client.Password, request.URI.ToString(), request.Method.ToString());
                        requestWithAuth.Header.AuthenticationHeader = new SIPAuthenticationHeader(digest)
                        {
                            SIPDigest = { Response = digest.Digest }
                        };

                        Join(SendNonInviteRequest(client, requestWithAuth), task);
                    }
                    else
                    {
                        var message = $"Failed: {transaction} {response}";
                        _logger.Error(message);
                        task.SetException(new Exception(message));
                    }
                }
                else
                {
                    task.SetResult(registrationTransaction);
                    _logger.Error(() => $"Something bad happen: {localEp} {remoteEp} {transaction} {response}");
                }

                return Task.FromResult(SocketError.Success);
            };

            registrationTransaction.NonInviteTransactionFailed += (transaction, reason) =>
            {
                var message = $"Failed: {transaction} {reason}";
                _logger.Error(message);
                task.SetException(new Exception(message));
            };

            _logger.Debug(() => "Send request: " + request);

            registrationTransaction.SendRequest();
            return task.Task;
        }

        // TODO Add cancellationToken
        // Don't try to combine it with SendNonInviteRequest, I already tried and failed
        private Task<UACInviteTransaction> SendInviteRequest(ISipClient client, SIPRequest request)
        {
            var task = new TaskCompletionSource<UACInviteTransaction>();

            var inviteTransaction = new UACInviteTransaction(_transport, request, _serverEp);

            request.Header.UserAgent = client.Username;
            request.Header.CSeq = _cseq++;

            inviteTransaction.UACInviteTransactionFinalResponseReceived += (localEp, remoteEp, transaction, response) =>
            {
                _logger.Debug(() => "Received response: " + response);

                if (response.Status == SIPResponseStatusCodesEnum.Ok)
                {
                    _logger.Debug(() => $"Succeed: {localEp} {remoteEp} {transaction} {response}");
                    task.SetResult(inviteTransaction);
                }
                else if (response.Status == SIPResponseStatusCodesEnum.Unauthorised)
                {
                    var authHeader = response.Header.AuthenticationHeader;
                    if (authHeader != null)
                    {
                        var requestWithAuth = request.Copy();
                        requestWithAuth.SetSendFromHints(request.LocalSIPEndPoint);
                        requestWithAuth.Header.Vias.TopViaHeader.Branch = CallProperties.CreateBranchId();
                        requestWithAuth.Header.From.FromTag = CallProperties.CreateNewTag();
                        requestWithAuth.Header.To.ToTag = null;
                        requestWithAuth.Header.CallId = CallProperties.CreateNewCallId();

                        var digest = authHeader.SIPDigest;
                        digest.SetCredentials(client.Login, client.Password, request.URI.ToString(), request.Method.ToString());
                        requestWithAuth.Header.AuthenticationHeader = new SIPAuthenticationHeader(digest)
                        {
                            SIPDigest = { Response = digest.Digest }
                        };

                        Join(SendInviteRequest(client, requestWithAuth), task);
                    }
                    else
                    {
                        var message = $"Failed: {transaction} {response}";
                        _logger.Error(message);
                        task.SetException(new Exception(message));
                    }
                }
                else
                {
                    task.SetResult(inviteTransaction);
                    _logger.Error(() => $"Something bad happen: {localEp} {remoteEp} {transaction} {response}");
                }

                return Task.FromResult(SocketError.Success);
            };

            inviteTransaction.UACInviteTransactionFailed += (transaction, reason) =>
            {
                var message = $"Failed: {transaction} {reason}";
                _logger.Error(message);
                task.SetException(new Exception(message));
            };

            _logger.Debug(() => "Send request: " + request);

            inviteTransaction.SendInviteRequest();
            return task.Task;
        }

        private Task OnRequestReceived(SIPEndPoint local, SIPEndPoint remote, SIPRequest request)
        {
            var taskSource = new TaskCompletionSource<bool>();

            if (!_serverEp.Equals(remote))
            {
                _logger.Debug("Not equals address.");

                var response = SIPResponse.GetResponse(request, SIPResponseStatusCodesEnum.Forbidden, null);
                LogResult(_transport.SendResponseAsync(response));
                return Task.CompletedTask;
            }

            //if (!_clients.TryGetValue(request.Header.From.FromURI.User, out var client))
            //{
            //    _logger.Debug("Not contains client.");

            //    var response = SIPResponse.GetResponse(request, SIPResponseStatusCodesEnum.NotFound, null);
            //    LogResult(_transport.SendResponseAsync(response));
            //    return Task.CompletedTask;
            //}

            var client = _clients.Values.FirstOrDefault();

            try
            {
                switch (request.Method)
                {
                    case SIPMethodsEnum.INVITE:
                        Enqueue(new InviteEvent(client, request));
                        break;

                    case SIPMethodsEnum.BYE:
                        Enqueue(new ByeEvent(client, request));
                        break;

                    case SIPMethodsEnum.MESSAGE:
                        _supervisor.Enqueue(new MessageEvent(client, request));
                        break;

                    default:
                        var response = SIPResponse.GetResponse(request, SIPResponseStatusCodesEnum.NotAcceptable, null);
                        LogResult(_transport.SendResponseAsync(response));
                        return Task.CompletedTask;
                }
            }
            catch (Exception ex)
            {
                taskSource.SetException(ex);
            }
            finally
            {
                if (!taskSource.Task.IsFaulted)
                    taskSource.SetResult(true);
            }

            return taskSource.Task;
        }

        private Task OnResponseReceived(SIPEndPoint local, SIPEndPoint remote, SIPResponse response)
        {
            return Task.CompletedTask;
        }


        private void CallFree(ISipCall call, bool hangup)
        {
            try
            {
                if (call.Session != null)
                    _rtpController.Release(call.Session);

                if (call.Settings == null)
                    return;

                if (!_dialogs.TryGetValue(call.Settings.DialogId, out SIPDialogue dialogue))
                    return;

                _calls.Remove(call.Settings.DialogId);
                _dialogs.Remove(call.Settings.DialogId);

                if (hangup)
                    dialogue.Hangup(_transport, _serverEp);
            }
            finally
            {
                call.Free();
            }
        }


        private int GetVersion()
        {
            return (int)(_timeline.TickCount / 1000);
        }

        public static string CalcDialogId(SIPMessageBase message, bool direct)
        {
            return new StringBuilder(256)
                .Append(message.Header.CallId)
                .Append('|')
                .Append(direct ? message.Header.From.FromTag : message.Header.To.ToTag)
                .Append('|')
                .Append(direct ? message.Header.To.ToTag : message.Header.From.FromTag)
                .ToString();
        }

        private string GetDialogId(SIPDialogue dialog)
        {
            return dialog.Direction == SIPCallDirection.In
                   ? $"{dialog.CallId}|{dialog.RemoteTag}|{dialog.LocalTag}"
                   : $"{dialog.CallId}|{dialog.LocalTag}|{dialog.RemoteTag}";
        }


        private void Join<T>(Task<T> task, TaskCompletionSource<T> taskSource)
        {
            task.ContinueWith(x =>
            {
                switch (x.Status)
                {
                    case TaskStatus.RanToCompletion:
                        taskSource.SetResult(x.Result);
                        break;

                    case TaskStatus.Faulted when x.Exception != null:
                        taskSource.SetException(x.Exception);
                        break;
                }
            });
        }

        private void LogResult(Task task)
        {
            task.ContinueWith(_ => _logger.Warn(task.Exception, $"Error happened: {task.Exception.Message}"), TaskContinuationOptions.OnlyOnFaulted);
        }

        private void Enqueue(BaseEvent @event)
        {
            _supervisor.Enqueue(@event);
        }

        private void SupervisorOnTick()
        {
            var currentTick = _timeline.TickCount;

            foreach (var client in _clients.Values)
            {
                client.Tick(currentTick);
            }
        }

        #endregion Methods
    }
}
