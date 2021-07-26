namespace SipProvider.SIP.SIPSorcery
{
    using global::SIPSorcery.SIP;

    public class SipMessageParser : ISipHeaderVisitor
    {
        #region Fields

        private static readonly ISipHeader[] _includeHeaders;

        private readonly SipMessage _message;

        private SIPMessageBase _messageBase;

        #endregion Fields

        #region Constructors

        static SipMessageParser() 
        {
            _includeHeaders = new ISipHeader[]
            {
                new Expires(),
                new UserAgent(),
                new ContentLength(),
                new ContentType(),
                new Content(),
            };
        }

        public SipMessageParser()
        {
            _message = new SipMessage();
        }

        #endregion Constructors

        #region Methods

        public SipMessage Parse(SIPMessageBase messageBase)
        {
            _messageBase = messageBase;

            _message.Id = messageBase.Header.CSeq;
            _message.SourceId = messageBase.Header.From.FromURI.User;
            _message.TargetId = messageBase.Header.To.ToURI.User;
            //_message.Method;
            //_message.Status;

            _message.Append(messageBase.Header.UnknownHeaders.ToArray());

            foreach (var header in _includeHeaders)
            {
                header.Handle(this);
            }

            return _message;
        }

        public void Handle(Expires header)
        {
            _message.Append(new Expires(_messageBase.Header.Expires));
        }

        public void Handle(UserAgent header)
        {
            _message.Append(new UserAgent(_messageBase.Header.UserAgent));
        }

        public void Handle(ContentLength header)
        {
            _message.Append(new ContentLength(_messageBase.Header.ContentLength));
        }

        public void Handle(ContentType header)
        {
            _message.Append(new ContentType(_messageBase.Header.ContentType));
        }

        public void Handle(Content header)
        {
            _message.Append(new Content(_messageBase.Body));
        }

        #endregion Methods
    }
}
