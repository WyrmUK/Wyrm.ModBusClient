using System.Net;
using System.Net.Sockets;
using Wyrm.ModBusClient.Socket;

namespace Wyrm.ModBusClient.Connection;

internal sealed class ModBusConnection(
    IModBusSocketFactory _modBusSocketFactory) : IModBusConnection
{
    private IModBusSocket? _socket;
    private const int MbapHeaderPlusFunctionLength = 8;
    private const byte ExceptionFlag = 0x80;
    private const byte FunctionMask = 0x7F;

    public ushort ProtocolIdentifier { get; set; }

    public byte UnitIdentifier { get; set; } = 1;

    public ushort TransactionId { get; set; } = 1;

    public Func<IList<byte>, IList<byte>>? PduFramer { get; set; }

    public Func<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>? PduDeframer { get; set; }

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
        var transactionId = await RequestFunctionAsync(functionNumber, parameters, values, cancellationToken);

        var receivedFunctionData = await PerformReadAsync(cancellationToken);

        if (transactionId != receivedFunctionData.TransactionId)
            throw new ModBusClientException("ModBus Client: Incorrect transaction id received.", ModBusExceptionCode.IncorrectTransactionIdReceived);

        if ((receivedFunctionData.FunctionNumber & FunctionMask) != functionNumber)
            throw new ModBusClientException("ModBus Client: Incorrect function received.", ModBusExceptionCode.IncorrectFunctionReceived);

        return receivedFunctionData.Data;
    }

    public async ValueTask<ushort> RequestFunctionAsync(byte functionNumber, ushort[] parameters, byte[] values, CancellationToken cancellationToken)
    {
        if (_socket == null)
            throw new ModBusClientException("ModBus Client: Socket not connected.", ModBusExceptionCode.SocketNotConnected);

        var transactionId = TransactionId;

        var data = BuildCommand(functionNumber, parameters, values, PduFramer);

        await SendDataAsync(data, cancellationToken);

        return transactionId;
    }

    public async ValueTask<FunctionData> PerformReadAsync(CancellationToken cancellationToken)
    {
        if (_socket == null)
            throw new ModBusClientException("ModBus Client: Socket not connected.", ModBusExceptionCode.SocketNotConnected);

        var receivedData = await ReceiveDataAsync(cancellationToken);

        if (receivedData.Length < MbapHeaderPlusFunctionLength + 1)
            throw new ModBusClientException("ModBus Client: Insufficient data.", ModBusExceptionCode.InsufficientData);

        var receivedDataSpan = receivedData.Span;

        ushort transactionId = (ushort)((receivedDataSpan[0] << 8) + receivedDataSpan[1]);

        if (PduDeframer != null)
        {
            var deframedPdu = PduDeframer.Invoke(receivedData[(MbapHeaderPlusFunctionLength - 2)..]);
            receivedData = new ReadOnlyMemory<byte>(receivedData[..(MbapHeaderPlusFunctionLength - 2)].ToArray().Concat(deframedPdu.ToArray()).ToArray());
            receivedDataSpan = receivedData.Span;
        }

        var unitIdentifier = receivedDataSpan[MbapHeaderPlusFunctionLength - 2];

        var receivedFunction = receivedDataSpan[MbapHeaderPlusFunctionLength - 1];

        if (receivedFunction >= ExceptionFlag)
        {
            var exceptionCode = (ModBusExceptionCode)receivedData.Span[MbapHeaderPlusFunctionLength];
            throw new ModBusClientException($"ModBus Client: Received exception: {exceptionCode}", exceptionCode);
        }

        return new FunctionData(transactionId, unitIdentifier, receivedFunction, receivedData[MbapHeaderPlusFunctionLength..]);
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

    private ReadOnlyMemory<byte> BuildCommand(byte functionNumber, ICollection<ushort> parameters, ICollection<byte> values, Func<IList<byte>, IList<byte>>? pduFramer)
    {
        var command = new List<byte>
        {
            UnitIdentifier, // Not strictly part of the PDU but can be framed.
            functionNumber
        };

        foreach (var parameter in parameters)
        {
            command.AddRange(GetBytes(parameter));
        }

        command.AddRange(values);

        return BuildCommand(pduFramer?.Invoke(command) ?? command);
    }

    private ReadOnlyMemory<byte> BuildCommand(IList<byte> command)
    {
        var buffer = new List<byte>();

        buffer.AddRange(GetBytes(TransactionId++));

        buffer.AddRange(GetBytes(ProtocolIdentifier));

        buffer.AddRange(GetBytes((ushort)command.Count));

        buffer.AddRange(command);

        return new ReadOnlyMemory<byte>([.. buffer]);
    }

    private async ValueTask SendDataAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        try
        {
            await _socket!.SendAsync(data, cancellationToken);
        }
        catch (SocketException ex)
        {
            throw new ModBusClientException("ModBus Client: Socket Error.", ModBusExceptionCode.SocketError, ex);
        }
    }

    private async ValueTask<ReadOnlyMemory<byte>> ReceiveDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _socket!.ReceiveAsync(cancellationToken);
        }
        catch (SocketException ex)
        {
            throw new ModBusClientException("ModBus Client: Socket Error.", ModBusExceptionCode.SocketError, ex);
        }
    }

    private static byte[] GetBytes(ushort value) =>
    [
        (byte)(value >> 8),
        (byte)(value & 0xFF)
    ];
}
