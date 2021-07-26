namespace SipProvider.SIP.SIPSorcery
{
    using global::SIPSorcery.SIP;

    public class SipMessageBuilder : ISipHeaderVisitor
    {
        #region Fields

        private readonly SIPRequest _request;

        #endregion Fields

        #region Constructors

        public SipMessageBuilder()
        {
            _request = SIPRequest.GetRequest(SIPMethodsEnum.UNKNOWN, null);
        }

        #endregion Constructors

        #region Methods

        public SIPRequest Build(ISipUserAgent agent, SIPMethodsEnum method, ISipMessage message)
        {
            var localUri = new SIPURI(SIPSchemesEnum.sip, agent.LocalAddress, agent.LocalPort);
            var remoteUri = new SIPURI(SIPSchemesEnum.sip, agent.Remote.Address, agent.Remote.Port);

            _request.URI = remoteUri;
            _request.Method = method;

            _request.Header.From = new SIPFromHeader(null, new SIPURI(message.SourceId, remoteUri.Host, null, SIPSchemesEnum.sip), CallProperties.CreateNewTag());
            _request.Header.To = new SIPToHeader(null, new SIPURI(message.TargetId, remoteUri.Host, null, SIPSchemesEnum.sip), null);
            _request.Header.CSeq = message.Id;
            _request.Header.CSeqMethod = method;

            _request.Header.Contact.Add(new SIPContactHeader(null, new SIPURI(agent.Id, localUri.Host, null, localUri.Scheme)));

            if (message.UnknownHeaders.Count > 0)
            {
                _request.Header.UnknownHeaders.AddRange(message.UnknownHeaders);
            }

            if (message.Headers.Count > 0)
            {
                foreach (var item in message.Headers)
                {
                    item.Handle(this);
                }
            }

            return _request;
        }

        public void Handle(Expires header)
        {
            _request.Header.Expires = header.Value;
        }

        public void Handle(UserAgent header)
        {
            _request.Header.UserAgent = header.Value;
        }

        public void Handle(ContentLength header)
        {
            _request.Header.ContentLength = header.Value;
        }

        public void Handle(ContentType header)
        {
            _request.Header.ContentType = header.Value;
        }

        public void Handle(Content header)
        {
            _request.Body = header.Value;
        }

        #endregion Methods
    }
}
