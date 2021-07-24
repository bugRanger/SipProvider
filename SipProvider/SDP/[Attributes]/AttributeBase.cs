namespace SipProvider.SDP
{
    public class AttributeBase : ISdpAttribute
    {
        #region Properties

        public string Name { get; }

        public string Value { get; }

        #endregion Properties

        #region Constructors

        public AttributeBase(string name, string value)
        {
            Name = name;
            Value = value;
        }

        #endregion Constructors

        #region Methods

        public string Pack() 
        {
            return Name + '=' + Value;
        }

        #endregion Methods
    }
}
