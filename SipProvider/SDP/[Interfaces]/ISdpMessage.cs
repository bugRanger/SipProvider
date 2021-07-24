namespace SipProvider.SDP
{
    using System.Net;
    using System.Collections.Generic;

    public interface ISdpMessage
    {
        #region Properties

        int SessionId { get; }

        string Username { get; }

        IPEndPoint Local { get; }

        IReadOnlyDictionary<int, string> Formats { get; }

        IReadOnlyCollection<string> Attributes { get; }

        #endregion Properties

        #region Methods

        ISdpMessage Append(ISdpAttribute attribute);

        ISdpMessage Append(ISdpFormat format);

        string Pack();

        #endregion Methods
    }
}
