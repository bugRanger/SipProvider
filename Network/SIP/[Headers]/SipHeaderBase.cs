namespace Network.SIP
{
    using System;

    public abstract class SIPHeaderBase : ISipHeader
    {
        #region Constants

        public const string HEADER_DELIMITER = ": ";

        #endregion Constants

        #region Properties

        public string Name { get; protected set; }

        public string Value { get; protected set; }

        #endregion Properties

        #region Constructors

        protected SIPHeaderBase() { }

        #endregion Constructors

        #region Methods

        public string Pack()
        {
            return Name + HEADER_DELIMITER + Value;
        }

        public virtual bool Unpack(string header)
        {
            var tokens = header.Split(new[] { HEADER_DELIMITER }, 2, StringSplitOptions.RemoveEmptyEntries);

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
