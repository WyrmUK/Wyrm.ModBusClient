using System.Net;

namespace Wyrm.ModBusClient.Connection;

internal interface IModBusConnection : IDisposable
{
    byte UnitIdentifier { get; set; }
    ushort TransactionId { get; set; }
    ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken);
    ValueTask<ReadOnlyMemory<byte>> PerformFunctionAsync(byte functionNumber, ushort[] parameters, byte[] values, CancellationToken cancellationToken);
    void Close();
}
