namespace Network.SIP
{
    using System;
    using System.Net;

    public class RequestArgs : EventArgs
    {
        #region Properties

        public IPEndPoint Remote { get; }

        public ISipMessage Message { get; }

        #endregion Properties

        #region Constructors

        public RequestArgs(IPEndPoint remote, ISipMessage message)
        {
            Remote = remote;
            Message = message;
        }

        #endregion Constructors
    }
}
