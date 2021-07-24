namespace Network.RTP
{
    using System;

    public interface IRtpController
    {
        #region Methods

        IRtpSession Take();

        void Release(IRtpSession session);

        #endregion Methods
    }
}
