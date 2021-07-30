namespace SIP.SIPSorcery
{
    using System.Linq;

    using Common;

    using Network.SIP;

    using global::SIPSorcery.SIP;

    public static class SIPMessageConverter
    {
        #region Fields

        private static readonly EnumerationMapper<SipMethodKind, SIPMethodsEnum> _methodMapper;

        #endregion Fields

        #region Methods

        static SIPMessageConverter() 
        {
            _methodMapper = new EnumerationMapper<SipMethodKind, SIPMethodsEnum>
            {
                { SipMethodKind.UNKNOWN, SIPMethodsEnum.UNKNOWN },
                { SipMethodKind.REGISTER, SIPMethodsEnum.REGISTER },
                { SipMethodKind.MESSAGE, SIPMethodsEnum.MESSAGE },
                { SipMethodKind.INVITE, SIPMethodsEnum.INVITE },
                { SipMethodKind.UPDATE, SIPMethodsEnum.UPDATE },
                { SipMethodKind.CANCEL, SIPMethodsEnum.CANCEL },
                { SipMethodKind.BYE, SIPMethodsEnum.BYE },
            };
        }

        public static SIPRequest ToRequest(this ISipMessage message, ISipUserAgent agent)
        {
            var request = SIPRequest.GetRequest(SIPMethodsEnum.UNKNOWN, null);

            var localUri = new SIPURI(SIPSchemesEnum.sip, agent.LocalAddress, agent.LocalPort);
            var remoteUri = new SIPURI(SIPSchemesEnum.sip, agent.Remote.Address, agent.Remote.Port);

            request.URI = remoteUri;
            request.Method = _methodMapper[message.Method];
            
            request.Header.CSeq = message.Id;
            request.Header.CSeqMethod = request.Method;
            request.Header.CallId = message.CallId;
            request.Header.Expires = message.Expires;
            request.Header.UserAgent = message.UserAgent;
            request.Header.From = new SIPFromHeader(message.From.Name, new SIPURI(message.From.User, remoteUri.Host, null, SIPSchemesEnum.sip), message.From.Tag);
            request.Header.To = new SIPToHeader(message.To.Name, new SIPURI(message.To.User, remoteUri.Host, null, SIPSchemesEnum.sip), message.To.Tag);
            request.Header.Contact.Add(new SIPContactHeader(null, new SIPURI(agent.Id, localUri.Host, null, localUri.Scheme)));
            request.Header.UnknownHeaders.AddRange(message.Extensions.Select(header => header.Pack()));
            request.Header.ContentLength = message.ContentLength;
            request.Header.ContentType = message.ContentType;
            request.Body = message.Content;

            return request;
        }

        public static SIPResponse ToResponse(this ISipMessage message, ISipUserAgent agent, SipResponseCode code)
        {
            return SIPResponse.GetResponse(message.ToRequest(agent), (SIPResponseStatusCodesEnum)code, null);
        }

        public static ISipMessage ToMessage(this SIPRequest request)
        {
            return request.ToMessageEx();
        }

        public static ISipMessage ToMessage(this SIPResponse response)
        {
            var message = response.ToMessageEx();
            message.ReasonCode = (SipResponseCode)response.Status;
            return message;
        }

        private static SipMessage ToMessageEx(this SIPMessageBase messageBase)
        {
            var message = new SipMessage
            {
                Id = messageBase.Header.CSeq,
                CallId = messageBase.Header.CallId,
                Expires = messageBase.Header.Expires,
                UserAgent = messageBase.Header.UserAgent,
                Method = _methodMapper[messageBase.Header.CSeqMethod],
                From = new SipUri(messageBase.Header.From.FromName, messageBase.Header.From.FromURI.User, messageBase.Header.From.FromTag),
                To = new SipUri(messageBase.Header.To.ToName, messageBase.Header.To.ToURI.User, messageBase.Header.To.ToTag),
                ContentLength = messageBase.Header.ContentLength,
                ContentType = messageBase.Header.ContentType,
                Content = messageBase.Body,
            };

            message.Append(messageBase.Header.UnknownHeaders.ToArray());

            return message;
        }

        #endregion Methods
    }
}