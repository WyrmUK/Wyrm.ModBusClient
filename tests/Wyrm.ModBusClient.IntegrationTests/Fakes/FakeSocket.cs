using System.Net;
using System.Net.Sockets;
using Wyrm.ModBusClient.Socket;

namespace Wyrm.ModBusClient.IntegrationTests.Fakes;

internal class FakeSocket : ISocketWrapper
{
    private byte[] _dataToSend = [];

    public EndPoint? RemoteEP { get; private set; }

    public ICollection<(byte[] Sent, byte[] Received)> SendReceiveData { get; set; } = [];

    public ValueTask ConnectAsync(EndPoint remoteEP, CancellationToken cancellationToken)
    {
        RemoteEP = remoteEP;
        return ValueTask.CompletedTask;
    }

    public ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        if (RemoteEP == null) throw new SocketException((int)SocketError.NotConnected);

        _dataToSend = SendReceiveData.FirstOrDefault(srd => srd.Sent.SequenceEqual(buffer.Span.ToArray())).Received ?? [];

        return ValueTask.FromResult(buffer.Length);
    }

    public ValueTask<int> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken)
    {
        if (RemoteEP == null) throw new SocketException((int)SocketError.NotConnected);

        var length = _dataToSend.Length;
        Array.Copy(_dataToSend, buffer, length);
        _dataToSend = [];

        return ValueTask.FromResult(length);
    }

    public void Close()
    {
        RemoteEP = null;
    }

    public void Dispose()
    {
        Close();
    }
}
