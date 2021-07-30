﻿namespace SipProvider.SIP
{
    public enum SipResponseCode
    {
        None = 0,
        Trying = 100,
        Ringing = 180,
        CallIsBeingForwarded = 181,
        Queued = 182,
        SessionProgress = 183,
        Ok = 200,
        Accepted = 202,
        NoNotification = 204,
        MultipleChoices = 300,
        MovedPermanently = 301,
        MovedTemporarily = 302,
        UseProxy = 303,
        AlternativeService = 304,
        BadRequest = 400,
        Unauthorised = 401,
        PaymentRequired = 402,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        NotAcceptable = 406,
        ProxyAuthenticationRequired = 407,
        RequestTimeout = 408,
        Gone = 410,
        ConditionalRequestFailed = 412,
        RequestEntityTooLarge = 413,
        RequestURITooLong = 414,
        UnsupportedMediaType = 415,
        UnsupportedURIScheme = 416,
        UnknownResourcePriority = 417,
        BadExtension = 420,
        ExtensionRequired = 421,
        SessionIntervalTooSmall = 422,
        IntervalTooBrief = 423,
        UseIdentityHeader = 428,
        ProvideReferrerIdentity = 429,
        FlowFailed = 430,
        AnonymityDisallowed = 433,
        BadIdentityInfo = 436,
        UnsupportedCertificate = 437,
        InvalidIdentityHeader = 438,
        FirstHopLacksOutboundSupport = 439,
        MaxBreadthExceeded = 440,
        ConsentNeeded = 470,
        TemporarilyUnavailable = 480,
        CallLegTransactionDoesNotExist = 481,
        LoopDetected = 482,
        TooManyHops = 483,
        AddressIncomplete = 484,
        Ambiguous = 485,
        BusyHere = 486,
        RequestTerminated = 487,
        NotAcceptableHere = 488,
        BadEvent = 489,
        RequestPending = 491,
        Undecipherable = 493,
        InternalServerError = 500,
        NotImplemented = 501,
        BadGateway = 502,
        ServiceUnavailable = 503,
        ServerTimeout = 504,
        SIPVersionNotSupported = 505,
        MessageTooLarge = 513,
        SecurityAgreementRequired = 580,
        PreconditionFailure = 580,
        BusyEverywhere = 600,
        Decline = 603,
        DoesNotExistAnywhere = 604,
        NotAcceptableAnywhere = 606,
    }
}