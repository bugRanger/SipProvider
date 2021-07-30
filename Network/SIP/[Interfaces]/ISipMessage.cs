namespace Network.SIP
{
    using System.Collections.Generic;

    public interface ISipMessage
    {
        #region Properties

        SipMethodKind Method { get; }

        SipResponseCode ReasonCode { get; }

        int Id { get; }

        string CallId { get; }

        SipUri From { get; }

        SipUri To { get; }

        int Expires { get; }

        string UserAgent { get; }

        int ContentLength { get; }

        string ContentType { get; }

        string Content { get; }

        IReadOnlyCollection<ISipHeader> Extensions { get; }

        #endregion Properties

        #region Methods

        bool IsCall();

        bool IsText();

        bool TryGetOrParse<T>(out T header) where T : ISipHeader, new();

        ISipMessage Append<T>(T header) where T : ISipHeader;

        #endregion Methods
    }
}