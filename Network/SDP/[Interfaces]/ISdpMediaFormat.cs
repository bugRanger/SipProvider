namespace Network.SDP
{
    public interface ISdpMediaFormat
    {
        #region Properties

        int Id { get; }

        string Rtpmap { get; }

        #endregion Properties
    }
}
