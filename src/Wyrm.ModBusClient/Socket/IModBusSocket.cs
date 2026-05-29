using System.Net;
using Wyrm.ModBusClient.Connection;

namespace Wyrm.ModBusClient.Socket;

internal interface IModBusSocket : IDisposable
{
    ValueTask ConnectAsync(EndPoint remoteEP, CancellationToken cancellationToken);
    bool Connected { get; }
    ValueTask<int> SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken);
    ValueTask<ReadOnlyMemory<byte>> ReceiveAsync(CancellationToken cancellationToken);
    void Close();
}
