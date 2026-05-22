using Microsoft.Extensions.Logging;
using System.Net;
using Wyrm.ModBusClient.Connection;

namespace Wyrm.ModBusClient;

internal sealed class ModBusClient(
    IModBusCommand _modBusCommand,
    ILogger<ModBusClient> _logger) : IModBusClient
{
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

    public ushort ProtocolIdentifier
    {
        get => _modBusCommand.ProtocolIdentifier;
        set => _modBusCommand.ProtocolIdentifier = value;
    }

    public byte UnitIdentifier
    {
        get => _modBusCommand.UnitIdentifier;
        set => _modBusCommand.UnitIdentifier = value;
    }

    public ushort TransactionId
    {
        get => _modBusCommand.TransactionId;
        set => _modBusCommand.TransactionId = value;
    }

    public Func<IList<byte>, IList<byte>>? PduFramer
    {
        get => _modBusCommand.PduFramer;
        set => _modBusCommand.PduFramer = value;
    }

    public Func<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>? PduDeframer
    {
        get => _modBusCommand.PduDeframer;
        set => _modBusCommand.PduDeframer = value;
    }

    public async ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("ModBus Client: Connecting to end point: {Address}", endPoint.Serialize().ToString());

        try
        {
            await _modBusCommand.ConnectAsync(endPoint, cancellationToken);

            _logger.LogInformation("ModBus Client: Connected to end point.");
        }
        catch (ModBusClientException ex)
        {
            _logger.LogError(ex, "ModBus Client: Exception while connecting to end point: {ExceptionCode}", ex.ExceptionCode);
            throw;
        }
    }

    public async ValueTask<ICollection<bool>> ReadCoilsAsync(ushort startingCoil, ushort coilsToRead, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("ModBus Client: Reading {NumberOfCoils} coils from: {CoilAddress}", coilsToRead, startingCoil);

        try
        {
            if (coilsToRead == 0)
            {
                _logger.LogInformation("ModBus Client: No values requested.");
                return [];
            }

            var coilValues = await _modBusCommand.ReadBitValuesAsync(ReadCoilsFunction, startingCoil, coilsToRead, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("ModBus Client: Read {NumberOfCoils} coils from: {CoilAddress}", coilsToRead, startingCoil);

            return coilValues;
        }
        catch (ModBusClientException ex)
        {
            _logger.LogError(ex, "ModBus Client: Exception while reading coils: {ExceptionCode}", ex.ExceptionCode);
            throw;
        }
    }

    public async ValueTask<ICollection<bool>> ReadDiscreteInputsAsync(ushort startingInput, ushort inputsToRead, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("ModBus Client: Reading {NumberOfInputs} discrete inputs from: {InputAddress}", inputsToRead, startingInput);

        try
        {
            if (inputsToRead == 0)
            {
                _logger.LogInformation("ModBus Client: No values requested.");
                return [];
            }

            var inputValues = await _modBusCommand.ReadBitValuesAsync(ReadDiscreteInputsFunction, startingInput, inputsToRead, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("ModBus Client: Read {NumberOfInputs} discrete inputs from: {InputAddress}", inputsToRead, startingInput);

            return inputValues;
        }
        catch (ModBusClientException ex)
        {
            _logger.LogError(ex, "ModBus Client: Exception while reading discrete inputs: {ExceptionCode}", ex.ExceptionCode);
            throw;
        }
    }

    public async ValueTask<ICollection<ushort>> ReadHoldingRegistersAsync(ushort startingRegister, ushort registersToRead, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("ModBus Client: Reading {NumberOfRegisters} holding registers from: {RegisterAddress}", registersToRead, startingRegister);

        try
        {
            if (registersToRead == 0)
            {
                _logger.LogInformation("ModBus Client: No values requested.");
                return [];
            }

            var registerValues = await _modBusCommand.ReadUshortValuesAsync(ReadHoldingRegistersFunction, startingRegister, registersToRead, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("ModBus Client: Read {NumberOfRegisters} holding registers from: {RegisterAddress}", registersToRead, startingRegister);

            return registerValues;
        }
        catch (ModBusClientException ex)
        {
            _logger.LogError(ex, "ModBus Client: Exception while reading holding registers: {ExceptionCode}", ex.ExceptionCode);
            throw;
        }
    }

    public async ValueTask<ICollection<ushort>> ReadInputRegistersAsync(ushort startingRegister, ushort registersToRead, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("ModBus Client: Reading {NumberOfRegisters} input registers from: {RegisterAddress}", registersToRead, startingRegister);

        try
        {
            if (registersToRead == 0)
            {
                _logger.LogInformation("ModBus Client: No values requested.");
                return [];
            }

            var registerValues = await _modBusCommand.ReadUshortValuesAsync(ReadInputRegistersFunction, startingRegister, registersToRead, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("ModBus Client: Read {NumberOfRegisters} input registers from: {RegisterAddress}", registersToRead, startingRegister);

            return registerValues;
        }
        catch (ModBusClientException ex)
        {
            _logger.LogError(ex, "ModBus Client: Exception while reading input registers: {ExceptionCode}", ex.ExceptionCode);
            throw;
        }
    }

    public async ValueTask WriteSingleCoilAsync(ushort coilToWrite, bool value, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("ModBus Client: Writing {Value} to coil: {CoilAddress}", value, coilToWrite);

        try
        {
            await _modBusCommand.PerformFunctionAsync(WriteSingleCoilFunction, [coilToWrite, (ushort)(value ? 0xFF00 : 0)], cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("ModBus Client: Written {Value} to coil: {CoilAddress}", value, coilToWrite);
        }
        catch (ModBusClientException ex)
        {
            _logger.LogError(ex, "ModBus Client: Exception while writing to coil: {ExceptionCode}", ex.ExceptionCode);
            throw;
        }
    }

    public async ValueTask WriteSingleRegisterAsync(ushort registerToWrite, ushort value, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("ModBus Client: Writing {Value} to register: {RegisterAddress}", value, registerToWrite);

        try
        {
            await _modBusCommand.PerformFunctionAsync(WriteSingleRegisterFunction, [registerToWrite, value], cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("ModBus Client: Written {Value} to coil: {RegisterAddress}", value, registerToWrite);
        }
        catch (ModBusClientException ex)
        {
            _logger.LogError(ex, "ModBus Client: Exception while writing to register: {ExceptionCode}", ex.ExceptionCode);
            throw;
        }
    }

    public async ValueTask WriteMultipleCoilsAsync(ushort startingCoil, ICollection<bool> values, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("ModBus Client: Writing values to {NumberOfCoils} coils from: {CoilAddress}", values.Count, startingCoil);

        try
        {
            if (values.Count == 0)
            {
                _logger.LogInformation("ModBus Client: No values to write.");
                return;
            }

            await _modBusCommand.WriteBitValuesAsync(WriteMultipleCoilsFunction, startingCoil, values, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("ModBus Client: Written values to {NumberOfCoils} coils from: {CoilAddress}", values.Count, startingCoil);
        }
        catch (ModBusClientException ex)
        {
            _logger.LogError(ex, "ModBus Client: Exception while writing to coils: {ExceptionCode}", ex.ExceptionCode);
            throw;
        }
    }

    public async ValueTask WriteMultipleRegistersAsync(ushort startingRegister, ICollection<ushort> values, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("ModBus Client: Writing values to {NumberOfRegisters} registers from: {RegisterAddress}", values.Count, startingRegister);

        try
        {
            if (values.Count == 0)
            {
                _logger.LogInformation("ModBus Client: No values to write.");
                return;
            }

            await _modBusCommand.WriteUshortValuesAsync(WriteMultipleRegistersFunction, startingRegister, values, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("ModBus Client: Written values to {NumberOfRegisters} registers from: {RegisterAddress}", values.Count, startingRegister);
        }
        catch (ModBusClientException ex)
        {
            _logger.LogError(ex, "ModBus Client: Exception while writing to registers: {ExceptionCode}", ex.ExceptionCode);
            throw;
        }
    }

    public async ValueTask<ICollection<ICollection<ushort>>> ReadFileRecordAsync(ICollection<FileRecords> fileRecordsToRead, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("ModBus Client: Reading file registers from {NumberOfFiles} files.", fileRecordsToRead.Count);

        try
        {
            if (fileRecordsToRead.Count == 0)
            {
                _logger.LogInformation("ModBus Client: No values requested.");
                return [];
            }

            var fileRecordsValues = await _modBusCommand.ReadFileRecordsAsync(ReadFileRecordFunction, fileRecordsToRead, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("ModBus Client: Read file registers from {NumberOfFiles} files.", fileRecordsToRead.Count);

            return fileRecordsValues;
        }
        catch (ModBusClientException ex)
        {
            _logger.LogError(ex, "ModBus Client: Exception while reading file registers: {ExceptionCode}", ex.ExceptionCode);
            throw;
        }
    }

    public async ValueTask WriteFileRecordAsync(ICollection<FileRecordsData> fileRecordsToWrite, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("ModBus Client: Writing file registers for {NumberOfFiles} files.", fileRecordsToWrite.Count);

        try
        {
            if (fileRecordsToWrite.Count == 0)
            {
                _logger.LogInformation("ModBus Client: No values to write.");
                return;
            }

            await _modBusCommand.WriteFileRecordsAsync(WriteFileRecordFunction, fileRecordsToWrite, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("ModBus Client: Written file registers for {NumberOfFiles} files.", fileRecordsToWrite.Count);
        }
        catch (ModBusClientException ex)
        {
            _logger.LogError(ex, "ModBus Client: Exception while writing file registers: {ExceptionCode}", ex.ExceptionCode);
            throw;
        }
    }

    public async ValueTask MaskWriteRegisterAsync(ushort registerToWrite, ushort andMask, ushort orMask, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("ModBus Client: Writing register {registerToWrite} with AND mask {AndMask} and OR mask {OrMask}.", registerToWrite, andMask, orMask);

        try
        {
            await _modBusCommand.PerformFunctionAsync(MaskWriteRegisterFunction, [registerToWrite, andMask, orMask], cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("ModBus Client: Written register {registerToWrite} with AND mask {AndMask} and OR mask {OrMask}.", registerToWrite, andMask, orMask);
        }
        catch (ModBusClientException ex)
        {
            _logger.LogError(ex, "ModBus Client: Exception while writing to register with masks: {ExceptionCode}", ex.ExceptionCode);
            throw;
        }
    }

    public async ValueTask<ICollection<ushort>> ReadWriteMultipleRegistersAsync(ushort startingReadRegister, ushort registersToRead, ushort startingWriteRegister, ICollection<ushort> values, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("ModBus Client: Reading {NumberOfRegistersToRead} holding registers from: {ReadRegisterAddress} and writing {NumberOfRegistersToWrite} holding registers from: {WriteRegisterAddress}", registersToRead, startingReadRegister, values.Count, startingWriteRegister);

        try
        {
            if (registersToRead == 0 && values.Count == 0)
            {
                _logger.LogInformation("ModBus Client: No values requested or to write.");
                return [];
            }

            var registerValues = await _modBusCommand.ReadWriteUshortValuesAsync(ReadWriteMultipleRegistersFunction, startingReadRegister, registersToRead, startingWriteRegister, values, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("ModBus Client: Read {NumberOfRegistersToRead} holding registers from: {ReadRegisterAddress} and written {NumberOfRegistersToWrite} holding registers from: {WriteRegisterAddress}", registersToRead, startingReadRegister, values.Count, startingWriteRegister);

            return registerValues;
        }
        catch (ModBusClientException ex)
        {
            _logger.LogError(ex, "ModBus Client: Exception while reading and writing holding registers: {ExceptionCode}", ex.ExceptionCode);
            throw;
        }
    }

    public async ValueTask<ICollection<ushort>> ReadFifoQueueAsync(ushort fifoPointerAddress, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("ModBus Client: Reading FIFO Queue: {FifoAddress}", fifoPointerAddress);

        try
        {
            var registerValues = await _modBusCommand.ReadFifoUshortValuesAsync(ReadFifoQueueFunction, fifoPointerAddress, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("ModBus Client: Read FIFO Queue: {FifoAddress}", fifoPointerAddress);

            return registerValues;
        }
        catch (ModBusClientException ex)
        {
            _logger.LogError(ex, "ModBus Client: Exception while reading FIFO Queue: {ExceptionCode}", ex.ExceptionCode);
            throw;
        }
    }

    public async ValueTask<ICollection<DeviceObject>> ReadDeviceIdentifierAsync(ReadDeviceIdentifier readDeviceId, byte objectId, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("ModBus Client: Reading Device Identifiers: {ReadDeviceId}/{ObjectId}", readDeviceId, objectId);

        try
        {
            var deviceIdentifierValues = new List<DeviceObject>();

            var result = new DeviceIdentifierResult(true, objectId, []);
            while (result.MoreFollows)
            {
                result = await _modBusCommand.ReadDeviceIdentifiersAsync(ReadDeviceIdentifierFunction, (byte)readDeviceId, result.NextObjectId, cancellationToken);
                deviceIdentifierValues.AddRange(result.ValuesRead);
            }

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("ModBus Client: Read Device Identifiers: {ReadDeviceId}/{ObjectId}", readDeviceId, objectId);

            return deviceIdentifierValues;
        }
        catch (ModBusClientException ex)
        {
            _logger.LogError(ex, "ModBus Client: Exception while reading device Identifiers: {ExceptionCode}", ex.ExceptionCode);
            throw;
        }
    }

    public void Close()
    {
        _logger.LogInformation("ModBus Client: Closing connection");

        _modBusCommand.Close();

        _logger.LogInformation("ModBus Client: Closed connection");
    }
}
