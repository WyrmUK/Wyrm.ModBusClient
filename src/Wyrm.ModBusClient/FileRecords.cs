namespace Wyrm.ModBusClient;

/// <summary>
/// Specifies a range of records for a file.
/// </summary>
/// <param name="FileNumber">The number of the file from 1 to 65535 (greater than 10 may cause issues with legacy equipment).</param>
/// <param name="StartingRecordAddress">The 0-based address of the record (register) to start from (0 to 9999).</param>
/// <param name="RecordLength">The number of records (registers).</param>
public record FileRecords(ushort FileNumber, ushort StartingRecordAddress, ushort RecordLength);
