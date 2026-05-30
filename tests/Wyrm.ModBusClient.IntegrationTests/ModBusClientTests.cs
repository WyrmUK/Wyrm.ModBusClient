using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System.Net;
using System.Text;
using Wyrm.ModBusClient.Connection;
using Wyrm.ModBusClient.IntegrationTests.Fakes;
using Wyrm.ModBusClient.Socket;

namespace Wyrm.ModBusClient.IntegrationTests;

public class ModBusClientTests
{
    #region Setup

    private readonly IModBusClient _modbusClient;
    private readonly FakeSocket _socketWrapper = new();
    private readonly ISocketFactory _socketFactory = Mock.Of<ISocketFactory>();
    private readonly ILogger<ModBusClient> _logger = Mock.Of<ILogger<ModBusClient>>();

    private static readonly EndPoint TestEndPoint = new IPEndPoint(new IPAddress([10, 0, 0, 1]), 8000);

    public ModBusClientTests()
    {
        _modbusClient = new ModBusClient(
            new ModBusCommand(
                new ModBusConnection(
                    new ModBusSocketFactory(
                        _socketFactory))),
            _logger);
        InitialiseSocketFactoryMock();
    }

    private void InitialiseSocketFactoryMock()
    {
        Mock.Get(_socketFactory)
            .Setup(sf => sf.CreateSocket(It.IsAny<EndPoint>()))
            .Returns(_socketWrapper);
    }

    #endregion

    #region Read Coils

    public static readonly TheoryData<byte, ushort, ushort, byte[], byte[], ICollection<bool>> ReadCoilsRequestsResponses = new()
    {
        { 1, 19, 19, [ 0, 1, 0, 0, 0, 6, 1, 1, 0, 19, 0, 19 ], [ 0, 1, 0, 0, 0, 6, 1, 1, 3, 0xCD, 0x6B, 0x05 ], [ true, false, true, true, false, false, true, true, true, true, false, true, false, true, true, false, true, false, true ] },
        { 2, 5, 1, [ 0, 1, 0, 0, 0, 6, 2, 1, 0, 5, 0, 1 ], [ 0, 1, 0, 0, 0, 4, 2, 1, 1, 0x01 ], [ true ] }
    };

    [Theory, MemberData(nameof(ReadCoilsRequestsResponses))]
    public async Task ReadCoilsAsync_Should_Return_Correct_Data(byte unitIdentifier, ushort startingCoil, ushort coilsToRead, byte[] sent, byte[] read, ICollection<bool> expected)
    {
        _socketWrapper.SendReceiveData = [(sent, read)];

        _modbusClient.UnitIdentifier = unitIdentifier;
        await _modbusClient.ConnectAsync(TestEndPoint, TestContext.Current.CancellationToken);
        var result = await _modbusClient.ReadCoilsAsync(startingCoil, coilsToRead, TestContext.Current.CancellationToken);
        _modbusClient.Close();

        result.ShouldBeEquivalentTo(expected);
    }

    #endregion

    #region Read Discrete Inputs

    public static readonly TheoryData<byte, ushort, ushort, byte[], byte[], ICollection<bool>> ReadDiscreteInputsRequestsResponses = new()
    {
        { 1, 196, 22, [ 0, 1, 0, 0, 0, 6, 1, 2, 0, 196, 0, 22 ], [ 0, 1, 0, 0, 0, 6, 1, 2, 3, 0xAC, 0xDB, 0x35 ], [ false, false, true, true, false, true, false, true, true, true, false, true, true, false, true, true, true, false, true, false, true, true ] },
        { 2, 5, 1, [ 0, 1, 0, 0, 0, 6, 2, 2, 0, 5, 0, 1 ], [ 0, 1, 0, 0, 0, 4, 2, 2, 1, 0x01 ], [ true ] }
    };

    [Theory, MemberData(nameof(ReadDiscreteInputsRequestsResponses))]
    public async Task ReadDiscreteInputsAsync_Should_Return_Correct_Data(byte unitIdentifier, ushort startingInput, ushort inputsToRead, byte[] sent, byte[] read, ICollection<bool> expected)
    {
        _socketWrapper.SendReceiveData = [(sent, read)];

        _modbusClient.UnitIdentifier = unitIdentifier;
        await _modbusClient.ConnectAsync(TestEndPoint, TestContext.Current.CancellationToken);
        var result = await _modbusClient.ReadDiscreteInputsAsync(startingInput, inputsToRead, TestContext.Current.CancellationToken);
        _modbusClient.Close();

        result.ShouldBeEquivalentTo(expected);
    }

    #endregion

    #region Read Holding Registers

    public static readonly TheoryData<byte, ushort, ushort, byte[], byte[], ICollection<ushort>> ReadHoldingRegistersRequestsResponses = new()
    {
        { 1, 107, 3, [ 0, 1, 0, 0, 0, 6, 1, 3, 0, 107, 0, 3 ], [ 0, 1, 0, 0, 0, 9, 1, 3, 6, 0x02, 0x2B, 0x00, 0x00, 0x00, 0x64 ], [ 0x022B, 0x0000, 0x0064 ] },
        { 2, 5, 1, [ 0, 1, 0, 0, 0, 6, 2, 3, 0, 5, 0, 1 ], [ 0, 1, 0, 0, 0, 5, 2, 3, 2, 0x03, 0xAC ], [ 0x03AC ] }
    };

    [Theory, MemberData(nameof(ReadHoldingRegistersRequestsResponses))]
    public async Task ReadHoldingRegistersAsync_Should_Return_Correct_Data(byte unitIdentifier, ushort startingRegister, ushort registersToRead, byte[] sent, byte[] read, ICollection<ushort> expected)
    {
        _socketWrapper.SendReceiveData = [(sent, read)];

        _modbusClient.UnitIdentifier = unitIdentifier;
        await _modbusClient.ConnectAsync(TestEndPoint, TestContext.Current.CancellationToken);
        var result = await _modbusClient.ReadHoldingRegistersAsync(startingRegister, registersToRead, TestContext.Current.CancellationToken);
        _modbusClient.Close();

        result.ShouldBeEquivalentTo(expected);
    }

    #endregion

    #region Read Input Registers

    public static readonly TheoryData<byte, ushort, ushort, byte[], byte[], ICollection<ushort>> ReadInputRegistersRequestsResponses = new()
    {
        { 1, 8, 2, [ 0, 1, 0, 0, 0, 6, 1, 4, 0, 8, 0, 2 ], [ 0, 1, 0, 0, 0, 7, 1, 4, 4, 0x12, 0x2C, 0xC0, 0x44 ], [ 0x122C, 0xC044 ] },
        { 2, 5, 1, [ 0, 1, 0, 0, 0, 6, 2, 4, 0, 5, 0, 1 ], [ 0, 1, 0, 0, 0, 5, 2, 4, 2, 0x15, 0x9D ], [ 0x159D ] }
    };

    [Theory, MemberData(nameof(ReadInputRegistersRequestsResponses))]
    public async Task ReadInputRegistersAsync_Should_Return_Correct_Data(byte unitIdentifier, ushort startingRegister, ushort registersToRead, byte[] sent, byte[] read, ICollection<ushort> expected)
    {
        _socketWrapper.SendReceiveData = [(sent, read)];

        _modbusClient.UnitIdentifier = unitIdentifier;
        await _modbusClient.ConnectAsync(TestEndPoint, TestContext.Current.CancellationToken);
        var result = await _modbusClient.ReadInputRegistersAsync(startingRegister, registersToRead, TestContext.Current.CancellationToken);
        _modbusClient.Close();

        result.ShouldBeEquivalentTo(expected);
    }

    #endregion

    #region Write Single Coil

    public static readonly TheoryData<byte, ushort, bool, byte[], byte[]> WriteSingleCoilRequestsResponses = new()
    {
        { 1, 172, true, [ 0, 1, 0, 0, 0, 6, 1, 5, 0, 172, 255, 0 ], [ 0, 1, 0, 0, 0, 6, 1, 5, 0, 172, 255, 0 ] },
        { 2, 5, false, [ 0, 1, 0, 0, 0, 6, 2, 5, 0, 5, 0, 0 ], [ 0, 1, 0, 0, 0, 6, 2, 5, 0, 5, 0, 0 ] }
    };

    [Theory, MemberData(nameof(WriteSingleCoilRequestsResponses))]
    public async Task WriteSingleCoilAsync_Should_Return_Correct_Data(byte unitIdentifier, ushort coilToWrite, bool value, byte[] sent, byte[] read)
    {
        _socketWrapper.SendReceiveData = [(sent, read)];

        _modbusClient.UnitIdentifier = unitIdentifier;
        await _modbusClient.ConnectAsync(TestEndPoint, TestContext.Current.CancellationToken);
        await _modbusClient.WriteSingleCoilAsync(coilToWrite, value, TestContext.Current.CancellationToken);
        _modbusClient.Close();
    }

    #endregion

    #region Write Single Register

    public static readonly TheoryData<byte, ushort, ushort, byte[], byte[]> WriteSingleRegisterRequestsResponses = new()
    {
        { 1, 1, 3, [ 0, 1, 0, 0, 0, 6, 1, 6, 0, 1, 0, 3 ], [ 0, 1, 0, 0, 0, 6, 1, 6, 0, 1, 0, 3 ] },
        { 2, 5, 256, [ 0, 1, 0, 0, 0, 6, 2, 6, 0, 5, 1, 0 ], [ 0, 1, 0, 0, 0, 6, 2, 6, 0, 5, 1, 0 ] }
    };

    [Theory, MemberData(nameof(WriteSingleRegisterRequestsResponses))]
    public async Task WriteSingleRegisterAsync_Should_Return_Correct_Data(byte unitIdentifier, ushort registerToWrite, ushort value, byte[] sent, byte[] read)
    {
        _socketWrapper.SendReceiveData = [(sent, read)];

        _modbusClient.UnitIdentifier = unitIdentifier;
        await _modbusClient.ConnectAsync(TestEndPoint, TestContext.Current.CancellationToken);
        await _modbusClient.WriteSingleRegisterAsync(registerToWrite, value, TestContext.Current.CancellationToken);
        _modbusClient.Close();
    }

    #endregion

    #region Write Multiple Coils

    public static readonly TheoryData<byte, ushort, ICollection<bool>, byte[], byte[]> WriteMultipleCoilsRequestsResponses = new()
    {
        { 1, 19, [ true, false, true, true, false, false, true, true, true, false ], [ 0, 1, 0, 0, 0, 9, 1, 15, 0, 19, 0, 10, 2, 0xCD, 0x01 ], [ 0, 1, 0, 0, 0, 6, 1, 15, 0, 19, 0, 10 ] },
        { 2, 5, [ true ], [ 0, 1, 0, 0, 0, 8, 2, 15, 0, 5, 0, 1, 1, 1 ], [ 0, 1, 0, 0, 0, 6, 2, 15, 0, 5, 0, 1 ] }
    };

    [Theory, MemberData(nameof(WriteMultipleCoilsRequestsResponses))]
    public async Task WriteMultipleCoilsAsync_Should_Return_Correct_Data(byte unitIdentifier, ushort startingCoil, ICollection<bool> values, byte[] sent, byte[] read)
    {
        _socketWrapper.SendReceiveData = [(sent, read)];

        _modbusClient.UnitIdentifier = unitIdentifier;
        await _modbusClient.ConnectAsync(TestEndPoint, TestContext.Current.CancellationToken);
        await _modbusClient.WriteMultipleCoilsAsync(startingCoil, values, TestContext.Current.CancellationToken);
        _modbusClient.Close();
    }

    #endregion

    #region Write Multiple Registers

    public static readonly TheoryData<byte, ushort, ICollection<ushort>, byte[], byte[]> WriteMultipleRegistersRequestsResponses = new()
    {
        { 1, 1, [ 0x000A, 0x0102 ], [ 0, 1, 0, 0, 0, 11, 1, 16, 0, 1, 0, 2, 4, 0x00, 0x0A, 0x01, 0x02 ], [ 0, 1, 0, 0, 0, 6, 1, 16, 0, 1, 0, 2 ] },
        { 2, 5, [ 0xCA41 ], [ 0, 1, 0, 0, 0, 9, 2, 16, 0, 5, 0, 1, 2, 0xCA, 0x41 ], [ 0, 1, 0, 0, 0, 6, 2, 16, 0, 5, 0, 1 ] }
    };

    [Theory, MemberData(nameof(WriteMultipleRegistersRequestsResponses))]
    public async Task WriteMultipleRegistersAsync_Should_Return_Correct_Data(byte unitIdentifier, ushort startingRegister, ICollection<ushort> values, byte[] sent, byte[] read)
    {
        _socketWrapper.SendReceiveData = [(sent, read)];

        _modbusClient.UnitIdentifier = unitIdentifier;
        await _modbusClient.ConnectAsync(TestEndPoint, TestContext.Current.CancellationToken);
        await _modbusClient.WriteMultipleRegistersAsync(startingRegister, values, TestContext.Current.CancellationToken);
        _modbusClient.Close();
    }

    #endregion

    #region Read File Record

    public static readonly TheoryData<byte, ICollection<FileRecords>, byte[], byte[], ICollection<ICollection<ushort>>> ReadFileRecordRequestsResponses = new()
    {
        { 1, [ new FileRecords(4, 1, 2), new FileRecords(3, 9, 2) ], [ 0, 1, 0, 0, 0, 17, 1, 20, 14, 6, 0, 4, 0, 1, 0, 2, 6, 0, 3, 0, 9, 0, 2 ], [ 0, 1, 0, 0, 0, 15, 1, 20, 12, 5, 6, 0x0D, 0xFE, 0x00, 0x20, 5, 6, 0x33, 0xCD, 0x00, 0x40 ], [ [ 0x0DFE, 0x0020 ], [ 0x33CD, 0x0040 ] ] },
        { 2, [ new FileRecords(5, 6, 1) ], [ 0, 1, 0, 0, 0, 10, 2, 20, 7, 6, 0, 5, 0, 6, 0, 1 ], [ 0, 1, 0, 0, 0, 7, 2, 20, 4, 3, 6, 0xF0, 0xA9 ], [ [ 0xF0A9 ] ] }
    };

    [Theory, MemberData(nameof(ReadFileRecordRequestsResponses))]
    public async Task ReadFileRecordAsync_Should_Return_Correct_Data(byte unitIdentifier, ICollection<FileRecords> fileRecordsToRead, byte[] sent, byte[] read, ICollection<ICollection<ushort>> expected)
    {
        _socketWrapper.SendReceiveData = [(sent, read)];

        _modbusClient.UnitIdentifier = unitIdentifier;
        await _modbusClient.ConnectAsync(TestEndPoint, TestContext.Current.CancellationToken);
        var result = await _modbusClient.ReadFileRecordAsync(fileRecordsToRead, TestContext.Current.CancellationToken);
        _modbusClient.Close();

        result.ShouldBeEquivalentTo(expected);
    }

    #endregion

    #region Write File Record

    public static readonly TheoryData<byte, ICollection<FileRecordsData>, byte[], byte[]> WriteFileRecordRequestsResponses = new()
    {
        { 1, [ new FileRecordsData(4, 7, [ 0x06AF, 0x04BE, 0x100D ]) ], [ 0, 1, 0, 0, 0, 16, 1, 21, 13, 6, 0, 4, 0, 7, 0, 3, 0x06, 0xAF, 0x04, 0xBE, 0x10, 0x0D ], [ 0, 1, 0, 0, 0, 16, 1, 21, 13, 6, 0, 4, 0, 7, 0, 3, 0x06, 0xAF, 0x04, 0xBE, 0x10, 0x0D ] },
        { 2, [ new FileRecordsData(5, 6, [ 0x80B4 ]) ], [ 0, 1, 0, 0, 0, 12, 2, 21, 9, 6, 0, 5, 0, 6, 0, 1, 0x80, 0xB4 ], [ 0, 1, 0, 0, 0, 12, 2, 21, 9, 6, 0, 5, 0, 6, 0, 1, 0x80, 0xB4 ] },
        { 3, [new FileRecordsData(4, 7, [0x06AF, 0x04BE, 0x100D]), new FileRecordsData(5, 6, [ 0x80B4 ]) ], [ 0, 1, 0, 0, 0, 25, 3, 21, 22, 6, 0, 4, 0, 7, 0, 3, 0x06, 0xAF, 0x04, 0xBE, 0x10, 0x0D, 6, 0, 5, 0, 6, 0, 1, 0x80, 0xB4 ], [ 0, 1, 0, 0, 0, 25, 3, 21, 22, 6, 0, 4, 0, 7, 0, 3, 0x06, 0xAF, 0x04, 0xBE, 0x10, 0x0D, 6, 0, 5, 0, 6, 0, 1, 0x80, 0xB4 ] }
    };

    [Theory, MemberData(nameof(WriteFileRecordRequestsResponses))]
    public async Task WriteFileRecordAsync_Should_Return_Correct_Data(byte unitIdentifier, ICollection<FileRecordsData> fileRecordsToWrite, byte[] sent, byte[] read)
    {
        _socketWrapper.SendReceiveData = [(sent, read)];

        _modbusClient.UnitIdentifier = unitIdentifier;
        await _modbusClient.ConnectAsync(TestEndPoint, TestContext.Current.CancellationToken);
        await _modbusClient.WriteFileRecordAsync(fileRecordsToWrite, TestContext.Current.CancellationToken);
        _modbusClient.Close();
    }

    #endregion

    #region Mask Write Register

    public static readonly TheoryData<byte, ushort, ushort, ushort, byte[], byte[]> MaskWriteRegisterRequestsResponses = new()
    {
        { 1, 4, 0x00F2, 0x0025, [ 0, 1, 0, 0, 0, 8, 1, 22, 0, 4, 0x00, 0xF2, 0x00, 0x25 ], [ 0, 1, 0, 0, 0, 8, 1, 22, 0, 4, 0x00, 0xF2, 0x00, 0x25 ] },
        { 2, 5, 0xC434, 0x8001, [ 0, 1, 0, 0, 0, 8, 2, 22, 0, 5, 0xC4, 0x34, 0x80, 0x01 ], [ 0, 1, 0, 0, 0, 8, 2, 22, 0, 5, 0xC4, 0x34, 0x80, 0x01] }
    };

    [Theory, MemberData(nameof(MaskWriteRegisterRequestsResponses))]
    public async Task MaskWriteRegisterAsync_Should_Return_Correct_Data(byte unitIdentifier, ushort registerToWrite, ushort andMask, ushort orMask, byte[] sent, byte[] read)
    {
        _socketWrapper.SendReceiveData = [(sent, read)];

        _modbusClient.UnitIdentifier = unitIdentifier;
        await _modbusClient.ConnectAsync(TestEndPoint, TestContext.Current.CancellationToken);
        await _modbusClient.MaskWriteRegisterAsync(registerToWrite, andMask, orMask, TestContext.Current.CancellationToken);
        _modbusClient.Close();
    }

    #endregion

    #region Read Write Multiple Registers

    public static readonly TheoryData<byte, ushort, ushort, ushort, ICollection<ushort>, byte[], byte[], ICollection<ushort>> ReadWriteMultipleRegistersRequestsResponses = new()
    {
        { 1, 3, 6, 14, [ 0x00FF, 0x00FF, 0x00FF ], [ 0, 1, 0, 0, 0, 17, 1, 23, 0, 3, 0, 6, 0, 14, 0, 3, 6, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF ], [ 0, 1, 0, 0, 0, 15, 1, 23, 12, 0x00, 0xFE, 0x0A, 0xCD, 0x00, 0x01, 0x00, 0x03, 0x00, 0x0D, 0x00, 0xFF ], [ 0x00FE, 0x0ACD, 0x0001, 0x0003, 0x000D, 0x00FF ] },
        { 2, 5, 1, 256, [ 0x85CD ], [ 0, 1, 0, 0, 0, 13, 2, 23, 0, 5, 0, 1, 1, 0, 0, 1, 2, 0x85, 0xCD ], [ 0, 1, 0, 0, 0, 5, 2, 23, 2, 0x03, 0xAC ], [ 0x03AC ] }
    };

    [Theory, MemberData(nameof(ReadWriteMultipleRegistersRequestsResponses))]
    public async Task ReadWriteMultipleRegistersAsync_Should_Return_Correct_Data(byte unitIdentifier, ushort startingReadRegister, ushort registersToRead, ushort startingWriteRegister, ICollection<ushort> values, byte[] sent, byte[] read, ICollection<ushort> expected)
    {
        _socketWrapper.SendReceiveData = [(sent, read)];

        _modbusClient.UnitIdentifier = unitIdentifier;
        await _modbusClient.ConnectAsync(TestEndPoint, TestContext.Current.CancellationToken);
        var result = await _modbusClient.ReadWriteMultipleRegistersAsync(startingReadRegister, registersToRead, startingWriteRegister, values, TestContext.Current.CancellationToken);
        _modbusClient.Close();

        result.ShouldBeEquivalentTo(expected);
    }

    #endregion

    #region Read FIFO Queue

    public static readonly TheoryData<byte, ushort, byte[], byte[], ICollection<ushort>> ReadFifoQueueRequestsResponses = new()
    {
        { 1, 1246, [ 0, 1, 0, 0, 0, 4, 1, 24, 4, 222 ], [ 0, 1, 0, 0, 0, 10, 1, 24, 0, 6, 0, 2, 0x01, 0xB8, 0x12, 0x84 ], [ 0x01B8, 0x1284 ] },
        { 2, 5, [ 0, 1, 0, 0, 0, 4, 2, 24, 0, 5 ], [ 0, 1, 0, 0, 0, 14, 2, 24, 0, 10, 0, 4, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 ], [ 0x0102, 0x0304, 0x0506, 0x0708 ] }
    };

    [Theory, MemberData(nameof(ReadFifoQueueRequestsResponses))]
    public async Task ReadFifoQueueAsync_Should_Return_Correct_Data(byte unitIdentifier, ushort fifoPointerAddress, byte[] sent, byte[] read, ICollection<ushort> expected)
    {
        _socketWrapper.SendReceiveData = [(sent, read)];

        _modbusClient.UnitIdentifier = unitIdentifier;
        await _modbusClient.ConnectAsync(TestEndPoint, TestContext.Current.CancellationToken);
        var result = await _modbusClient.ReadFifoQueueAsync(fifoPointerAddress, TestContext.Current.CancellationToken);
        _modbusClient.Close();

        result.ShouldBeEquivalentTo(expected);
    }

    #endregion

    #region Read Device Identification

    public static readonly byte[] CompanyIdentification = "Company identification".Select(c => (byte)c).ToArray();
    public static readonly byte[] CompanyIdentificationId = [ 0, (byte)CompanyIdentification.Length ];
    public static readonly byte[] ProductCode1 = "Product code".Select(c => (byte)c).ToArray();
    public static readonly byte[] ProductCode1Id = [1, (byte)ProductCode1.Length];
    public static readonly byte[] ProductCode2 = "Product code 123456789".Select(c => (byte)c).ToArray();
    public static readonly byte[] ProductCode2Id = [1, (byte)ProductCode2.Length];
    public static readonly byte[] VersionCode = "V2.11".Select(c => (byte)c).ToArray();
    public static readonly byte[] VersionCodeId = [2, (byte)VersionCode.Length];
    public static readonly byte[] RdiHeader1 = [ 0, 1, 0, 0, 0, (byte)(8 + 2 + CompanyIdentification.Length + 2 + ProductCode1.Length + 2 + VersionCode.Length), 1, 43, 14, 1, 1, 0, 0, 3 ];
    public static readonly byte[] RdiReceived1 = RdiHeader1
        .Concat(CompanyIdentificationId).Concat(CompanyIdentification)
        .Concat(ProductCode1Id).Concat(ProductCode1)
        .Concat(VersionCodeId).Concat(VersionCode)
        .ToArray();
    public static readonly byte[] RdiHeader21 = [0, 1, 0, 0, 0, (byte)(8 + 2 + CompanyIdentification.Length + 2 + ProductCode1.Length), 1, 43, 14, 1, 1, 255, 2, 3];
    public static readonly byte[] RdiHeader22 = [0, 2, 0, 0, 0, (byte)(8 + 2 + VersionCode.Length), 1, 43, 14, 1, 1, 0, 0, 3];
    public static readonly byte[] RdiReceived21 = RdiHeader21
        .Concat(CompanyIdentificationId).Concat(CompanyIdentification)
        .Concat(ProductCode2Id).Concat(ProductCode2)
        .ToArray();
    public static readonly byte[] RdiReceived22 = RdiHeader22
        .Concat(VersionCodeId).Concat(VersionCode)
        .ToArray();

    public static readonly TheoryData<byte, ReadDeviceIdentifier, byte, byte[][], byte[][], ICollection<DeviceObject>> ReadDeviceIdentifierRequestsResponses = new()
    {
        { 1, ReadDeviceIdentifier.Basic, 0, [ [ 0, 1, 0, 0, 0, 5, 1, 43, 14, 1, 0 ] ], [ RdiReceived1 ], [ new DeviceObject(0, CompanyIdentification), new DeviceObject(1, ProductCode1), new DeviceObject(2, VersionCode) ] },
        { 1, ReadDeviceIdentifier.Basic, 0, [ [ 0, 1, 0, 0, 0, 5, 1, 43, 14, 1, 0 ], [ 0, 2, 0, 0, 0, 5, 1, 43, 14, 1, 2 ] ], [ RdiReceived21, RdiReceived22 ], [ new DeviceObject(0, CompanyIdentification), new DeviceObject(1, ProductCode2), new DeviceObject(2, VersionCode) ] }
    };

    [Theory, MemberData(nameof(ReadDeviceIdentifierRequestsResponses))]
    public async Task ReadDeviceIdentifierAsync_Should_Return_Correct_Data(byte unitIdentifier, ReadDeviceIdentifier readDeviceId, byte objectId, byte[][] sent, byte[][] read, ICollection<DeviceObject> expected)
    {
        _socketWrapper.SendReceiveData = Enumerable.Range(0, sent.Length).Select(i => (sent[i], read[i])).ToArray();

        _modbusClient.UnitIdentifier = unitIdentifier;
        await _modbusClient.ConnectAsync(TestEndPoint, TestContext.Current.CancellationToken);
        var result = await _modbusClient.ReadDeviceIdentifierAsync(readDeviceId, objectId, TestContext.Current.CancellationToken);
        _modbusClient.Close();

        result.ShouldBeEquivalentTo(expected);
    }

    #endregion

    #region Read Holding Registers Request/Response

    private const byte ReadHoldingRegistersFunction = 3;

    [Theory, MemberData(nameof(ReadHoldingRegistersRequestsResponses))]
    public async Task ReadHoldingRegistersRequestAsync_Should_Return_Correct_Response_Data(byte unitIdentifier, ushort startingRegister, ushort registersToRead, byte[] sent, byte[] read, ICollection<ushort> expected)
    {
        _socketWrapper.SendReceiveData = [(sent, read)];

        _modbusClient.UnitIdentifier = unitIdentifier;
        await _modbusClient.ConnectAsync(TestEndPoint, TestContext.Current.CancellationToken);
        await _modbusClient.ReadHoldingRegistersRequestAsync(startingRegister, registersToRead, TestContext.Current.CancellationToken);
        var result = await _modbusClient.ReadRegistersResponseDataAsync(TestContext.Current.CancellationToken);
        _modbusClient.Close();

        result.ShouldBeEquivalentTo(new UshortDataResponse
        {
            TransactionId = 1,
            UnitIdentifier = unitIdentifier,
            FunctionNumber = ReadHoldingRegistersFunction,
            UshortData = expected
        });
    }

    #endregion

    #region Read Input Registers Request/Response

    private const byte ReadInputRegistersFunction = 4;

    [Theory, MemberData(nameof(ReadInputRegistersRequestsResponses))]
    public async Task ReadInputRegistersRequestAsync_Should_Return_Correct_Response_Data(byte unitIdentifier, ushort startingRegister, ushort registersToRead, byte[] sent, byte[] read, ICollection<ushort> expected)
    {
        _socketWrapper.SendReceiveData = [(sent, read)];

        _modbusClient.UnitIdentifier = unitIdentifier;
        await _modbusClient.ConnectAsync(TestEndPoint, TestContext.Current.CancellationToken);
        await _modbusClient.ReadInputRegistersRequestAsync(startingRegister, registersToRead, TestContext.Current.CancellationToken);
        var result = await _modbusClient.ReadRegistersResponseDataAsync(TestContext.Current.CancellationToken);
        _modbusClient.Close();

        result.ShouldBeEquivalentTo(new UshortDataResponse
        {
            TransactionId = 1,
            UnitIdentifier = unitIdentifier,
            FunctionNumber = ReadInputRegistersFunction,
            UshortData = expected
        });
    }

    #endregion

    #region Framer Deframer

    private const ushort ProtocolIdentifier = 0x0001;
    private const ushort TransactionId = 0x5959;
    private const byte GivUnitId = 0x01;
    private const byte GivFuncNo = 0x02;
    private static readonly byte[] Padding = new byte[16];
    private static readonly byte[] CheckSum = [0xf2, 0x8b];
    private const string WifiHost = "WH2311F150";
    private static readonly byte[] WifiHostBytes = Encoding.ASCII.GetBytes(WifiHost);
    private const string SerialNo = "FA2311F150";
    private static readonly byte[] SerialNoBytes = Encoding.ASCII.GetBytes(SerialNo);
    private string _wifiHost = string.Empty;
    private string _serialNo = string.Empty;

    public static readonly TheoryData<byte, ushort, ushort, byte[], byte[], ICollection<ushort>> ReadGivEnergyInputRegistersRequestsResponses = new()
    {
        { 17, 0, 60, [ TransactionId >> 8, TransactionId & 0xff, ProtocolIdentifier >> 8, ProtocolIdentifier & 0xff, 0, 28, GivUnitId, GivFuncNo, ..Padding, 0, 8, 17, 4, 0, 0, 0, 60, ..CheckSum ], [ TransactionId >> 8, TransactionId & 0xff, ProtocolIdentifier >> 8, ProtocolIdentifier & 0xff, 0, 158, GivUnitId, GivFuncNo, ..WifiHostBytes, 0, 0, 0, 0, 0, 0, 0, 138, 17, 4, ..SerialNoBytes, 0, 0, 0, 60, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x10, 0xb8, 0x00, 0x00, 0x09, 0x6f, 0x00, 0x00, 0xa8, 0xdc, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0xcb, 0xa1, 0x13, 0x92, 0x00, 0x02, 0x0b, 0x41, 0x14, 0x1e, 0x00, 0x8c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b, 0xc5, 0x00, 0x00, 0xff, 0xd2, 0x00, 0x51, 0x00, 0x48, 0x00, 0x00, 0x41, 0xbb, 0x00, 0x00, 0xfe, 0x90, 0x00, 0x00, 0x00, 0x03, 0x17, 0xf5, 0x00, 0x00, 0x00, 0x17, 0x00, 0x1a, 0x00, 0x19, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x90, 0x01, 0x42, 0x02, 0xf1, 0x00, 0x97, 0x00, 0x00, 0xdf, 0x1a, 0x00, 0x00, 0x61, 0xf5, 0x00, 0x01, 0x14, 0x50, 0x00, 0x00, 0x00, 0x00, 0x09, 0x6f, 0x13, 0x94, 0x01, 0x87, 0x01, 0x22, 0x00, 0x00, 0x01, 0x42, 0x00, 0x05, 0xd3, 0x72 ], [ 0x0001, 0x0000, 0x0000, 0x10b8, 0x0000, 0x096f, 0x0000, 0xa8dc, 0x0000, 0x0000, 0x0006, 0x0000, 0xcba1, 0x1392, 0x0002, 0x0b41, 0x141e, 0x008c, 0x0000, 0x0000, 0x0000, 0x0000, 0x3bc5, 0x0000, 0xffd2, 0x0051, 0x0048, 0x0000, 0x41bb, 0x0000, 0xfe90, 0x0000, 0x0003, 0x17f5, 0x0000, 0x0017, 0x001a, 0x0019, 0x0000, 0x0000, 0x0000, 0x0190, 0x0142, 0x02f1, 0x0097, 0x0000, 0xdf1a, 0x0000, 0x61f5, 0x0001, 0x1450, 0x0000, 0x0000, 0x096f, 0x1394, 0x0187, 0x0122, 0x0000, 0x0142, 0x0005 ] }
    };

    [Theory, MemberData(nameof(ReadGivEnergyInputRegistersRequestsResponses))]
    public async Task ReadGivEnergyInputRegistersAsync_Should_Return_Correct_Data(byte unitIdentifier, ushort startingRegister, ushort registersToRead, byte[] sent, byte[] read, ICollection<ushort> expected)
    {
        _socketWrapper.SendReceiveData = [(sent, read)];

        _modbusClient.ProtocolIdentifier = ProtocolIdentifier;
        _modbusClient.UnitIdentifier = unitIdentifier;
        _modbusClient.TransactionId = TransactionId;
        await _modbusClient.ConnectAsync(TestEndPoint, TestContext.Current.CancellationToken);
        _modbusClient.PduFramer = PduFramer;
        _modbusClient.PduDeframer = PduDeframer;
        var result = await _modbusClient.ReadInputRegistersAsync(startingRegister, registersToRead, TestContext.Current.CancellationToken);
        _modbusClient.Close();

        result.ShouldBeEquivalentTo(expected);
        _wifiHost.ShouldBe(WifiHost);
        _serialNo.ShouldBe(SerialNo);
    }

    private IList<byte> PduFramer(IList<byte> command)
    {
        var count = command.Count + 2;
        var givCommand = new List<byte>([ GivUnitId, GivFuncNo, ..Padding, (byte)(count >> 8), (byte)(count &0xff) ]);
        givCommand.AddRange(command);
        givCommand.AddRange(CheckSum);
        return givCommand;
    }

    private ReadOnlyMemory<byte> PduDeframer(ReadOnlyMemory<byte> givResponse)
    {
        _wifiHost = Encoding.ASCII.GetString([.. givResponse.Slice(2, 16).TrimEnd((byte)0).ToArray()]);
        _serialNo = Encoding.ASCII.GetString([.. givResponse.Slice(22, 10).TrimEnd((byte)0).ToArray()]);
        var response = new List<byte>();
        response.AddRange(givResponse.Slice(20, 2).Span);
        var bytes = givResponse.Span[35] * 2;
        response.Add((byte)bytes);
        response.AddRange(givResponse.Slice(36, bytes).Span);
        return new ReadOnlyMemory<byte>(response.ToArray());
    }

    #endregion
}
