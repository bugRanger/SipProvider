namespace SipProvider.SIP
{
    /// <summary>
    /// Укороченный интерфейс данных.
    /// </summary>
    public class SipUri
    {
        #region Properties

        public string Name { get; } 

        public string User { get; } 

        public string Tag { get; }

        #endregion Properties

        #region Constructors

        public SipUri(string user, string tag) 
            : this(null, user, tag) { }

        public SipUri(string name, string user, string tag)
        {
            Name = name;
            User = user;
            Tag = tag;
        }

        #endregion Constructors
    }
}
