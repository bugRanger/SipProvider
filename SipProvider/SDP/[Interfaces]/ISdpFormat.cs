namespace SipProvider.SDP
{
    public interface ISdpFormat
    {
        #region Properties

        int Id { get; }

        string Rtpmap { get; }

        #endregion Properties
    }
}
