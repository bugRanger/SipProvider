namespace SipProvider.SIP
{
    using SipProvider.SDP;

    public class Content : HeaderBase
    {
        #region Constants

        private const string HEADER_NAME = "Content";

        #endregion Constants

        #region Constructors

        public Content() : base(HEADER_NAME) { }

        public Content(string value) : this()
        {
            Value = value;
        }

        #endregion Constructors

        #region Methods

        public static Content ContentSDP(ISdpMessage sdp) => new Content(sdp.Pack());

        public override void Handle(ISipHeaderVisitor builder)
        {
            builder.Handle(this);
        }

        #endregion Methods
    }
}
