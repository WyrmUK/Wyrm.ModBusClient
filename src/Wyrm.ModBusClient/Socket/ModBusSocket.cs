using System.Net;

namespace Wyrm.ModBusClient.Socket;

internal sealed class ModBusSocket(
    ISocketWrapper _socket) : IModBusSocket
{
    public ValueTask ConnectAsync(EndPoint remoteEP, CancellationToken cancellationToken) =>
        _socket.ConnectAsync(remoteEP, cancellationToken);

    public bool Connected => _socket.Connected;

    public async ValueTask<int> SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        var sent = 0;
        while (sent < data.Length)
        {
            var hasSent = await _socket.SendAsync(data.Slice(sent), cancellationToken);
            if (hasSent == 0) break;
            sent += hasSent;
        }
        return sent;
    }

    public async ValueTask<ReadOnlyMemory<byte>> ReceiveAsync(CancellationToken cancellationToken)
    {
        const int minReceived = 10;
        const int maxReceived = 260;
        const int lengthHiIndex = 4;
        const int lengthLoIndex = 5;
        const int headerLength = lengthLoIndex + 1;
        const byte exceptionFlag = 0x80;

        var receivedBuffer = new byte[maxReceived];
        var buffer = new byte[maxReceived];
        var expectedLength = minReceived;

        var received = 0;
        do
        {
            var bytesReceived = await _socket.ReceiveAsync(buffer, cancellationToken);
            if (bytesReceived == 0)
                throw new ModBusClientException("ModBus Socket: No data received.", ModBusExceptionCode.NoDataReceived);

            Array.Copy(buffer, 0, receivedBuffer, received, bytesReceived);
            received += bytesReceived;

            expectedLength = headerLength + (receivedBuffer[lengthHiIndex] * 256) + receivedBuffer[lengthLoIndex];

        } while (received < minReceived || ((buffer[minReceived - 1] & exceptionFlag) == 0 && received < expectedLength));

        return new ReadOnlyMemory<byte>(receivedBuffer, 0, received);
    }

    public void Close()
    {
        _socket.Close();
    }

    public void Dispose()
    {
        _socket.Dispose();
    }
}
