namespace SipProvider.SDP
{
    public interface ISdpAttribute
    {
        #region Properties

        string Name { get; }

        string Value { get; }

        #endregion Properties

        #region Methods

        string Pack();

        #endregion Methods
    }
}
