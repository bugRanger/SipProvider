namespace SipProvider.SIP
{
    public interface ISipHeaderVisitor
    {
        #region Methods

        void Handle(Expires header);

        void Handle(UserAgent header);

        void Handle(ContentLength header);

        void Handle(ContentType header);

        void Handle(Content header);

        #endregion Methods
    }
}
