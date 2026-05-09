namespace Wyrm.ModBusClient;

/// <summary>
/// Represents errors that occur during ModBus calls.
/// </summary>
public class ModBusClientException : Exception
{
    internal ModBusClientException(string? message, ModBusExceptionCode exceptionCode, Exception? innerException = null) :
        base(message, innerException)
    {
        ExceptionCode = exceptionCode;
    }

    /// <summary>
    /// Gets the <see cref="ModBusExceptionCode"/> for the exception.
    /// </summary>
    public ModBusExceptionCode ExceptionCode { get; }
}
