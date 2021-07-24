namespace Network.RTP
{
    using System;
    using System.Net;

    public interface IRtpSession
    {
        #region Properties

        uint Ssrc { get; }

        ushort Port { get; }

        bool Marker { get; set; }

        bool Enabled { get; set; }

        #endregion Properties

        #region Events

        event EventHandler<IRtpPacket> PacketReceived;

        #endregion Events

        #region Methods

        void Send(IPEndPoint remote, IRtpPacket packet);

        #endregion Methods
    }
}
