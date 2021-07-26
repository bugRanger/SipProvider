namespace SipProvider.SIP
{
    using SipProvider.SDP;

    public class ContentType : HeaderBase
    {
        #region Constants

        private const string HEADER_NAME = "Content-Type";

        #endregion Constants

        #region Constructors

        public ContentType() : base(HEADER_NAME) { }

        public ContentType(string value) : this()
        {
            Value = value;
        }

        #endregion Constructors

        #region Methods

        public static ContentType ContentTypeSDP() => new ContentType(SdpMessage.MIME_CONTENTTYPE);

        public override void Handle(ISipHeaderVisitor builder)
        {
            builder.Handle(this);
        }

        #endregion Methods
    }
}
