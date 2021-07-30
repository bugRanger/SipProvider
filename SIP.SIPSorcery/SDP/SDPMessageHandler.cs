namespace SIP.SIPSorcery
{
    using System.Linq;
    using System.Collections.Generic;

    using Network.SDP;

    using global::SIPSorcery.Net;

    public class SDPMessageHandler : ISdpMessageHandler
    {
        #region Methods

        public void Unpack(SdpMessage message, string description)
        {
            var sdp = SDP.ParseSDPDescription(description);

            message.Username = sdp.Username;
            message.SessionId = sdp.SessionId;
            message.SessionName = sdp.SessionName;
            message.AnnouncementVersion = sdp.AnnouncementVersion;

            if (sdp.Media.Count > 0)
            {
                foreach (var format in sdp.Media.SelectMany(s => s.MediaFormats.Values))
                {
                    message.Append(new SdpMediaFormat(format.ID, format.Rtpmap));
                }
            }

            if (sdp.ExtraSessionAttributes.Count > 0)
            {
                foreach (var attribute in sdp.ExtraSessionAttributes)
                {
                    if (SdpMediaAttribute.TryUnpack(attribute, out var mediaAttribute))
                        message.Append(mediaAttribute);
                }
            }
        }

        public string Pack(ISdpMessage message)
        {
            var sdp = new SDP(message.Local.Address)
            {
                Username = message.Username,
                SessionId = message.SessionId,
                SessionName = SdpMessage.DEFAULT_SESSION_NAME,
                AnnouncementVersion = message.AnnouncementVersion,
                Connection = new SDPConnectionInformation(message.Local.Address),
            };

            if (message.Formats.Count > 0)
            {
                var mediaFormats = new List<SDPAudioVideoMediaFormat>(message.Formats.Count);
                foreach (var format in message.Formats)
                {
                    if (!SDPAudioVideoMediaFormat.TryParseRtpmap(format.Rtpmap, out _, out _, out _))
                        continue;

                    mediaFormats.Add(new SDPAudioVideoMediaFormat(SDPMediaTypesEnum.audio, format.Id, format.Rtpmap));
                }

                sdp.Media.Add(new SDPMediaAnnouncement(SDPMediaTypesEnum.audio, message.Local.Port, mediaFormats));
            }

            if (message.Attributes.Count > 0)
            {
                foreach (var attribute in message.Attributes)
                {
                    sdp.AddExtra(attribute.Pack());
                }
            }

            return sdp.RawString();
        }

        #endregion Methods
    }
}
