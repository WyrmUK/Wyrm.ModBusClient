using System.Net;

namespace Wyrm.ModBusClient.Connection;

internal interface IModBusConnection : IDisposable
{
    ushort ProtocolIdentifier { get; set; }
    byte UnitIdentifier { get; set; }
    ushort TransactionId { get; set; }
    Func<IList<byte>, IList<byte>>? PduFramer { get; set; }
    Func<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>? PduDeframer { get; set; }
    ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken);
    ValueTask<ReadOnlyMemory<byte>> PerformFunctionAsync(byte functionNumber, ushort[] parameters, byte[] values, CancellationToken cancellationToken);
    ValueTask<ushort> RequestFunctionAsync(byte functionNumber, ushort[] parameters, byte[] values, CancellationToken cancellationToken);
    ValueTask<FunctionData> PerformReadAsync(CancellationToken cancellationToken);
    void Close();
}
