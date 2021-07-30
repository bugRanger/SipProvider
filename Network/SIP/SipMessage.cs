namespace Network.SIP
{
    using System;
    using System.Collections.Generic;

    public class SipMessage : ISipMessage
    {
        #region Fields

        private readonly Dictionary<Type, ISipHeader> _extensions;
        private readonly List<string> _extensionsCache;

        #endregion Fields

        #region Properties

        public SipMethodKind Method { get; set; }

        public SipResponseCode ReasonCode { get; set; }

        public int Id { get; set; }

        public string CallId { get; set; }

        public SipUri From { get; set; }

        public SipUri To { get; set; }

        public int Expires { get; set; }

        public string UserAgent { get; set; }

        public int ContentLength { get; set; }

        public string ContentType { get; set; }

        public string Content { get; set; }

        public IReadOnlyCollection<ISipHeader> Extensions => _extensions.Values;

        #endregion Properties

        #region Constructors

        public SipMessage()
        {
            _extensions = new Dictionary<Type, ISipHeader>();
            _extensionsCache = new List<string>();
        }

        #endregion Constructors

        #region Methods

        public bool IsCall()
        {
            return 
                Method == SipMethodKind.INVITE ||
                Method == SipMethodKind.UPDATE ||
                Method == SipMethodKind.CANCEL ||
                Method == SipMethodKind.BYE;
        }

        public bool IsText()
        {
            return Method == SipMethodKind.MESSAGE;
        }

        public bool TryGetOrParse<T>(out T header) where T : ISipHeader, new()
        {
            if (_extensions.TryGetValue(typeof(T), out var findHeader))
            {
                header = (T)findHeader;
                return true;
            }

            header = new T();

            foreach (var unknownHeader in _extensionsCache)
            {
                if (header.Unpack(unknownHeader))
                {
                    _extensions.Add(typeof(T), header);
                    return true;
                }
            }

            return false;
        }

        public ISipMessage Append<T>(T header) where T : ISipHeader
        {
            _extensions.Add(typeof(T), header);
            return this;
        }

        public ISipMessage Append(params string[] headers)
        {
            _extensionsCache.AddRange(headers);
            return this;
        }

        #endregion Methods
    }
}
