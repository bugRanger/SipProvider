namespace SIP.SIPSorcery
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    using NLog;

    using Network.SIP;
    using Network.SDP;

    using global::SIPSorcery.SIP;

    public class SIPTransportProvider : ISipTransportProvider, IDisposable
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly IPEndPoint _listenInterface;
        private readonly SDPMessageHandler _handlerSDP;
        private readonly SIPTransport _transport;
        private readonly SIPChannelUDP _channel;

        #endregion Fields

        #region Properties

        public int Port => _listenInterface.Port;

        #endregion Properties

        #region Events

        public event Action<RequestWithHandleArgs> RequestReceived;

        #endregion Events

        #region Constructors

        public SIPTransportProvider(IPEndPoint listenInterface)
        {
            //Parent = parent;
            //ObjectId = SystemIdHelper.Safe(listenInterface.ToString());
            //_logger = LogManager.GetLogger(SystemIdHelper.GetSystemId(this));
            _logger = LogManager.GetCurrentClassLogger();
            _listenInterface = listenInterface;

            _handlerSDP = new SDPMessageHandler();

            _transport = new SIPTransport();
            _transport.SIPTransportRequestReceived += OnRequestReceived;
            _transport.SIPTransportResponseReceived += OnResponseReceived;

            _channel = new SIPChannelUDP(_listenInterface);
            _channel.Connected += OnConnected;
            _channel.Disconnected += OnDisconnected;
            _channel.Start();
        }

        public void Dispose()
        {
            _channel.Stop();
            _channel.Connected -= OnConnected;
            _channel.Disconnected -= OnDisconnected;
            _channel.Dispose();

            _transport.SIPTransportRequestReceived -= OnRequestReceived;
            _transport.SIPTransportResponseReceived -= OnResponseReceived;
            _transport.Dispose();
        }

        #endregion Constructors

        #region Methods

        public Task<ISipMessage> RequestAsync(ISipUserAgent agent, ISipMessage message)
        {
            var taskSource = new TaskCompletionSource<ISipMessage>();
            var request = message.ToRequest(agent);

            if (message.IsCall())
            {
                Join(SendInviteRequest(agent, request), taskSource);
            }
            else
            {
                Join(SendNonInviteRequest(agent, request), taskSource);
            }

            return taskSource.Task;
        }

        public Task<SocketError> ResponseAsync(ISipUserAgent agent, ISipMessage message, SipResponseCode code)
        {
            return _transport.SendResponseAsync(message.ToResponse(agent, code));
        }

        public SdpMessage CreateSdp() 
        {
            return new SdpMessage(_handlerSDP);
        }

        //public void CreateSdp(ISipMessage message, Action<SdpMessage> builder)
        //{
        //    var sdp = new SdpMessage(_handlerSDP);
        //    builder?.Invoke(sdp);
        //    return ;
        //}

        private Task<SIPNonInviteTransaction> SendNonInviteRequest(ISipUserAgent agent, SIPRequest request)
        {
            var task = new TaskCompletionSource<SIPNonInviteTransaction>();

            var nonInviteTransaction = new SIPNonInviteTransaction(_transport, request, new SIPEndPoint(agent.Remote));

            nonInviteTransaction.NonInviteTransactionFinalResponseReceived += (localEp, remoteEp, transaction, response) =>
            {
                _logger.Debug(() => "Received response: " + response);

                if (response.Status == SIPResponseStatusCodesEnum.Ok)
                {
                    _logger.Debug(() => $"Succeed: {localEp} {remoteEp} {transaction} {response}");
                    task.SetResult(nonInviteTransaction);
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
                        digest.SetCredentials(agent.Login, agent.Password, request.URI.ToString(), request.Method.ToString());
                        requestWithAuth.Header.AuthenticationHeader = new SIPAuthenticationHeader(digest)
                        {
                            SIPDigest = { Response = digest.Digest }
                        };

                        Join(SendNonInviteRequest(agent, requestWithAuth), task);
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
                    task.SetResult(nonInviteTransaction);
                    _logger.Error(() => $"Something bad happen: {localEp} {remoteEp} {transaction} {response}");
                }

                return Task.FromResult(SocketError.Success);
            };

            nonInviteTransaction.NonInviteTransactionFailed += (transaction, reason) =>
            {
                var message = $"Failed: {transaction} {reason}";
                _logger.Error(message);
                task.SetException(new Exception(message));
            };

            _logger.Debug(() => "Send request: " + request);

            nonInviteTransaction.SendRequest();
            return task.Task;
        }

        // TODO Add cancellationToken
        // Don't try to combine it with SendNonInviteRequest, I already tried and failed
        private Task<UACInviteTransaction> SendInviteRequest(ISipUserAgent agent, SIPRequest request)
        {
            var task = new TaskCompletionSource<UACInviteTransaction>();

            var inviteTransaction = new UACInviteTransaction(_transport, request, new SIPEndPoint(agent.Remote));

            request.Header.UserAgent = agent.Username;

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
                        digest.SetCredentials(agent.Login, agent.Password, request.URI.ToString(), request.Method.ToString());
                        requestWithAuth.Header.AuthenticationHeader = new SIPAuthenticationHeader(digest)
                        {
                            SIPDigest = { Response = digest.Digest }
                        };

                        Join(SendInviteRequest(agent, requestWithAuth), task);
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

        private void LogResult(Task task)
        {
            task.ContinueWith(_ => _logger.Warn(task.Exception, $"Error happened: {task.Exception.Message}"), TaskContinuationOptions.OnlyOnFaulted);
        }

        private void Join<T>(Task<T> task, TaskCompletionSource<T> taskSource) where T : SIPTransaction
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

        private void Join<T>(Task<T> task, TaskCompletionSource<ISipMessage> taskSource) where T : SIPTransaction
        {
            task.ContinueWith(x =>
            {
                switch (x.Status)
                {
                    case TaskStatus.RanToCompletion:
                        taskSource.SetResult(x.Result.TransactionFinalResponse.ToMessage());
                        break;

                    case TaskStatus.Faulted when x.Exception != null:
                        taskSource.SetException(x.Exception);
                        break;
                }
            });
        }

        private void OnConnected(object sender, EventArgs e)
        {
            _transport.AddSIPChannel(_channel);
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            _transport.RemoveSIPChannel(_channel);
        }

        private Task OnRequestReceived(SIPEndPoint local, SIPEndPoint remote, SIPRequest request)
        {
            var args = new RequestWithHandleArgs(remote.GetIPEndPoint(), request.ToMessage());

            RequestReceived?.Invoke(args);

            if (!args.Handle)
            {
                var response = SIPResponse.GetResponse(request, SIPResponseStatusCodesEnum.Forbidden, null);
                LogResult(_transport.SendResponseAsync(response));
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        private Task OnResponseReceived(SIPEndPoint local, SIPEndPoint remote, SIPResponse response)
        {
            return Task.CompletedTask;
        }

        #endregion Methods
    }
}
