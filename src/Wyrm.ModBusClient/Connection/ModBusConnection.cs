using System.Net;
using System.Net.Sockets;
using Wyrm.ModBusClient.Socket;

namespace Wyrm.ModBusClient.Connection;

internal sealed class ModBusConnection(
    IModBusSocketFactory _modBusSocketFactory) : IModBusConnection
{
    private IModBusSocket? _socket;
    private ushort _transactionId = 0;
    private readonly byte[] _protocolIdentifier = [0, 0];
    private const int MbapHeaderPlusFunctionLength = 8;
    private const byte ExceptionFlag = 0x80;
    private const byte FunctionMask = 0x7F;

    public byte UnitIdentifier { get; set; } = 1;

    public ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken)
    {
        _socket ??= _modBusSocketFactory.CreateSocket(endPoint);

        try
        {
            return _socket.ConnectAsync(endPoint, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (SocketException ex)
        {
            throw new ModBusClientException("ModBus Client: Socket Error while connecting to end point.", ModBusExceptionCode.SocketError, ex);
        }
        catch (Exception ex)
        {
            throw new ModBusClientException("ModBus Client: Error while connecting to end point.", ModBusExceptionCode.ConnectionError, ex);
        }
    }

    public async ValueTask<ReadOnlyMemory<byte>> PerformFunctionAsync(byte functionNumber, ushort[] parameters, byte[] values, CancellationToken cancellationToken)
    {
        if (_socket == null)
            throw new ModBusClientException("ModBus Client: Socket not connected.", ModBusExceptionCode.SocketNotConnected);

        var data = BuildCommand(functionNumber, parameters, values);

        var receivedData = await SendReceiveDataAsync(data, cancellationToken);

        if (receivedData.Length < MbapHeaderPlusFunctionLength + 1)
            throw new ModBusClientException("ModBus Client: Insufficient data.", ModBusExceptionCode.InsufficientData);

        if (!data[..2].Span.SequenceEqual(receivedData[..2].Span))
            throw new ModBusClientException("ModBus Client: Incorrect transaction id received.", ModBusExceptionCode.IncorrectTransactionIdReceived);

        var receivedFunction = receivedData.Span[MbapHeaderPlusFunctionLength - 1];

        if ((receivedFunction & FunctionMask) != functionNumber)
            throw new ModBusClientException("ModBus Client: Incorrect function received.", ModBusExceptionCode.IncorrectFunctionReceived);

        if (receivedFunction >= ExceptionFlag)
        {
            var exceptionCode = (ModBusExceptionCode)receivedData.Span[MbapHeaderPlusFunctionLength];
            throw new ModBusClientException($"ModBus Client: Received exception: {exceptionCode}", exceptionCode);
        }

        return receivedData[MbapHeaderPlusFunctionLength..];
    }

    public void Close()
    {
        _socket?.Close();
    }

    public void Dispose()
    {
        _socket?.Dispose();
        _socket = null;
    }

    private ReadOnlyMemory<byte> BuildCommand(byte functionNumber, ICollection<ushort> parameters, ICollection<byte>? values = null)
    {
        var command = new List<byte>
        {
            functionNumber
        };

        foreach (var parameter in parameters)
        {
            command.AddRange(GetBytes(parameter));
        }

        if (values != null)
        {
            command.AddRange(values);
        }

        return BuildCommand(command);
    }

    private ReadOnlyMemory<byte> BuildCommand(List<byte> command)
    {
        var buffer = new List<byte>();

        ++_transactionId;
        buffer.AddRange(GetBytes(_transactionId));

        buffer.Add(_protocolIdentifier[1]);
        buffer.Add(_protocolIdentifier[0]);

        buffer.AddRange(GetBytes((ushort)(command.Count + 1)));

        buffer.Add(UnitIdentifier);

        buffer.AddRange(command);

        return new ReadOnlyMemory<byte>([.. buffer]);
    }

    private async ValueTask<ReadOnlyMemory<byte>> SendReceiveDataAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        try
        {
            await _socket!.SendAsync(data, cancellationToken);
            return await _socket.ReceiveAsync(cancellationToken);
        }
        catch (SocketException ex)
        {
            throw new ModBusClientException("ModBus Client: Socket Error while reading coils.", ModBusExceptionCode.SocketError, ex);
        }
    }

    private static byte[] GetBytes(ushort value) =>
    [
        (byte)(value >> 8),
        (byte)(value & 0xFF)
    ];
}
