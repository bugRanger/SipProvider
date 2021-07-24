namespace SipProvider.SIP
{
    public interface ISipHeader
    {
        #region Properties

        string Name { get; }

        string Value { get; }

        #endregion Properties

        #region Methods

        string Pack();

        bool Unpack(string header);

        #endregion Methods
    }
}
