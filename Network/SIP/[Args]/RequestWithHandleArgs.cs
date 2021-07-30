namespace Network.SIP
{
    using System.Net;

    public class RequestWithHandleArgs : RequestArgs
    {
        #region Properties

        public bool Handle { get; set; }

        #endregion Properties

        #region Constructors

        public RequestWithHandleArgs(IPEndPoint remote, ISipMessage message) 
            : base(remote, message) { }

        #endregion Constructors
    }
}
