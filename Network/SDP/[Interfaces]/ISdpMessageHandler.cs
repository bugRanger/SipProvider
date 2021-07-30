namespace Network.SDP
{
    public interface ISdpMessageHandler
    {
        #region Methods

        void Unpack(SdpMessage message, string description);

        string Pack(ISdpMessage message);

        #endregion Methods
    }
}
