namespace SIP.Example
{
    using System.Net;

    using Network.SIP;

    using SIP.SIPSorcery;
    using SIP.SIPSorcery.SDP;

    class Program
    {
        static void Main(string[] args)
        {
            //var id = 1;
            //var agent = new SipUserAgent(null, new SIPTransportProvider(new IPEndPoint(IPAddress.Any, 5060)));

            //var offer = new SdpMessage
            //{
            //    SessionId = 1,
            //    Username = "1",
            //    Local = new IPEndPoint(IPAddress.Loopback, 40000),
            //}
            //.Append()
            //.GetRawString();

            //var message = new SipMessage()
            //{
            //    Id = id++,
            //    From = new SipUri("1", null),
            //    To = new SipUri("2", null),

            //    ContentType = SdpMessage.MIME_CONTENTTYPE,
            //    ContentLength = offer.Length,
            //    Content = offer,
            //};


            //var response = agent.RequestAsync(message)
            //    .ConfigureAwait(false)
            //    .GetAwaiter()
            //    .GetResult();
        }
    }
}
