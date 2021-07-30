namespace Network.SDP
{
    public class SdpMediaFormat : ISdpMediaFormat
    {
        #region Properties

        public int Id { get; }

        public string Rtpmap { get; }

        #endregion Properties

        #region Constructors

        public SdpMediaFormat(int id, string rtpmap)
        {
            Id = id;
            Rtpmap = rtpmap;
        }

        #endregion Constructors
    }
}
