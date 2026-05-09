namespace Wyrm.ModBusClient;

/// <summary>
/// Ids for the Read Device Identifier command.
/// </summary>
public enum ReadDeviceIdentifier : byte
{
    /// <summary>
    /// Read the basic device identification stream.
    /// </summary>
    Basic = 1,
    /// <summary>
    /// Read the standard device identification stream.
    /// </summary>
    Regular = 2,
    /// <summary>
    /// Read the extended device identification stream.
    /// </summary>
    Extended = 3,
    /// <summary>
    /// Read a single device identifier.
    /// </summary>
    Single = 4
}
