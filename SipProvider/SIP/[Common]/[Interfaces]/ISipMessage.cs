namespace SipProvider.SIP
{
    using System.Collections.Generic;

    public interface ISipMessage
    {
        #region Properties

        int Id { get; }

        string SourceId { get; }

        string TargetId { get; }

        //SipMethodKind Method { get; }

        //SipStatusCode Status { get; }

        IReadOnlyCollection<ISipHeader> Headers { get; }

        #endregion Properties

        #region Methods

        bool TryGet<T>(out T header) where T : ISipHeader, new();

        ISipMessage Append<T>(T header) where T : ISipHeader;

        #endregion Methods
    }
}