namespace SipProvider.SIP
{
    public class UserAgent : HeaderBase
    {
        #region Constants

        private const string HEADER_NAME = "User-Agent";

        #endregion Constants

        #region Constructors

        public UserAgent(string value) : base()
        {
            Name = HEADER_NAME;
            Value = value;
        }

        #endregion Constructors
    }
}
