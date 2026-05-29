namespace Wyrm.ModBusClient;

/// <summary>
/// Data retrieved from the server asynchronously.
/// </summary>
public record UshortDataResponse
{
    /// <summary>
    /// The transaction id for the data.
    /// </summary>
    public ushort TransactionId { get; init; }
    /// <summary>
    /// The unit identifier supplying the response.
    /// </summary>
    public byte UnitIdentifier { get; init; }
    /// <summary>
    /// The function number illiciting the response.
    /// </summary>
    public byte FunctionNumber { get; init; }
    /// <summary>
    /// Ushort data values.
    /// </summary>
    public required ICollection<ushort> UshortData { get; init; }
}
