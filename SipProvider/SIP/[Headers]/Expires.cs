namespace SipProvider.SIP
{
    public class Expires : HeaderBase
    {
        #region Constants

        private const string HEADER_NAME = "Expires";

        #endregion Constants

        #region Properties

        public new int Value { get; private set; }

        #endregion Properties

        #region Constructors

        public Expires(int value) : base(HEADER_NAME, value.ToString())
        {
            Value = value;
        }

        #endregion Constructors

        #region Methods

        public override bool Unpack(string header)
        {
            if (!base.Unpack(header))
                return false;

            if (!int.TryParse(base.Value, out var value))
                return false;

            Value = value;
            return true;
        }

        #endregion Methods
    }
}
