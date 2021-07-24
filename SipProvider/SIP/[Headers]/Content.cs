namespace SipProvider.SIP
{
    using SipProvider.SDP;

    public class Content : HeaderBase
    {
        #region Constants

        private const string HEADER_NAME = "Content";

        #endregion Constants

        #region Constructors

        public Content(string value) : base() 
        {
            Name = HEADER_NAME;
            Value = value;
        }

        #endregion Constructors

        #region Methods

        public static Content ContentSDP(ISdpMessage sdp) => new Content(sdp.Pack());

        #endregion Methods
    }
}
