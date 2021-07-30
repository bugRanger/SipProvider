namespace Network.SDP
{
    using System.Net;
    using System.Collections.Generic;

    public interface ISdpMessage
    {
        #region Properties

        string SessionId { get; }

        string SessionName { get; }

        string Username { get; }
        
        int AnnouncementVersion { get; }

        IPEndPoint Local { get; }

        IReadOnlyCollection<ISdpMediaFormat> Formats { get; }

        IReadOnlyCollection<ISdpMediaAttribute> Attributes { get; }

        #endregion Properties

        #region Methods

        ISdpMessage Append(ISdpMediaAttribute attribute);

        ISdpMessage Append(ISdpMediaFormat format);

        void Unpack(string description);

        string Pack();

        #endregion Methods
    }
}
