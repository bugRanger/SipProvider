namespace SipProvider.SIP
{
    public class UserAgent : HeaderBase
    {
        #region Constants

        private const string HEADER_NAME = "User-Agent";

        #endregion Constants

        #region Constructors

        public UserAgent() : base(HEADER_NAME) { }

        public UserAgent(string value) : base(HEADER_NAME, value)
        {
            Value = value;
        }

        #endregion Constructors

        #region Methods

        public override void Handle(ISipHeaderVisitor builder)
        {
            builder.Handle(this);
        }

        #endregion Methods
    }
}
