using Moq;
using Shouldly;
using System.Net;
using System.Net.Sockets;
using Wyrm.ModBusClient.Connection;
using Wyrm.ModBusClient.Socket;

namespace Wyrm.ModBusClient.UnitTests.Connection;

public class ModBusConnectionTests
{
    private readonly IModBusConnection _modBusConnection;

    private readonly IModBusSocketFactory _modBusSocketFactory = Mock.Of<IModBusSocketFactory>();
    private readonly EndPoint _endPoint = Mock.Of<EndPoint>();
    private readonly IModBusSocket _modBusSocket = Mock.Of<IModBusSocket>();

    public ModBusConnectionTests()
    {
        _modBusConnection = new ModBusConnection(_modBusSocketFactory);
        InitialiseSocketFactoryMock();
    }

    private void InitialiseSocketFactoryMock()
    {
        Mock.Get(_modBusSocketFactory)
            .Setup(x => x.CreateSocket(_endPoint))
            .Returns(_modBusSocket);
    }

    #region Unit Identifier

    [Fact]
    public void UnitIdentifier_Should_Get_What_Is_Set()
    {
        const byte unitIdentitier = 0xFF;

        _modBusConnection.UnitIdentifier = unitIdentitier;

        _modBusConnection.UnitIdentifier.ShouldBe(unitIdentitier);
    }

    #region Transaction Id

    [Fact]
    public void TransactionId_Should_Get_What_Is_Set()
    {
        const ushort transactionId = 0x5959;

        _modBusConnection.TransactionId = transactionId;

        _modBusConnection.TransactionId.ShouldBe(transactionId);
    }

    #endregion

    #endregion

    #region Connect

    [Fact]
    public async Task ConnectAsync_Should_Call_ConnectAsync()
    {
        await _modBusConnection.ConnectAsync(_endPoint, TestContext.Current.CancellationToken);

        Mock.Get(_modBusSocket)
            .Verify(x => x.ConnectAsync(_endPoint, TestContext.Current.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task ConnectAsync_Should_Throw_OperationCanceledExceptions()
    {
        var cancellationToken = new CancellationToken(true);
        Mock.Get(_modBusSocket)
            .Setup(x => x.ConnectAsync(_endPoint, cancellationToken))
            .Throws<TaskCanceledException>();

        await Should.ThrowAsync<TaskCanceledException>(() => _modBusConnection.ConnectAsync(_endPoint, cancellationToken).AsTask());
    }

    [Fact]
    public async Task ConnectAsync_Should_Throw_SocketExceptions_As_ModBusClientExceptions()
    {
        var socketException = new SocketException();
        Mock.Get(_modBusSocket)
            .Setup(x => x.ConnectAsync(_endPoint, TestContext.Current.CancellationToken))
            .Throws(socketException);

        var result = await Should.ThrowAsync<ModBusClientException>(() => _modBusConnection.ConnectAsync(_endPoint, TestContext.Current.CancellationToken).AsTask());

        result.ShouldNotBeNull().InnerException.ShouldBe(socketException);
        result.ExceptionCode.ShouldBe(ModBusExceptionCode.SocketError);
    }

    [Fact]
    public async Task ConnectAsync_Should_Throw_Exceptions_As_ModBusClientExceptions()
    {
        var exception = new ArgumentNullException();
        Mock.Get(_modBusSocket)
            .Setup(x => x.ConnectAsync(_endPoint, TestContext.Current.CancellationToken))
            .Throws(exception);

        var result = await Should.ThrowAsync<ModBusClientException>(() => _modBusConnection.ConnectAsync(_endPoint, TestContext.Current.CancellationToken).AsTask());

        result.ShouldNotBeNull().InnerException.ShouldBe(exception);
        result.ExceptionCode.ShouldBe(ModBusExceptionCode.ConnectionError);
    }

    #endregion

    #region Perform Function

    private const byte FunctionNumber = 2;
    private const byte UnitIdentifier = 5;
    private const ushort TransactionId = 0x5959;
    private static readonly ushort[] UshortParameters = [ 0x0001, 0xFFFF ];
    private static readonly byte[] ByteParameters = [ 0x01, 0xFF ];
    private static readonly byte[] ExpectedCommand = [ TransactionId >> 8, TransactionId & 0xFF, 0, 0, 0, 8, UnitIdentifier, FunctionNumber, 0x00, 0x01, 0xFF, 0xFF, 0x01, 0xFF ];
    private static readonly byte[] ExpectedResult = [TransactionId >> 8, TransactionId & 0xFF, 0, 0, 0, 5, UnitIdentifier, FunctionNumber, 3, 4, 5 ];

    public static readonly TheoryData<byte[], ModBusExceptionCode> ErrorData = new()
    {
        { [TransactionId >> 8, TransactionId & 0xFF, 0], ModBusExceptionCode.InsufficientData },
        { [1, TransactionId & 0xFF, 0, 0, 0, 5, 1, FunctionNumber, 3, 4, 5], ModBusExceptionCode.IncorrectTransactionIdReceived },
        { [TransactionId >> 8, 1, 0, 0, 0, 5, 1, FunctionNumber, 3, 4, 5], ModBusExceptionCode.IncorrectTransactionIdReceived },
        { [TransactionId >> 8, TransactionId & 0xFF, 0, 0, 0, 5, 1, FunctionNumber + 1, 3, 4, 5], ModBusExceptionCode.IncorrectFunctionReceived },
        { [TransactionId >> 8, TransactionId & 0xFF, 0, 0, 0, 5, 1, 0x80 + FunctionNumber, 1], ModBusExceptionCode.IllegalFunction }
    };

    [Fact]
    public async Task PerformFunctionAsync_Should_Throw_ModBusClientException_If_Not_Connected()
    {
        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modBusConnection.PerformFunctionAsync(FunctionNumber, UshortParameters, ByteParameters, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldNotBeNull().ExceptionCode.ShouldBe(ModBusExceptionCode.SocketNotConnected);
    }

    [Fact]
    public async Task PerformFunctionAsync_Should_Throw_ModBusClientException_For_SocketException()
    {
        var socketException = new SocketException();
        Mock.Get(_modBusSocket)
            .Setup(x => x.SendAsync(It.IsAny<ReadOnlyMemory<byte>>(), TestContext.Current.CancellationToken))
            .Throws(socketException);

        await _modBusConnection.ConnectAsync(_endPoint, TestContext.Current.CancellationToken);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modBusConnection.PerformFunctionAsync(FunctionNumber, UshortParameters, ByteParameters, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldNotBeNull().ExceptionCode.ShouldBe(ModBusExceptionCode.SocketError);
        exception.InnerException.ShouldBe(socketException);
    }

    [Fact]
    public async Task PerformFunctionAsync_Should_Format_Command_Correctly_And_Return_Result()
    {
        ReadOnlyMemory<byte> receiveData = new();
        Mock.Get(_modBusSocket)
            .Setup(x => x.SendAsync(It.IsAny<ReadOnlyMemory<byte>>(), TestContext.Current.CancellationToken))
            .Callback<ReadOnlyMemory<byte>, CancellationToken>((sent, _) =>
            {
                if (!sent.Span.SequenceEqual(ExpectedCommand)) return;

                receiveData = ExpectedResult;
            });
        Mock.Get(_modBusSocket)
            .Setup(x => x.ReceiveAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(() => receiveData);

        await _modBusConnection.ConnectAsync(_endPoint, TestContext.Current.CancellationToken);
        _modBusConnection.UnitIdentifier = UnitIdentifier;
        _modBusConnection.TransactionId = TransactionId;

        var result = await _modBusConnection.PerformFunctionAsync(FunctionNumber, UshortParameters, ByteParameters, TestContext.Current.CancellationToken);

        result.ShouldBeEquivalentTo(new ReadOnlyMemory<byte>(ExpectedResult).Slice(8));

        _modBusConnection.TransactionId.ShouldBe((ushort)(TransactionId + 1));
    }

    [Theory]
    [MemberData(nameof(ErrorData))]
    public async Task PerformFunctionAsync_Should_Throw_ModBusClientException_For_Errors(byte[] result, ModBusExceptionCode expectedCode)
    {
        Mock.Get(_modBusSocket)
            .Setup(x => x.ReceiveAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(() => new ReadOnlyMemory<byte>(result));

        await _modBusConnection.ConnectAsync(_endPoint, TestContext.Current.CancellationToken);
        _modBusConnection.TransactionId = TransactionId;

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modBusConnection.PerformFunctionAsync(FunctionNumber, UshortParameters, ByteParameters, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldNotBeNull().ExceptionCode.ShouldBe(expectedCode);
    }

    #endregion

    #region Close

    [Fact]
    public async Task Close_Should_Call_Close()
    {
        await _modBusConnection.ConnectAsync(_endPoint, TestContext.Current.CancellationToken);

        _modBusConnection.Close();

        Mock.Get(_modBusSocket)
            .Verify(x => x.Close(), Times.Once);
    }

    [Fact]
    public void Close_Should_Not_Throw_Exception_If_Not_Connected()
    {
        _modBusConnection.Close();

        Mock.Get(_modBusSocket)
            .Verify(x => x.Close(), Times.Never);
    }

    #endregion

    #region Dispose

    [Fact]
    public async Task Dispose_Should_Call_Dispose_Once()
    {
        await _modBusConnection.ConnectAsync(_endPoint, TestContext.Current.CancellationToken);

        _modBusConnection.Dispose();
        _modBusConnection.Dispose();

        Mock.Get(_modBusSocket)
            .Verify(x => x.Dispose(), Times.Once);
    }

    #endregion
}
