namespace SipProvider.SIP
{
    using SipProvider.SDP;

    public class ContentType : HeaderBase
    {
        #region Constants

        private const string HEADER_NAME = "Content-Type";

        #endregion Constants

        #region Constructors

        public ContentType(string value) : base()
        {
            Name = HEADER_NAME;
            Value = value;
        }

        #endregion Constructors

        #region Methods

        public static ContentType ContentTypeSDP() => new ContentType(SdpMessage.MIME_CONTENTTYPE);

        #endregion Methods
    }
}
