using Moq;
using Shouldly;
using System.Net;
using Wyrm.ModBusClient.Connection;

namespace Wyrm.ModBusClient.UnitTests.Connection;

public class ModBusCommandTests
{
    private readonly IModBusCommand _modBusCommand;

    private readonly IModBusConnection _modBusConnection = Mock.Of<IModBusConnection>();

    public ModBusCommandTests()
    {
        _modBusCommand = new ModBusCommand(_modBusConnection);
    }

    private const byte FunctionNumber = 3;
    private const ushort StartingAddress = 0x0123;
    private const ushort StartingAddressWrite = 0x0234;

    #region Unit Identifier

    [Fact]
    public void UnitIdentifier_Should_Get_What_Is_Set()
    {
        const byte unitIdentifier = 6;
        byte setUnitIdenifier = 0;
        Mock.Get(_modBusConnection)
            .SetupSet(x => x.UnitIdentifier = It.IsAny<byte>())
            .Callback<byte>(ui => setUnitIdenifier = ui);
        Mock.Get(_modBusConnection)
            .SetupGet(x => x.UnitIdentifier)
            .Returns(() => setUnitIdenifier);

        _modBusCommand.UnitIdentifier = unitIdentifier;

        _modBusCommand.UnitIdentifier.ShouldBe(unitIdentifier);
    }

    #endregion

    #region Connect

    [Fact]
    public async Task ConnectAsync_Should_Call_ConnectAsync()
    {
        var endPoint = Mock.Of<EndPoint>();

        await _modBusCommand.ConnectAsync(endPoint, TestContext.Current.CancellationToken);

        Mock.Get(_modBusConnection)
            .Verify(x => x.ConnectAsync(endPoint, TestContext.Current.CancellationToken), Times.Once);
    }

    #endregion

    #region Read Bit Values

    [Fact]
    public async Task ReadBitValuesAsync_Should_Throw_ModBusClientException_If_Too_Many_Values_Requested()
    {
        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modBusCommand.ReadBitValuesAsync(FunctionNumber, StartingAddress, 2001, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldNotBeNull().ExceptionCode.ShouldBe(ModBusExceptionCode.TooManyBitValues);
    }

    [Fact]
    public async Task ReadBitValuesAsync_Should_Return_Correct_Response()
    {
        byte[] bitsResult = [2, 0xC5, 0x0A];
        const ushort bitsToRead = 12;
        Mock.Get(_modBusConnection)
            .Setup(x => x.PerformFunctionAsync(FunctionNumber, It.Is<ushort[]>(a => a.SequenceEqual(new ushort[] { StartingAddress, bitsToRead })), It.Is<byte[]>(a => a.Length == 0), TestContext.Current.CancellationToken))
            .ReturnsAsync(bitsResult);

        var result = await _modBusCommand.ReadBitValuesAsync(FunctionNumber, StartingAddress, bitsToRead, TestContext.Current.CancellationToken);

        result.ShouldBeEquivalentTo(new List<bool> { true, false, true, false, false, false, true, true, false, true, false, true });
    }

    #endregion

    #region Read Ushort Values

    [Fact]
    public async Task ReadUshortValuesAsync_Should_Throw_ModBusClientException_If_Too_Many_Values_Requested()
    {
        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modBusCommand.ReadUshortValuesAsync(FunctionNumber, StartingAddress, 126, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldNotBeNull().ExceptionCode.ShouldBe(ModBusExceptionCode.TooManyUshortValues);
    }

    [Fact]
    public async Task ReadUshortValuesAsync_Should_Return_Correct_Response()
    {
        byte[] ushortsResult = [8, 0xC5, 0x0A, 0x00, 0x01, 0x34, 0x56, 0xFF, 0xFF];
        const ushort ushortsToRead = 4;
        Mock.Get(_modBusConnection)
            .Setup(x => x.PerformFunctionAsync(FunctionNumber, It.Is<ushort[]>(a => a.SequenceEqual(new ushort[] { StartingAddress, ushortsToRead })), It.Is<byte[]>(a => a.Length == 0), TestContext.Current.CancellationToken))
            .ReturnsAsync(ushortsResult);

        var result = await _modBusCommand.ReadUshortValuesAsync(FunctionNumber, StartingAddress, ushortsToRead, TestContext.Current.CancellationToken);

        result.ShouldBeEquivalentTo(new List<ushort> { 0xC50A, 0x0001, 0x3456, 0xFFFF });
    }

    #endregion

    #region Write Bit Values

    [Fact]
    public async Task WriteBitValuesAsync_Should_Throw_ModBusClientException_If_Too_Many_Values()
    {
        var values = Enumerable.Range(1, 2001).Select(i => i % 2 == 0).ToArray();

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modBusCommand.WriteBitValuesAsync(FunctionNumber, StartingAddressWrite, values, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldNotBeNull().ExceptionCode.ShouldBe(ModBusExceptionCode.TooManyBitValues);
    }

    [Fact]
    public async Task WriteBitValuesAsync_Should_Write_Correct_Values()
    {
        bool[] values = [false, true, false, true, false, false, true, true, false, true];

        var result = _modBusCommand.WriteBitValuesAsync(FunctionNumber, StartingAddressWrite, values, TestContext.Current.CancellationToken);

        Mock.Get(_modBusConnection)
            .Verify(x => x.PerformFunctionAsync(FunctionNumber, It.Is<ushort[]>(a => a.SequenceEqual(new ushort[] { StartingAddressWrite, (ushort)values.Length })), It.Is<byte[]>(a => a.SequenceEqual(new byte[] { 2, 0xCA, 0x02 })), TestContext.Current.CancellationToken), Times.Once);
    }

    #endregion

    #region Write Ushort Values

    [Fact]
    public async Task WriteUshortValuesAsync_Should_Throw_ModBusClientException_If_Too_Many_Values()
    {
        var values = Enumerable.Range(1, 126).Select(i => (ushort)i).ToArray();

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modBusCommand.WriteUshortValuesAsync(FunctionNumber, StartingAddressWrite, values, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldNotBeNull().ExceptionCode.ShouldBe(ModBusExceptionCode.TooManyUshortValues);
    }

    [Fact]
    public async Task WriteUshortValuesAsync_Should_Write_Correct_Values()
    {
        ushort[] values = [0xCA82, 0x0001, 0x8054, 0xFFFF];

        var result = _modBusCommand.WriteUshortValuesAsync(FunctionNumber, StartingAddressWrite, values, TestContext.Current.CancellationToken);

        Mock.Get(_modBusConnection)
            .Verify(x => x.PerformFunctionAsync(FunctionNumber, It.Is<ushort[]>(a => a.SequenceEqual(new ushort[] { StartingAddressWrite, (ushort)values.Length })), It.Is<byte[]>(a => a.SequenceEqual(new byte[] { 8, 0xCA, 0x82, 0x00, 0x01, 0x80, 0x54, 0xFF, 0xFF })), TestContext.Current.CancellationToken), Times.Once);
    }

    #endregion

    #region Read Write Ushort Values

    [Fact]
    public async Task ReadWriteUshortValuesAsync_Should_Throw_ModBusClientException_If_Too_Many_Values_Requested()
    {
        ushort[] values = [0xCA82, 0x0001, 0x8054, 0xFFFF];

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modBusCommand.ReadWriteUshortValuesAsync(FunctionNumber, StartingAddress, 126, StartingAddressWrite, values, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldNotBeNull().ExceptionCode.ShouldBe(ModBusExceptionCode.TooManyUshortValues);
    }

    [Fact]
    public async Task ReadWriteUshortValuesAsync_Should_Throw_ModBusClientException_If_Too_Many_Values()
    {
        ushort[] values = Enumerable.Range(1, 122).Select(i => (ushort)i).ToArray();

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modBusCommand.ReadWriteUshortValuesAsync(FunctionNumber, StartingAddress, 4, StartingAddressWrite, values, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldNotBeNull().ExceptionCode.ShouldBe(ModBusExceptionCode.TooManyUshortValues);
    }

    [Fact]
    public async Task ReadWriteUshortValuesAsync_Should_Return_Correct_Response()
    {
        ushort[] values = [0xCA82, 0x0001, 0x8054, 0xFFFF];
        byte[] ushortsResult = [8, 0xC5, 0x0A, 0x00, 0x01, 0x34, 0x56, 0xFF, 0xFF];
        const ushort ushortsToRead = 4;
        Mock.Get(_modBusConnection)
            .Setup(x => x.PerformFunctionAsync(FunctionNumber, It.Is<ushort[]>(a => a.SequenceEqual(new ushort[] { StartingAddress, ushortsToRead, StartingAddressWrite, (ushort)values.Length })), It.Is<byte[]>(a => a.SequenceEqual(new byte[] { 8, 0xCA, 0x82, 0x00, 0x01, 0x80, 0x54, 0xFF, 0xFF })), TestContext.Current.CancellationToken))
            .ReturnsAsync(ushortsResult);

        var result = await _modBusCommand.ReadWriteUshortValuesAsync(FunctionNumber, StartingAddress, ushortsToRead, StartingAddressWrite, values, TestContext.Current.CancellationToken);

        result.ShouldBeEquivalentTo(new List<ushort> { 0xC50A, 0x0001, 0x3456, 0xFFFF });
    }

    #endregion

    #region Read FIFO Values

    [Fact]
    public async Task ReadFifoUshortValuesAsync_Should_Return_Correct_Values()
    {
        byte[] ushortsToRead = [ 0, 8, 0, 3, 0xCB, 0x82, 0x00, 0x01, 0x40, 0x23];
        Mock.Get(_modBusConnection)
            .Setup(x => x.PerformFunctionAsync(FunctionNumber, It.Is<ushort[]>(a => a.SequenceEqual(new ushort[] { StartingAddress })), It.Is<byte[]>(a => a.Length == 0), TestContext.Current.CancellationToken))
            .ReturnsAsync(ushortsToRead);

        var result = await _modBusCommand.ReadFifoUshortValuesAsync(FunctionNumber, StartingAddress, TestContext.Current.CancellationToken);

        result.ShouldBeEquivalentTo(new List<ushort> { 0xCB82, 0x0001, 0x4023 });
    }

    #endregion

    #region Read File Records

    [Theory]
    [InlineData(1, 125)]
    [InlineData(2, 62)]
    [InlineData(3, 41)]
    [InlineData(63, 1)]
    public async Task ReadFileRecordsAsync_Should_Throw_ModBusClientException_If_Too_Many_Values_Requested(int files, ushort qty)
    {
        ICollection<FileRecords> fileRecordsToRead = Enumerable.Range(1, files).Select(i => new FileRecords((ushort)i, StartingAddress, qty)).ToArray();

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modBusCommand.ReadFileRecordsAsync(FunctionNumber, fileRecordsToRead, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldNotBeNull().ExceptionCode.ShouldBe(ModBusExceptionCode.TooManyUshortValues);
    }

    [Fact]
    public async Task ReadFileRecordsAsync_Should_Return_Correct_Values()
    {
        ICollection<FileRecords> fileRecordsToRead = [ new FileRecords(1, StartingAddress, 2), new FileRecords(2, StartingAddressWrite, 3) ];
        byte[] expectedByteData = [14, 6, 0, 1, 0x01, 0x23, 0, 2, 6, 0, 2, 0x02, 0x34, 0, 3 ];
        byte[] resultBytes = [14, 5, 6, 0xCD, 0x23, 0x80, 0x67, 7, 6, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC];
        Mock.Get(_modBusConnection)
            .Setup(x => x.PerformFunctionAsync(FunctionNumber, It.Is<ushort[]>(a => a.Length == 0), It.Is<byte[]>(a => a.SequenceEqual(expectedByteData)), TestContext.Current.CancellationToken))
            .ReturnsAsync(resultBytes);

        var result = await _modBusCommand.ReadFileRecordsAsync(FunctionNumber, fileRecordsToRead, TestContext.Current.CancellationToken);

        result.ShouldBeEquivalentTo(new List<ICollection<ushort>> { new List<ushort> { 0xCD23, 0x8067 }, new List<ushort> { 0x1234, 0x5678, 0x9ABC } });
    }

    [Fact]
    public async Task ReadFileRecordsAsync_Should_Throw_ModBusClientException_If_Bad_FileReferenceType()
    {
        ICollection<FileRecords> fileRecordsToRead = [new FileRecords(1, StartingAddress, 1)];
        byte[] expectedByteData = [7, 6, 0, 1, 0x01, 0x23, 0, 1];
        byte[] resultBytes = [4, 3, 5, 0xCD, 0x23];
        Mock.Get(_modBusConnection)
            .Setup(x => x.PerformFunctionAsync(FunctionNumber, It.Is<ushort[]>(a => a.Length == 0), It.Is<byte[]>(a => a.SequenceEqual(expectedByteData)), TestContext.Current.CancellationToken))
            .ReturnsAsync(resultBytes);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modBusCommand.ReadFileRecordsAsync(FunctionNumber, fileRecordsToRead, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldNotBeNull().ExceptionCode.ShouldBe(ModBusExceptionCode.BadFileReferenceType);
    }

    #endregion

    #region Write File Records

    [Theory]
    [InlineData(1, 125)]
    [InlineData(2, 62)]
    [InlineData(3, 41)]
    [InlineData(62, 2)]
    public async Task WriteFileRecordsAsync_Should_Throw_ModBusClientException_If_Too_Many_Values(int files, ushort qty)
    {
        ICollection<FileRecordsData> fileRecordsToWrite = Enumerable.Range(1, files).Select(i => new FileRecordsData((ushort)i, StartingAddress, new ushort[qty])).ToArray();

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modBusCommand.WriteFileRecordsAsync(FunctionNumber, fileRecordsToWrite, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldNotBeNull().ExceptionCode.ShouldBe(ModBusExceptionCode.TooManyUshortValues);
    }

    [Fact]
    public async Task WriteFileRecordsAsync_Should_Write_Correct_Values()
    {
        ICollection<FileRecordsData> values = [ new FileRecordsData(1, StartingAddress, [0x0123, 0xCDA5]), new FileRecordsData(2, StartingAddressWrite, [ 0xFFFF, 0x8032, 0xA533 ]) ];
        byte[] expectedBytes = [ 24, 6, 0, 1, 0x01, 0x23, 0, 2, 0x01, 0x23, 0xCD, 0xA5, 6, 0, 2, 0x02, 0x34, 0, 3, 0xFF, 0xFF, 0x80, 0x32, 0xA5, 0x33 ];

        await _modBusCommand.WriteFileRecordsAsync(FunctionNumber, values, TestContext.Current.CancellationToken);

        Mock.Get(_modBusConnection)
            .Verify(x => x.PerformFunctionAsync(FunctionNumber, It.Is<ushort[]>(a => a.Length == 0), It.Is<byte[]>(a => a.SequenceEqual(expectedBytes)), TestContext.Current.CancellationToken), Times.Once);
    }

    #endregion

    #region Read Device Identifiers

    [Fact]
    public async Task ReadDeviceIdentifiersAsync_Should_Return_Correct_Values()
    {
        const byte meiType = 14;
        const byte readDeviceId = (byte)ReadDeviceIdentifier.Basic;
        const byte objectId = 2;
        const byte nextObjectId = objectId + 2;
        byte[] expectedReadBytes = [meiType, readDeviceId, objectId];
        byte[] identifierData = [meiType, readDeviceId, 0x01, 0xFF, nextObjectId, 2, objectId, 3, (byte)'A', (byte)'B', (byte)'C', objectId + 1, 2, (byte)'V', (byte)'1' ];
        var value1 = new ReadOnlySpan<byte>(identifierData).Slice(8, 3).ToArray();
        var value2 = new ReadOnlySpan<byte>(identifierData).Slice(13, 2).ToArray();

        Mock.Get(_modBusConnection)
            .Setup(x => x.PerformFunctionAsync(FunctionNumber, It.Is<ushort[]>(a => a.Length == 0), It.Is<byte[]>(a => a.SequenceEqual(expectedReadBytes)), TestContext.Current.CancellationToken))
            .ReturnsAsync(identifierData);

        var result = await _modBusCommand.ReadDeviceIdentifiersAsync(FunctionNumber, readDeviceId, objectId, TestContext.Current.CancellationToken);

        result.ShouldBeEquivalentTo(new DeviceIdentifierResult(true, nextObjectId, new List<DeviceObject> { new DeviceObject(objectId, value1), new DeviceObject(objectId + 1, value2) }));
    }

    [Fact]
    public async Task ReadDeviceIdentifiersAsync_Should_Throw_ModBusClientException_If_MeiType_Mismatches()
    {
        const byte readDeviceId = (byte)ReadDeviceIdentifier.Basic;
        const byte objectId = 2;
        byte[] identifierData = [15, readDeviceId, 0x01, 0x00, 0, 1, objectId, 3, (byte)'A', (byte)'B', (byte)'C'];

        Mock.Get(_modBusConnection)
            .Setup(x => x.PerformFunctionAsync(FunctionNumber, It.IsAny<ushort[]>(), It.IsAny<byte[]>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(identifierData);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modBusCommand.ReadDeviceIdentifiersAsync(FunctionNumber, readDeviceId, objectId, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldNotBeNull().ExceptionCode.ShouldBe(ModBusExceptionCode.IncorrectMeiType);
    }

    [Fact]
    public async Task ReadDeviceIdentifiersAsync_Should_Throw_ModBusClientException_If_ReadDeviceId_Mismatches()
    {
        const byte readDeviceId = (byte)ReadDeviceIdentifier.Basic;
        const byte objectId = 2;
        byte[] identifierData = [14, (byte)ReadDeviceIdentifier.Regular, 0x01, 0x00, 0, 1, objectId, 3, (byte)'A', (byte)'B', (byte)'C'];

        Mock.Get(_modBusConnection)
            .Setup(x => x.PerformFunctionAsync(FunctionNumber, It.IsAny<ushort[]>(), It.IsAny<byte[]>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(identifierData);

        var exception = await Should.ThrowAsync<ModBusClientException>(() => _modBusCommand.ReadDeviceIdentifiersAsync(FunctionNumber, readDeviceId, objectId, TestContext.Current.CancellationToken).AsTask());

        exception.ShouldNotBeNull().ExceptionCode.ShouldBe(ModBusExceptionCode.IncorrectReadDeviceId);
    }

    #endregion

    #region Perform Function

    [Fact]
    public async Task PerformFunctionAsync_Should_Call_PerformFunctionAsync()
    {
        var expectedReturn = new ReadOnlyMemory<byte>();
        Mock.Get(_modBusConnection)
            .Setup(x => x.PerformFunctionAsync(FunctionNumber, new ushort[] { StartingAddress }, It.Is<byte[]>(a => a.Length == 0), TestContext.Current.CancellationToken))
            .ReturnsAsync(expectedReturn);

        var result = await _modBusCommand.PerformFunctionAsync(FunctionNumber, [StartingAddress], TestContext.Current.CancellationToken);

        result.ShouldBe(expectedReturn);
    }

    [Fact]
    public async Task PerformFunctionAsync_Should_Call_PerformFunctionAsync_With_Bytes()
    {
        const byte byteVal = 0x12;
        var expectedReturn = new ReadOnlyMemory<byte>();
        Mock.Get(_modBusConnection)
            .Setup(x => x.PerformFunctionAsync(FunctionNumber, new ushort[] { StartingAddress }, new byte[] { byteVal }, TestContext.Current.CancellationToken))
            .ReturnsAsync(expectedReturn);

        var result = await _modBusCommand.PerformFunctionAsync(FunctionNumber, [StartingAddress], [byteVal], TestContext.Current.CancellationToken);

        result.ShouldBe(expectedReturn);
    }

    #endregion

    #region Close

    [Fact]
    public void Close_Should_Call_Close()
    {
        _modBusCommand.Close();

        Mock.Get(_modBusConnection)
            .Verify(x => x.Close(), Times.Once);
    }

    #endregion
}
