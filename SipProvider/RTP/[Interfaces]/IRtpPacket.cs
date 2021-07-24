// ---------------------------------------------------------------------------------------------------------------------------------------------------
// Copyright ElcomPlus LLC. All rights reserved.
// Author: Alexander Pushkin
// ---------------------------------------------------------------------------------------------------------------------------------------------------

namespace Network.RTP
{
    public interface IRtpPacket : IUdpPacket
    {
        #region Properties

        int Version { get; set; }
        bool Padding { get; set; }
        bool Extension { get; set; }
        int CsrcCount { get; }
        bool Marker { get; set; }
        int PayloadType { get; set; }
        ushort SequenceNumber { get; set; }
        uint Timestamp { get; set; }
        uint Ssrc { get; set; }
        uint[] Csrc { get; set; }
        ushort ExtensionType { get; set; }
        byte[] ExtensionData { get; set; }
        byte[] Payload { get; set; }

        int HeaderLength { get; }
        int PayloadLength { get; }

        #endregion

        #region Methods

        int Parse(byte[] buffer, int offset, int length);

        IRtpPacket Clone();

        //IPacket Build(bool pooledMemory);

        void UpdatePayload(byte[] buffer);

        void UpdateMarker(bool marker);

        void UpdateSequenceNumber(ushort sequenceNumber);

        #endregion
    }
}
