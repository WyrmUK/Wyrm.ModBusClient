using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System.Net;
using Wyrm.ModBusClient.Connection;

namespace Wyrm.ModBusClient.UnitTests;

public class ModBusClientTests
{
    private readonly IModBusClient _modbusClient;

    private readonly IModBusCommand _modBusCommand = Mock.Of<IModBusCommand>();
    private readonly ILogger<ModBusClient> _logger = Mock.Of<ILogger<ModBusClient>>();

    private static readonly ModBusClientException TestException = new ModBusClientException("Exception", ModBusExceptionCode.ConnectionError);
    private const ushort StartingAddress = 0x0123;
    private const byte ReadCoilsFunction = 1;
    private const byte ReadDiscreteInputsFunction = 2;
    private const byte ReadHoldingRegistersFunction = 3;
    private const byte ReadInputRegistersFunction = 4;
    private const byte WriteSingleCoilFunction = 5;
    private const byte WriteSingleRegisterFunction = 6;
    private const byte WriteMultipleCoilsFunction = 15;
    private const byte WriteMultipleRegistersFunction = 16;
    private const byte ReadFileRecordFunction = 20;
    private const byte WriteFileRecordFunction = 21;
    private const byte MaskWriteRegisterFunction = 22;
    private const byte ReadWriteMultipleRegistersFunction = 23;
    private const byte ReadFifoQueueFunction = 24;
    private const byte ReadDeviceIdentifierFunction = 43;

    public ModBusClientTests()
    {
        _modbusClient = new ModBusClient(_modBusCommand, _logger);
        InitialiseLoggerMock();
    }

    private void InitialiseLoggerMock()
    {
        Mock.Get(_logger)
            .Setup(x => x.IsEnabled(LogLevel.Information))
            .Returns(true);
    }

    #region Protocol Identifier

    [Fact]
    public void ProtocolIdentifier_Should_Get_What_Is_Set()
    {
        const ushort protocolIdentifier = 0x0001;
        ushort setProtocolIdentifier = 0;
        Mock.Get(_modBusCommand)
            .SetupSet(x => x.ProtocolIdentifier = It.IsAny<ushort>())
            .Callback<ushort>(ui => setProtocolIdentifier = ui);
        Mock.Get(_modBusCommand)
            .SetupGet(x => x.ProtocolIdentifier)
            .Returns(() => setProtocolIdentifier);

        _modBusCommand.ProtocolIdentifier = protocolIdentifier;

        _modBusCommand.ProtocolIdentifier.ShouldBe(protocolIdentifier);
    }

    #endregion

    #region Unit Identifier

    [Fact]
    public void UnitIdentifier_Should_Get_What_Is_Set()
    {
        const byte unitIdentifier = 6;
        byte setUnitIdenifier = 0;
        Mock.Get(_modBusCommand)
            .SetupSet(x => x.UnitIdentifier = It.IsAny<byte>())
            .Callback<byte>(ui => setUnitIdenifier = ui);
        Mock.Get(_modBusCommand)
            .SetupGet(x => x.UnitIdentifier)
            .Returns(() => setUnitIdenifier);

        _modbusClient.UnitIdentifier = unitIdentifier;

        _modbusClient.UnitIdentifier.ShouldBe(unitIdentifier);
    }

    #endregion

    #region Transaction Id

    [Fact]
    public void TransactionId_Should_Get_What_Is_Set()
    {
        const ushort transactionId = 0x5959;
        ushort setTransactionId = 0;
        Mock.Get(_modBusCommand)
            .SetupSet(x => x.TransactionId = It.IsAny<ushort>())
            .Callback<ushort>(ui => setTransactionId = ui);
        Mock.Get(_modBusCommand)
            .SetupGet(x => x.TransactionId)
            .Returns(() => setTransactionId);

        _modbusClient.TransactionId = transactionId;

        _modbusClient.TransactionId.ShouldBe(transactionId);
    }

    #endregion

    #region Connect

    [Fact]
    public async Task ConnectAsync_Should_Call_ConnectAsync()
    {
        var endPoint = Mock.Of<EndPoint>();
        Mock.Get(endPoint)
            .Setup(x => x.Serialize())
            .Returns(new SocketAddress(System.Net.Sockets.AddressFamily.InterNetwork));

        await _modbusClient.ConnectAsync(endPoint, TestContext.Current.CancellationToken);

        Mock.Get(_modBusCommand)
            .Verify(x => x.ConnectAsync(endPoint, TestContext.Current.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task ConnectAsync_Should_Throw_ModBusClientException()
    {
        var endPoint = Mock.Of<EndPoint>();
        Mock.Get(endPoint)
            .Setup(x => x.Serialize())
            .Returns(new SocketAddress(System.Net.Sockets.AddressFamily.InterNetwork));
        Mock.Get(_modBusCommand)
            .Setup(x => x.ConnectAsync(endPoint, TestContext.Current.CancellationToken))
            .Throws(TestException);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modbusClient.ConnectAsync(endPoint, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldBe(TestException);
    }

    #endregion

    #region Read Coils

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public async Task ReadCoilsAsync_Should_Return_Correct_Values(ushort numToRead)
    {
        var expectedResult = new List<bool>(numToRead);
        Mock.Get(_modBusCommand)
            .Setup(x => x.ReadBitValuesAsync(ReadCoilsFunction, StartingAddress, numToRead, TestContext.Current.CancellationToken))
            .ReturnsAsync(expectedResult);

        var result = await _modbusClient.ReadCoilsAsync(StartingAddress, numToRead, TestContext.Current.CancellationToken);

        result.ShouldBeEquivalentTo(numToRead == 0 ? [] : expectedResult);
    }

    [Fact]
    public async Task ReadCoilsAsync_Should_Throw_ModBusClientException()
    {
        var endPoint = Mock.Of<EndPoint>();
        Mock.Get(_modBusCommand)
            .Setup(x => x.ReadBitValuesAsync(ReadCoilsFunction, StartingAddress, It.IsAny<ushort>(), TestContext.Current.CancellationToken))
            .Throws(TestException);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modbusClient.ReadCoilsAsync(StartingAddress, 5, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldBe(TestException);
    }

    #endregion

    #region Read Discrete Inputs

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public async Task ReadDiscreteInputsAsync_Should_Return_Correct_Values(ushort numToRead)
    {
        var expectedResult = new List<bool>(numToRead);
        Mock.Get(_modBusCommand)
            .Setup(x => x.ReadBitValuesAsync(ReadDiscreteInputsFunction, StartingAddress, numToRead, TestContext.Current.CancellationToken))
            .ReturnsAsync(expectedResult);

        var result = await _modbusClient.ReadDiscreteInputsAsync(StartingAddress, numToRead, TestContext.Current.CancellationToken);

        result.ShouldBeEquivalentTo(numToRead == 0 ? [] : expectedResult);
    }

    [Fact]
    public async Task ReadDiscreteInputsAsync_Should_Throw_ModBusClientException()
    {
        var endPoint = Mock.Of<EndPoint>();
        Mock.Get(_modBusCommand)
            .Setup(x => x.ReadBitValuesAsync(ReadDiscreteInputsFunction, StartingAddress, It.IsAny<ushort>(), TestContext.Current.CancellationToken))
            .Throws(TestException);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modbusClient.ReadDiscreteInputsAsync(StartingAddress, 5, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldBe(TestException);
    }

    #endregion

    #region Read Holding Registers

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public async Task ReadHoldingRegistersAsync_Should_Return_Correct_Values(ushort numToRead)
    {
        var expectedResult = new List<ushort>(numToRead);
        Mock.Get(_modBusCommand)
            .Setup(x => x.ReadUshortValuesAsync(ReadHoldingRegistersFunction, StartingAddress, numToRead, TestContext.Current.CancellationToken))
            .ReturnsAsync(expectedResult);

        var result = await _modbusClient.ReadHoldingRegistersAsync(StartingAddress, numToRead, TestContext.Current.CancellationToken);

        result.ShouldBeEquivalentTo(numToRead == 0 ? [] : expectedResult);
    }

    [Fact]
    public async Task ReadHoldingRegistersAsync_Should_Throw_ModBusClientException()
    {
        var endPoint = Mock.Of<EndPoint>();
        Mock.Get(_modBusCommand)
            .Setup(x => x.ReadUshortValuesAsync(ReadHoldingRegistersFunction, StartingAddress, It.IsAny<ushort>(), TestContext.Current.CancellationToken))
            .Throws(TestException);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modbusClient.ReadHoldingRegistersAsync(StartingAddress, 5, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldBe(TestException);
    }

    #endregion

    #region Read Input Registers

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public async Task ReadInputRegistersAsync_Should_Return_Correct_Values(ushort numToRead)
    {
        var expectedResult = new List<ushort>(numToRead);
        Mock.Get(_modBusCommand)
            .Setup(x => x.ReadUshortValuesAsync(ReadInputRegistersFunction, StartingAddress, numToRead, TestContext.Current.CancellationToken))
            .ReturnsAsync(expectedResult);

        var result = await _modbusClient.ReadInputRegistersAsync(StartingAddress, numToRead, TestContext.Current.CancellationToken);

        result.ShouldBeEquivalentTo(numToRead == 0 ? [] : expectedResult);
    }

    [Fact]
    public async Task ReadInputRegistersAsync_Should_Throw_ModBusClientException()
    {
        var endPoint = Mock.Of<EndPoint>();
        Mock.Get(_modBusCommand)
            .Setup(x => x.ReadUshortValuesAsync(ReadInputRegistersFunction, StartingAddress, It.IsAny<ushort>(), TestContext.Current.CancellationToken))
            .Throws(TestException);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modbusClient.ReadInputRegistersAsync(StartingAddress, 5, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldBe(TestException);
    }

    #endregion

    #region Write Single Coil

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WriteSingleCoilAsync_Should_Return_Correct_Values(bool value)
    {
        await _modbusClient.WriteSingleCoilAsync(StartingAddress, value, TestContext.Current.CancellationToken);

        Mock.Get(_modBusCommand)
            .Verify(x => x.PerformFunctionAsync(WriteSingleCoilFunction, It.Is<ushort[]>(a => a.SequenceEqual(new ushort[] { StartingAddress, (ushort)(value ? 0xFF00 : 0) })), TestContext.Current.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task WriteSingleCoilAsync_Should_Throw_ModBusClientException()
    {
        var endPoint = Mock.Of<EndPoint>();
        Mock.Get(_modBusCommand)
            .Setup(x => x.PerformFunctionAsync(WriteSingleCoilFunction, It.IsAny<ushort[]>(), TestContext.Current.CancellationToken))
            .Throws(TestException);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modbusClient.WriteSingleCoilAsync(StartingAddress, false, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldBe(TestException);
    }

    #endregion

    #region Write Single Register

    [Fact]
    public async Task WriteSingleRegisterAsync_Should_Return_Correct_Values()
    {
        const ushort value = 0x0345;

        await _modbusClient.WriteSingleRegisterAsync(StartingAddress, value, TestContext.Current.CancellationToken);

        Mock.Get(_modBusCommand)
            .Verify(x => x.PerformFunctionAsync(WriteSingleRegisterFunction, It.Is<ushort[]>(a => a.SequenceEqual(new ushort[] { StartingAddress, value })), TestContext.Current.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task WriteSingleRegisterAsync_Should_Throw_ModBusClientException()
    {
        var endPoint = Mock.Of<EndPoint>();
        Mock.Get(_modBusCommand)
            .Setup(x => x.PerformFunctionAsync(WriteSingleRegisterFunction, It.IsAny<ushort[]>(), TestContext.Current.CancellationToken))
            .Throws(TestException);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modbusClient.WriteSingleRegisterAsync(StartingAddress, 0x0001, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldBe(TestException);
    }

    #endregion

    #region Write Multiple Coils

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public async Task WriteMultipleCoilsAsync_Should_Return_Correct_Values(int numToWrite)
    {
        var values = Enumerable.Range(1, numToWrite).Select(i => i % 2 == 0).ToArray();

        await _modbusClient.WriteMultipleCoilsAsync(StartingAddress, values, TestContext.Current.CancellationToken);

        Mock.Get(_modBusCommand)
            .Verify(x => x.WriteBitValuesAsync(WriteMultipleCoilsFunction, StartingAddress, values, TestContext.Current.CancellationToken), numToWrite == 0 ? Times.Never : Times.Once);
    }

    [Fact]
    public async Task WriteMultipleCoilsAsync_Should_Throw_ModBusClientException()
    {
        var endPoint = Mock.Of<EndPoint>();
        Mock.Get(_modBusCommand)
            .Setup(x => x.WriteBitValuesAsync(WriteMultipleCoilsFunction, It.IsAny<ushort>(), It.IsAny<ICollection<bool>>(), TestContext.Current.CancellationToken))
            .Throws(TestException);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modbusClient.WriteMultipleCoilsAsync(StartingAddress, [false], TestContext.Current.CancellationToken).AsTask());

        exception.ShouldBe(TestException);
    }

    #endregion

    #region Write Multiple Registers

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public async Task WriteMultipleRegistersAsync_Should_Return_Correct_Values(int numToWrite)
    {
        var values = Enumerable.Range(1, numToWrite).Select(i => (ushort)i).ToArray();

        await _modbusClient.WriteMultipleRegistersAsync(StartingAddress, values, TestContext.Current.CancellationToken);

        Mock.Get(_modBusCommand)
            .Verify(x => x.WriteUshortValuesAsync(WriteMultipleRegistersFunction, StartingAddress, values, TestContext.Current.CancellationToken), numToWrite == 0 ? Times.Never : Times.Once);
    }

    [Fact]
    public async Task WriteMultipleRegistersAsync_Should_Throw_ModBusClientException()
    {
        var endPoint = Mock.Of<EndPoint>();
        Mock.Get(_modBusCommand)
            .Setup(x => x.WriteUshortValuesAsync(WriteMultipleRegistersFunction, It.IsAny<ushort>(), It.IsAny<ICollection<ushort>>(), TestContext.Current.CancellationToken))
            .Throws(TestException);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modbusClient.WriteMultipleRegistersAsync(StartingAddress, [0x0001], TestContext.Current.CancellationToken).AsTask());

        exception.ShouldBe(TestException);
    }

    #endregion

    #region Read File Record

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public async Task ReadFileRecordAsync_Should_Return_Correct_Values(ushort numToRead)
    {
        var fileRecordsToRead = Enumerable.Range(1, numToRead).Select(i => new FileRecords((ushort)i, StartingAddress, 1)).ToArray();
        ICollection<ICollection<ushort>> expectedResult = new List<ICollection<ushort>>(numToRead);
        Mock.Get(_modBusCommand)
            .Setup(x => x.ReadFileRecordsAsync(ReadFileRecordFunction, fileRecordsToRead, TestContext.Current.CancellationToken))
            .ReturnsAsync(expectedResult);

        var result = await _modbusClient.ReadFileRecordAsync(fileRecordsToRead, TestContext.Current.CancellationToken);

        result.ShouldBeEquivalentTo(numToRead == 0 ? [] : expectedResult);
    }

    [Fact]
    public async Task ReadFileRecordAsync_Should_Throw_ModBusClientException()
    {
        var endPoint = Mock.Of<EndPoint>();
        Mock.Get(_modBusCommand)
            .Setup(x => x.ReadFileRecordsAsync(ReadFileRecordFunction, It.IsAny<ICollection<FileRecords>>(), TestContext.Current.CancellationToken))
            .Throws(TestException);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modbusClient.ReadFileRecordAsync([new FileRecords(1, StartingAddress, 1)], TestContext.Current.CancellationToken).AsTask());

        exception.ShouldBe(TestException);
    }

    #endregion

    #region Write File Record

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public async Task WriteFileRecordAsync_Should_Return_Correct_Values(int numToWrite)
    {
        var values = Enumerable.Range(1, numToWrite).Select(i => new FileRecordsData((ushort)i, StartingAddress, [1])).ToArray();

        await _modbusClient.WriteFileRecordAsync(values, TestContext.Current.CancellationToken);

        Mock.Get(_modBusCommand)
            .Verify(x => x.WriteFileRecordsAsync(WriteFileRecordFunction, values, TestContext.Current.CancellationToken), numToWrite == 0 ? Times.Never : Times.Once);
    }

    [Fact]
    public async Task WriteFileRecordAsync_Should_Throw_ModBusClientException()
    {
        var endPoint = Mock.Of<EndPoint>();
        Mock.Get(_modBusCommand)
            .Setup(x => x.WriteFileRecordsAsync(WriteFileRecordFunction, It.IsAny<ICollection<FileRecordsData>>(), TestContext.Current.CancellationToken))
            .Throws(TestException);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modbusClient.WriteFileRecordAsync([new FileRecordsData(1, StartingAddress, [1])], TestContext.Current.CancellationToken).AsTask());

        exception.ShouldBe(TestException);
    }

    #endregion

    #region Mask Write Register

    [Fact]
    public async Task MaskWriteRegisterAsync_Should_Return_Correct_Values()
    {
        const ushort andMask = 0x0345;
        const ushort orMask = 0xFF00;

        await _modbusClient.MaskWriteRegisterAsync(StartingAddress, andMask, orMask, TestContext.Current.CancellationToken);

        Mock.Get(_modBusCommand)
            .Verify(x => x.PerformFunctionAsync(MaskWriteRegisterFunction, It.Is<ushort[]>(a => a.SequenceEqual(new ushort[] { StartingAddress, andMask, orMask })), TestContext.Current.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task MaskWriteRegisterAsync_Should_Throw_ModBusClientException()
    {
        var endPoint = Mock.Of<EndPoint>();
        Mock.Get(_modBusCommand)
            .Setup(x => x.PerformFunctionAsync(MaskWriteRegisterFunction, It.IsAny<ushort[]>(), TestContext.Current.CancellationToken))
            .Throws(TestException);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modbusClient.MaskWriteRegisterAsync(StartingAddress, 0x0001, 0x0001, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldBe(TestException);
    }

    #endregion

    #region Read Write Multiple Registers

    [Theory]
    [InlineData(0, 0)]
    [InlineData(4, 0)]
    [InlineData(0, 4)]
    [InlineData(4, 4)]
    public async Task ReadWriteMultipleRegistersAsync_Should_Return_Correct_Values(ushort numToRead, ushort numToWrite)
    {
        var expectedResult = new List<ushort>(numToRead);
        var startingAddressWrite = (ushort)(StartingAddress + numToRead);
        var values = Enumerable.Range(1, numToWrite).Select(i => (ushort)i).ToArray();
        Mock.Get(_modBusCommand)
            .Setup(x => x.ReadWriteUshortValuesAsync(ReadWriteMultipleRegistersFunction, StartingAddress, numToRead, startingAddressWrite, values, TestContext.Current.CancellationToken))
            .ReturnsAsync(expectedResult);

        var result = await _modbusClient.ReadWriteMultipleRegistersAsync(StartingAddress, numToRead, startingAddressWrite, values, TestContext.Current.CancellationToken);

        result.ShouldBeEquivalentTo(numToRead == 0 && numToWrite == 0 ? [] : expectedResult);
    }

    [Fact]
    public async Task ReadWriteMultipleRegistersAsync_Should_Throw_ModBusClientException()
    {
        var endPoint = Mock.Of<EndPoint>();
        Mock.Get(_modBusCommand)
            .Setup(x => x.ReadWriteUshortValuesAsync(ReadWriteMultipleRegistersFunction, It.IsAny<ushort>(), It.IsAny<ushort>(), It.IsAny<ushort>(), It.IsAny<ICollection<ushort>>(), TestContext.Current.CancellationToken))
            .Throws(TestException);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modbusClient.ReadWriteMultipleRegistersAsync(StartingAddress, 5, StartingAddress, [0x0001], TestContext.Current.CancellationToken).AsTask());

        exception.ShouldBe(TestException);
    }

    #endregion

    #region Read FIFO Queue

    [Fact]
    public async Task ReadFifoQueueAsync_Should_Return_Correct_Values()
    {
        var expectedResult = new List<ushort>(4);
        Mock.Get(_modBusCommand)
            .Setup(x => x.ReadFifoUshortValuesAsync(ReadFifoQueueFunction, StartingAddress, TestContext.Current.CancellationToken))
            .ReturnsAsync(expectedResult);

        var result = await _modbusClient.ReadFifoQueueAsync(StartingAddress, TestContext.Current.CancellationToken);

        result.ShouldBeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task ReadFifoQueueAsync_Should_Throw_ModBusClientException()
    {
        var endPoint = Mock.Of<EndPoint>();
        Mock.Get(_modBusCommand)
            .Setup(x => x.ReadFifoUshortValuesAsync(ReadFifoQueueFunction, It.IsAny<ushort>(), TestContext.Current.CancellationToken))
            .Throws(TestException);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modbusClient.ReadFifoQueueAsync(StartingAddress, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldBe(TestException);
    }

    #endregion

    #region Read Device Identifier

    [Fact]
    public async Task ReadDeviceIdentifierAsync_Should_Return_Correct_Values()
    {
        const byte objectId = 1;
        const byte nextObjectId = 3;
        var values1 = new List<DeviceObject> { new DeviceObject(objectId, [0x01, 0x02]), new DeviceObject(objectId + 1, [0x03, 0x04]) };
        Mock.Get(_modBusCommand)
            .Setup(x => x.ReadDeviceIdentifiersAsync(ReadDeviceIdentifierFunction, (byte)ReadDeviceIdentifier.Basic, objectId, TestContext.Current.CancellationToken))
            .ReturnsAsync(new DeviceIdentifierResult(true, nextObjectId, values1));
        var values2 = new List<DeviceObject> { new DeviceObject(nextObjectId, [0x81, 0x82]), new DeviceObject(nextObjectId + 1, [0x83, 0x84]) };
        Mock.Get(_modBusCommand)
            .Setup(x => x.ReadDeviceIdentifiersAsync(ReadDeviceIdentifierFunction, (byte)ReadDeviceIdentifier.Basic, nextObjectId, TestContext.Current.CancellationToken))
            .ReturnsAsync(new DeviceIdentifierResult(false, 0, values2));

        var result = await _modbusClient.ReadDeviceIdentifierAsync(ReadDeviceIdentifier.Basic, objectId, TestContext.Current.CancellationToken);

        result.ShouldBeEquivalentTo(values1.Concat(values2).ToList());
    }

    [Fact]
    public async Task ReadDeviceIdentifierAsync_Should_Throw_ModBusClientException()
    {
        var endPoint = Mock.Of<EndPoint>();
        Mock.Get(_modBusCommand)
            .Setup(x => x.ReadDeviceIdentifiersAsync(ReadDeviceIdentifierFunction, It.IsAny<byte>(), It.IsAny<byte>(), TestContext.Current.CancellationToken))
            .Throws(TestException);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modbusClient.ReadDeviceIdentifierAsync(ReadDeviceIdentifier.Basic, 1, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldBe(TestException);
    }

    #endregion

    #region Close

    [Fact]
    public void Close_Should_Call_Close()
    {
        _modbusClient.Close();

        Mock.Get(_modBusCommand)
            .Verify(x => x.Close(), Times.Once);
    }

    #endregion
}
