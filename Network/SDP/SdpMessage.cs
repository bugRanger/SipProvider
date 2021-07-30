namespace Network.SDP
{
    using System.Net;
    using System.Collections.Generic;

    public class SdpMessage : ISdpMessage
    {
        #region Constants

        public const string MIME_CONTENTTYPE = "application/sdp";
        public const string DEFAULT_SESSION_NAME = "-";

        #endregion Constants

        #region Fields

        private readonly ISdpMessageHandler _handler;
        private readonly Dictionary<int, ISdpMediaFormat> _formats;
        private readonly List<ISdpMediaAttribute> _attributes;

        #endregion Fields

        #region Properties

        public string SessionId { get; set; }

        public string SessionName { get; set; }

        public string Username { get; set; }

        public int AnnouncementVersion { get; set; }

        public IPEndPoint Local { get; set; }

        public IReadOnlyCollection<ISdpMediaFormat> Formats => _formats.Values;

        public IReadOnlyCollection<ISdpMediaAttribute> Attributes => _attributes;

        #endregion Properties

        #region Constructors

        public SdpMessage(ISdpMessageHandler handler)
        {
            _handler = handler;
            _formats = new Dictionary<int, ISdpMediaFormat>();
            _attributes = new List<ISdpMediaAttribute>();
        }

        #endregion Constructors

        #region Methods

        public ISdpMessage Append(ISdpMediaFormat format)
        {
            _formats.Add(format.Id, format);
            return this;
        }

        public ISdpMessage Append(ISdpMediaAttribute attribute)
        {
            _attributes.Add(attribute);
            return this;
        }

        public void Unpack(string description)
        {
            _handler.Unpack(this, description);
        }

        public string Pack()
        {
            return _handler.Pack(this);
        }

        #endregion Methods
    }
}
