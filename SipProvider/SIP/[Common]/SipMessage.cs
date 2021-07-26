namespace SipProvider.SIP
{
    using System;
    using System.Collections.Generic;

    public class SipMessage : ISipMessage
    {
        #region Fields

        private readonly List<string> _unknownHeaders;
        private readonly Dictionary<Type, ISipHeader> _knownHeaders;

        #endregion Fields

        #region Properties

        public int Id { get; set; }

        public string SourceId { get; set; }

        public string TargetId { get; set; }

        public SipMethodKind Method { get; set; }

        public SipStatusCode Status { get; set; }

        public IReadOnlyCollection<ISipHeader> Headers => _knownHeaders.Values;

        #endregion Properties

        #region Constructors

        public SipMessage()
        {
            _knownHeaders = new Dictionary<Type, ISipHeader>();
            _unknownHeaders = new List<string>();
        }

        #endregion Constructors

        #region Methods

        public ISipMessage Append(string header)
        {
            _unknownHeaders.Add(header);
            return this;
        }

        public ISipMessage Append(params string[] headers)
        {
            _unknownHeaders.AddRange(headers);
            return this;
        }

        public ISipMessage Append<T>(T header) where T : ISipHeader
        {
            _knownHeaders.Add(typeof(T), header);
            return this;
        }

        public bool TryGet<T>(out T header) where T : ISipHeader, new()
        {
            if (_knownHeaders.TryGetValue(typeof(T), out var knownHeader))
            {
                header = (T)knownHeader;
                return true;
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

        #endregion Methods
    }
}
