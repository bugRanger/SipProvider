using System;

namespace Network.SIP
{
    public interface ISipUserAgentHandler
    {
        #region Methods

        SipResponseCode IncomingCall(ISipUserAgent userAgent, RequestArgs args);

        SipResponseCode IncomingMessage(ISipUserAgent userAgent, RequestArgs args);

        ISipMessage OutcomingCall(ISipUserAgent userAgent, Action<ISipMessage> prepare);

        ISipMessage OutcomingMessage(ISipUserAgent userAgent, Action<ISipMessage> prepare);

        #endregion Methods
    }
}
