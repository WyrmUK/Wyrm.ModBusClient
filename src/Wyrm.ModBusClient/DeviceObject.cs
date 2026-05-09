namespace Wyrm.ModBusClient;

/// <summary>
/// A value from a Read Device Identification command.
/// </summary>
/// <param name="Id">The Id of the value.</param>
/// <param name="Value">The value read.</param>
public record DeviceObject(byte Id, ICollection<byte> Value);
