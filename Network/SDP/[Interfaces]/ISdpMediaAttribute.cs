namespace Network.SDP
{
    public interface ISdpMediaAttribute
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
