using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace Wyrm.ModBusClient.Socket;

[ExcludeFromCodeCoverage(Justification = "This is essentially a wrapper of the Socket class.")]
internal sealed class SocketWrapper(
    System.Net.Sockets.Socket _socket) : ISocketWrapper
{
    public ValueTask ConnectAsync(EndPoint remoteEP, CancellationToken cancellationToken)
    {
        return _socket.ConnectAsync(remoteEP, cancellationToken);
    }

    public bool Connected => _socket.Connected;

    public void Close()
    {
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
    }

    public ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken) =>
        _socket.SendAsync(buffer, cancellationToken);

    public ValueTask<int> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken) =>
        _socket.ReceiveAsync(buffer, cancellationToken);

    public void Dispose()
    {
        _socket.Dispose();
    }
}
