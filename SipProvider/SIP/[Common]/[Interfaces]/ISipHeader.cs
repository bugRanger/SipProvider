namespace SipProvider.SIP
{
    public interface ISipHeader
    {
        #region Properties

        string Name { get; }

        string Value { get; }

        #endregion Properties

        #region Methods

        bool Unpack(string header);

        string Pack();

        #endregion Methods
    }
}
