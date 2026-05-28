namespace Wyrm.ModBusClient.Connection;

internal record FunctionData(ushort TransactionId, byte UnitIdentifier, byte FunctionNumber, ReadOnlyMemory<byte> Data);
