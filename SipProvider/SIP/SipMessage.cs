namespace SipProvider.SIP
{
    using System;
    using System.Collections.Generic;

    using SIPSorcery.SIP;

    class HeaderMapper 
    { 
        public Func<SIPMessageBase, ISipHeader> Import { get; set; }

        public Action<ISipHeader, SIPMessageBase> Export { get; set; }
    }

    public class SipMessage : ISipMessage
    {
        #region Fields

        private static readonly List<Func<SIPMessageBase, ISipHeader>> _importHeaders;
        private static readonly List<Action<ISipHeader, SIPMessageBase>> _exportHeaders;

        private readonly List<string> _unknownHeaders;
        private readonly List<ISipHeader> _knownHeaders;

        #endregion Fields

        #region Properties

        public int Id { get; set; }

        public string SourceId { get; set; }

        public string TargetId { get; set; }

        public IReadOnlyCollection<ISipHeader> Headers => _knownHeaders;

        #endregion Properties

        #region Constructors

        static SipMessage()
        {
            _includeHeaders = new List<HeaderProperty>(5)
            {
                { 
                    new HeaderProperty 
                    { 
                        Getter = (m) => new UserAgent(m.Header.UserAgent), 
                        Setter = (h, m) => m.Header.UserAgent = h.Value,
                    } 
                }
                //(m) => new Expires(m.Header.Expires),
                //(m) => new Content(m.Body),
                //(m) => new ContentType(m.Header.ContentType),
                //(m) => new ContentLength(m.Header.ContentLength),
            };
        }

        public SipMessage()
        {
            _knownHeaders = new List<ISipHeader>();
            _unknownHeaders = new List<string>();
        }

        #endregion Constructors

        #region Methods

        public ISipMessage Append(ISipHeader header)
        {
            _knownHeaders.Add(header);
            return this;
        }

        public bool TryGet<T>(out T header) where T : ISipHeader, new()
        {
            foreach (var knownHeader in _knownHeaders)
            {
                if (knownHeader is T item)
                {
                    header = item;
                    return true;
                }
            }

            header = new T();

            foreach (var unknownHeader in _unknownHeaders)
            {
                if (header.Unpack(unknownHeader))
                {
                    return true;
                }
            }

            return false;
        }

        //internal SipMessage Parse(SIPMessageBase messageBase)
        //{
        //    var message = new SipMessage
        //    {
        //        Id = messageBase.Header.CSeq,
        //        SourceId = messageBase.Header.From.FromURI.User,
        //        TargetId = messageBase.Header.To.ToURI.User,
        //    };

        //    _knownHeaders.AddRange(_includeHeaders.ConvertAll(getHeader => getHeader(messageBase)));
        //    _unknownHeaders.AddRange(messageBase.Header.UnknownHeaders);

        //    return message;
        //}

        //internal void CopyTo(SIPMessageBase messageBase)
        //{
        //}

        #endregion Methods
    }
}
