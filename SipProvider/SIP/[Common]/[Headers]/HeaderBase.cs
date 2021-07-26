namespace SipProvider.SIP
{
    using System;

    public abstract class HeaderBase : ISipHeader
    {
        #region Constants

        public const string HEADER_DELIMITER_CHAR = ": ";

        #endregion Constants

        #region Properties

        public string Name { get; private set; }

        public string Value { get; protected set; }

        #endregion Properties

        #region Constructors

        protected HeaderBase(string name) 
        {
            Name = name;
        }

        protected HeaderBase(string name, string value) : this(name)
        {
            Value = value;
        }

        #endregion Constructors

        #region Methods

        public abstract void Handle(ISipHeaderVisitor builder);

        public string Pack()
        {
            return Name + HEADER_DELIMITER_CHAR + Value;
        }

        public virtual bool Unpack(string header)
        {
            var tokens = header.Split(new[] { HEADER_DELIMITER_CHAR }, 2, StringSplitOptions.RemoveEmptyEntries);

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
