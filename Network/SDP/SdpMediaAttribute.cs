namespace Network.SDP
{
    using System;

    public class SdpMediaAttribute : ISdpMediaAttribute
    {
        #region Constants

        private const string ATTRIBUTE_DELIMITER = "=";

        #endregion Constants

        #region Properties

        public string Name { get; private set; }

        public string Value { get; private set; }

        #endregion Properties

        #region Constructors

        public SdpMediaAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        #endregion Constructors

        #region Methods

        public string Pack()
        {
            return Name + ATTRIBUTE_DELIMITER + Value;
        }

        public static bool TryUnpack(string attribute, out ISdpMediaAttribute mediaAttribute)
        {
            mediaAttribute = null;
            var tokens = attribute.Split(new[] { ATTRIBUTE_DELIMITER }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != 2)
            {
                return false;
            }

            mediaAttribute = new SdpMediaAttribute(name: tokens[0], 
                                                   value: tokens[1]);

            return true;
        }

        #endregion Methods
    }
}
