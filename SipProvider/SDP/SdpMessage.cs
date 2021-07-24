namespace SipProvider.SDP
{
    using System.Net;
    using System.Collections.Generic;

    using SIPSorcery.Net;

    public class SdpMessage : ISdpMessage
    {
        #region Constants

        public const string MIME_CONTENTTYPE = "application/sdp";

        private const string DEFAULT_SESSION_NAME = "-";

        #endregion Constants

        #region Fields

        private readonly Dictionary<int, string> _formats;
        private readonly List<string> _attributes;

        #endregion Fields

        #region Properties

        public int SessionId { get; set; }

        public string Username { get; set; }

        public IPEndPoint Local { get; set; }

        public IReadOnlyDictionary<int, string> Formats => _formats;

        public IReadOnlyCollection<string> Attributes => _attributes;

        #endregion Properties

        #region Constructors

        public SdpMessage()
        {
            _formats = new Dictionary<int, string>();
            _attributes = new List<string>();
        }

        #endregion Constructors

        #region Methods

        public ISdpMessage Append(ISdpFormat format)
        {
            _formats.Add(format.Id, format.Rtpmap);
            return this;
        }

        public ISdpMessage Append(ISdpAttribute attribute)
        {
            _attributes.Add(attribute.Pack());
            return this;
        }

        public string Pack()
        {
            var offer = CreateSDP(SessionId, Username, Local, Formats);

            if (Attributes.Count > 0)
            {
                foreach (var attribute in Attributes)
                {
                    offer.AddExtra(attribute);
                }
            }

            return offer.RawString();
        }

        internal SDP CreateSDP(int sessionId, string username, IPEndPoint local, IReadOnlyDictionary<int, string> formats)
        {
            var offer = new SDP(local.Address)
            {
                Username = username,
                AnnouncementVersion = sessionId,
                SessionId = sessionId.ToString(),
                SessionName = DEFAULT_SESSION_NAME,
                Connection = new SDPConnectionInformation(local.Address),
            };

            if (formats.Count > 0)
            {
                var mediaFormats = new List<SDPAudioVideoMediaFormat>(formats.Count);
                foreach (var format in formats)
                {
                    if (!SDPAudioVideoMediaFormat.TryParseRtpmap(format.Value, out _, out _, out _))
                        continue;

                    mediaFormats.Add(new SDPAudioVideoMediaFormat(SDPMediaTypesEnum.audio, format.Key, format.Value));
                }

                offer.Media.Add(new SDPMediaAnnouncement(SDPMediaTypesEnum.audio, local.Port, mediaFormats));
            }

            return offer;
        }

        #endregion Methods
    }
}
