using System.Net;

namespace Wyrm.ModBusClient.Connection;

internal interface IModBusConnection : IDisposable
{
    ushort ProtocolIdentifier { get; set; }
    byte UnitIdentifier { get; set; }
    ushort TransactionId { get; set; }
    ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken);
    ValueTask<ReadOnlyMemory<byte>> PerformFunctionAsync(byte functionNumber, ushort[] parameters, byte[] values, CancellationToken cancellationToken);
    void Close();
}
