namespace VehicleFinder.Api.Exceptions;

public enum NhtsaFailureReason
{
    Unavailable,
    Timeout,
    InvalidResponse,
}

/// <summary>Thrown when the NHTSA client cannot fulfil a request. Carries a message that is safe to show to end users.</summary>
public class NhtsaApiException : Exception
{
    public NhtsaFailureReason Reason { get; }

    public NhtsaApiException(string message, NhtsaFailureReason reason, Exception? inner = null)
        : base(message, inner)
    {
        Reason = reason;
    }
}
