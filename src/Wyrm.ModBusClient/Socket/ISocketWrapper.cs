using System.Net;

namespace Wyrm.ModBusClient.Socket;

internal interface ISocketWrapper : IDisposable
{
    ValueTask ConnectAsync(EndPoint remoteEP, CancellationToken cancellationToken);
    bool Connected { get; }
    ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken);
    ValueTask<int> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken);
    void Close();
}
