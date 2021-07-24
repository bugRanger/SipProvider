namespace SipProvider.SIP
{
    using System;

    using SIPSorcery.SIP;

    public abstract class HeaderBase : ISipHeader
    {
        #region Fields

        private static readonly string _delimiter;

        #endregion Fields

        #region Properties

        public string Name { get; protected set; }

        public string Value { get; protected set; }

        #endregion Properties

        #region Constructors

        static HeaderBase()
        {
            _delimiter = SIPConstants.HEADER_DELIMITER_CHAR + " ";
        }

        protected HeaderBase() { }

        protected HeaderBase(string name, string value) 
        {
            Name = name;
            Value = value;
        }

        #endregion Constructors

        #region Methods

        public string Pack()
        {
            return Name + _delimiter + Value;
        }

        public virtual bool Unpack(string header)
        {
            var tokens = header.Split(new[] { _delimiter }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length != 2)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Name) && Name != tokens[0])
            {
                return false;
            }

            Name = tokens[0];
            Value = tokens[1];

            return true;
        }

        #endregion Methods
    }
}
