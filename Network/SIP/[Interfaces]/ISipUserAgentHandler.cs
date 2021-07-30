namespace Network.SIP
{
    public interface ISipUserAgentHandler
    {
        #region Methods

        SipResponseCode IncomingRequest(SipUserAgent sipUserAgent, RequestArgs args);

        SipResponseCode IncomingMessage(SipUserAgent sipUserAgent, RequestArgs args);

        SipResponseCode IncomingCall(SipUserAgent sipUserAgent, RequestArgs args);

        #endregion Methods
    }
}
