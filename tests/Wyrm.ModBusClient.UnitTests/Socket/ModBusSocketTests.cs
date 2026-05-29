using Moq;
using Shouldly;
using System.Net;
using Wyrm.ModBusClient.Socket;

namespace Wyrm.ModBusClient.UnitTests.Socket;

public class ModBusSocketTests
{
    private readonly IModBusSocket _modBusSocket;

    private readonly ISocketWrapper _socketWrapper = Mock.Of<ISocketWrapper>();

    private static readonly ReadOnlyMemory<byte> SendData = new([1, 2, 3, 4, 5, 6, 7, 255]);
    private static readonly byte[] ReceiveData = [8, 9, 10, 11, 0, 6, 14, 15, 16, 17, 18, 255];
    private static readonly byte[][] ReceiveDataChunks = [[8, 9, 10, 11], [0, 6, 14, 15, 16, 17, 18, 255]];
    private static readonly byte[] ReceiveExceptionData = [8, 9, 10, 11, 0, 4, 14, 15, 16, 0x97];

    public ModBusSocketTests()
    {
        _modBusSocket = new ModBusSocket(_socketWrapper);
    }

    #region Connect

    [Fact]
    public async Task ConnectAsync_Should_Call_ConnectAsync()
    {
        var endPoint = Mock.Of<EndPoint>();

        await _modBusSocket.ConnectAsync(endPoint, TestContext.Current.CancellationToken);

        Mock.Get(_socketWrapper)
            .Verify(x => x.ConnectAsync(endPoint, TestContext.Current.CancellationToken), Times.Once);
    }

    #endregion

    #region Connected

    [Fact]
    public void Connected_Should_Get_Socket_Connected()
    {
        Mock.Get(_socketWrapper)
            .Setup(x => x.Connected)
            .Returns(true);

        var result = _modBusSocket.Connected;

        result.ShouldBeTrue();
    }

    #endregion

    #region Send

    [Fact]
    public async Task SendAsync_Should_Call_SendAsync()
    {
        Mock.Get(_socketWrapper)
            .Setup(x => x.SendAsync(SendData, TestContext.Current.CancellationToken))
            .ReturnsAsync(SendData.Length);

        var result = await _modBusSocket.SendAsync(SendData, TestContext.Current.CancellationToken);

        result.ShouldBe(SendData.Length);
    }

    [Fact]
    public async Task SendAsync_Should_Repeatedly_Call_SendAsync()
    {
        var dataSentCount = SendData.Length - 1;
        var lastDataSent = SendData.Slice(dataSentCount);
        Mock.Get(_socketWrapper)
            .Setup(x => x.SendAsync(SendData, TestContext.Current.CancellationToken))
            .ReturnsAsync(dataSentCount);
        Mock.Get(_socketWrapper)
            .Setup(x => x.SendAsync(lastDataSent, TestContext.Current.CancellationToken))
            .ReturnsAsync(lastDataSent.Length);

        var result = await _modBusSocket.SendAsync(SendData, TestContext.Current.CancellationToken);

        result.ShouldBe(SendData.Length);
    }

    [Fact]
    public async Task SendAsync_Should_Stop_If_No_Data_Sent()
    {
        var dataSentCount = SendData.Length - 1;
        var lastDataSent = SendData.Slice(dataSentCount);
        Mock.Get(_socketWrapper)
            .Setup(x => x.SendAsync(SendData, TestContext.Current.CancellationToken))
            .ReturnsAsync(dataSentCount);
        Mock.Get(_socketWrapper)
            .Setup(x => x.SendAsync(lastDataSent, TestContext.Current.CancellationToken))
            .ReturnsAsync(0);

        var result = await _modBusSocket.SendAsync(SendData, TestContext.Current.CancellationToken);

        result.ShouldBe(dataSentCount);
    }

    #endregion

    #region Receive

    [Fact]
    public async Task ReceiveAsync_Should_Call_ReceiveAsync()
    {
        Mock.Get(_socketWrapper)
            .Setup(x => x.ReceiveAsync(It.IsAny<byte[]>(), TestContext.Current.CancellationToken))
            .Callback<byte[], CancellationToken>((buffer, _) =>
            {
                Array.Copy(ReceiveData, buffer, ReceiveData.Length);
            })
            .ReturnsAsync(ReceiveData.Length);

        var result = await _modBusSocket.ReceiveAsync(TestContext.Current.CancellationToken);

        result.Span.ToArray().ShouldBeEquivalentTo(ReceiveData);
    }

    [Fact]
    public async Task ReceiveAsync_Should_Repeatedly_Call_ReceiveAsync()
    {
        var chunkIndex = 0;
        Mock.Get(_socketWrapper)
            .Setup(x => x.ReceiveAsync(It.IsAny<byte[]>(), TestContext.Current.CancellationToken))
            .Callback<byte[], CancellationToken>((buffer, _) =>
            {
                Array.Copy(ReceiveDataChunks[chunkIndex], buffer, ReceiveDataChunks[chunkIndex].Length);
            })
            .ReturnsAsync(() => ReceiveDataChunks[chunkIndex++].Length);

        var result = await _modBusSocket.ReceiveAsync(TestContext.Current.CancellationToken);

        result.Span.ToArray().ShouldBeEquivalentTo(ReceiveData);
    }

    [Fact]
    public async Task ReceiveAsync_Should_Return_If_Exception_Received()
    {
        Mock.Get(_socketWrapper)
            .Setup(x => x.ReceiveAsync(It.IsAny<byte[]>(), TestContext.Current.CancellationToken))
            .Callback<byte[], CancellationToken>((buffer, _) =>
            {
                Array.Copy(ReceiveExceptionData, buffer, ReceiveExceptionData.Length);
            })
            .ReturnsAsync(ReceiveExceptionData.Length);

        var result = await _modBusSocket.ReceiveAsync(TestContext.Current.CancellationToken);

        result.Span.ToArray().ShouldBeEquivalentTo(ReceiveExceptionData);
    }

    [Fact]
    public async Task ReceiveAsync_Should_Throw_ModBusClientException_If_No_Data_Received()
    {
        Mock.Get(_socketWrapper)
            .Setup(x => x.ReceiveAsync(It.IsAny<byte[]>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(0);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modBusSocket.ReceiveAsync(TestContext.Current.CancellationToken).AsTask());

        exception.ExceptionCode.ShouldBe(ModBusExceptionCode.NoDataReceived);
    }

    #endregion

    #region Close

    [Fact]
    public void Close_Should_Call_Close()
    {
        _modBusSocket.Close();

        Mock.Get(_socketWrapper)
            .Verify(x => x.Close(), Times.Once);
    }

    #endregion

    #region Dispose

    [Fact]
    public void Dispose_Should_Call_Dispose()
    {
        _modBusSocket.Dispose();

        Mock.Get(_socketWrapper)
            .Verify(x => x.Dispose(), Times.Once);
    }

    #endregion
}
