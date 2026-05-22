using System.Net;

namespace Wyrm.ModBusClient.Connection;

internal class ModBusCommand(
    IModBusConnection _modBusConnection) : IModBusCommand
{
    private const int MaximumBitValues = 2000;
    private const int MaximumUshortValues = 125;
    private const int LowerMaximumUshortValues = 121;
    private const int MaximumFileRecordsValues = 251;
    private const byte FileReferenceType = 6;

    public ushort ProtocolIdentifier
    {
        get => _modBusConnection.ProtocolIdentifier;
        set => _modBusConnection.ProtocolIdentifier = value;
    }

    public byte UnitIdentifier
    {
        get => _modBusConnection.UnitIdentifier;
        set => _modBusConnection.UnitIdentifier = value;
    }

    public ushort TransactionId
    {
        get => _modBusConnection.TransactionId;
        set => _modBusConnection.TransactionId = value;
    }

    public Func<IList<byte>, IList<byte>>? PduFramer
    {
        get => _modBusConnection.PduFramer;
        set => _modBusConnection.PduFramer = value;
    }

    public Func<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>? PduDeframer
    {
        get => _modBusConnection.PduDeframer;
        set => _modBusConnection.PduDeframer = value;
    }

    public ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken) =>
        _modBusConnection.ConnectAsync(endPoint, cancellationToken);

    public async ValueTask<ICollection<bool>> ReadBitValuesAsync(byte functionNumber, ushort startingAddress, ushort bitsToRead, CancellationToken cancellationToken)
    {
        if (bitsToRead > MaximumBitValues)
            throw new ModBusClientException("ModBus Client: Too many values requested.", ModBusExceptionCode.TooManyBitValues);

        var bitsData = await _modBusConnection.PerformFunctionAsync(functionNumber, [startingAddress, bitsToRead], [], cancellationToken);

        var bitValues = new List<bool>();
        var valueData = bitsData[1..].Span;
        for (var bit = 0; bit < bitsToRead; ++bit)
        {
            bitValues.Add((valueData[bit / 8] & (byte)(0x01 << (bit % 8))) != 0);
        }
        return bitValues;
    }

    public async ValueTask<ICollection<ushort>> ReadUshortValuesAsync(byte functionNumber, ushort startingAddress, ushort ushortsToRead, CancellationToken cancellationToken)
    {
        if (ushortsToRead > MaximumUshortValues)
            throw new ModBusClientException("ModBus Client: Too many values requested.", ModBusExceptionCode.TooManyUshortValues);

        var ushortsData = await _modBusConnection.PerformFunctionAsync(functionNumber, [startingAddress, ushortsToRead], [], cancellationToken);

        return GetUshortValues(ushortsData, ushortsToRead);
    }

    private ICollection<ushort> GetUshortValues(ReadOnlyMemory<byte> ushortsData, ushort ushortsToRead)
    {
        var ushortValues = new List<ushort>();
        var valueData = ushortsData[1..].Span;
        for (var ushrt = 0; ushrt < ushortsToRead; ++ushrt)
        {
            ushortValues.Add((ushort)((valueData[ushrt * 2] << 8) + valueData[ushrt * 2 + 1]));
        }
        return ushortValues;
    }

    public async ValueTask WriteBitValuesAsync(byte functionNumber, ushort startingCoil, ICollection<bool> values, CancellationToken cancellationToken)
    {
        if (values.Count > MaximumBitValues)
            throw new ModBusClientException("ModBus Client: Too many values to write.", ModBusExceptionCode.TooManyBitValues);

        var coilValues = MakeBitValuesArray(values);
        await _modBusConnection.PerformFunctionAsync(functionNumber, [startingCoil, (ushort)values.Count], coilValues, cancellationToken);
    }

    private static byte[] MakeBitValuesArray(ICollection<bool> values)
    {
        var bitValues = new List<byte>();
        var valueIndex = 0;
        foreach (var value in values)
        {
            if (valueIndex % 8 == 0)
            {
                bitValues.Add((byte)(value ? 1 : 0));
            }
            else if (value)
            {
                bitValues[valueIndex / 8] |= (byte)(1 << (valueIndex % 8));
            }
            ++valueIndex;
        }

        bitValues.Insert(0, (byte)bitValues.Count);

        return [.. bitValues];
    }

    public async ValueTask WriteUshortValuesAsync(byte functionNumber, ushort startingAddress, ICollection<ushort> values, CancellationToken cancellationToken)
    {
        if (values.Count > MaximumUshortValues)
            throw new ModBusClientException("ModBus Client: Too many values to write.", ModBusExceptionCode.TooManyUshortValues);

        var registerValues = MakeUshortValuesArray(values);
        await _modBusConnection.PerformFunctionAsync(functionNumber, [startingAddress, (ushort)values.Count], registerValues, cancellationToken);
    }

    private static byte[] MakeUshortValuesArray(ICollection<ushort> values)
    {
        var ushortValues = new List<byte>();
        foreach (var value in values)
        {
            AddToList(ushortValues, value);
        }

        ushortValues.Insert(0, (byte)ushortValues.Count);

        return [.. ushortValues];
    }

    public async ValueTask<ICollection<ushort>> ReadWriteUshortValuesAsync(byte functionNumber, ushort startingReadAddress, ushort ushortsToRead, ushort startingWriteAddress, ICollection<ushort> values, CancellationToken cancellationToken)
    {
        if (ushortsToRead > MaximumUshortValues)
            throw new ModBusClientException("ModBus Client: Too many values requested.", ModBusExceptionCode.TooManyUshortValues);

        if (values.Count > LowerMaximumUshortValues)
            throw new ModBusClientException("ModBus Client: Too many values to write.", ModBusExceptionCode.TooManyUshortValues);

        var registerValues = MakeUshortValuesArray(values);

        var ushortsData = await _modBusConnection.PerformFunctionAsync(functionNumber, [startingReadAddress, ushortsToRead, startingWriteAddress, (ushort)values.Count], registerValues, cancellationToken);

        return GetUshortValues(ushortsData, ushortsToRead);
    }

    public async ValueTask<ICollection<ushort>> ReadFifoUshortValuesAsync(byte functionNumber, ushort startingAddress, CancellationToken cancellationToken)
    {
        var ushortsData = await _modBusConnection.PerformFunctionAsync(functionNumber, [startingAddress], [], cancellationToken);

        var ushortValues = new List<ushort>();
        var valueData = ushortsData[4..].Span;
        for (var ushrt = 0; ushrt < (valueData.Length / 2); ++ushrt)
        {
            ushortValues.Add((ushort)((valueData[ushrt * 2] << 8) + valueData[ushrt * 2 + 1]));
        }
        return ushortValues;
    }

    public async ValueTask<ICollection<ICollection<ushort>>> ReadFileRecordsAsync(byte functionNumber, ICollection<FileRecords> fileRecordsToRead, CancellationToken cancellationToken)
    {
        if ((fileRecordsToRead.Count * 2 + fileRecordsToRead.Sum(r => r.RecordLength * 2)) > MaximumFileRecordsValues)
            throw new ModBusClientException("ModBus Client: Too many values to read.", ModBusExceptionCode.TooManyUshortValues);

        var fileRecordsBytes = MakeFileRecordsByteArray(fileRecordsToRead);

        var fileRecordsData = await _modBusConnection.PerformFunctionAsync(functionNumber, [], [.. fileRecordsBytes], cancellationToken);

        return MakeFileRecordsValuesList(fileRecordsData);
    }

    private static List<byte> MakeFileRecordsByteArray(ICollection<FileRecords> fileRecordsToRead)
    {
        var byteArray = new List<byte>();
        foreach (var fileRecords in fileRecordsToRead)
        {
            byteArray.Add(FileReferenceType);
            AddToList(byteArray, fileRecords.FileNumber);
            AddToList(byteArray, fileRecords.StartingRecordAddress);
            AddToList(byteArray, fileRecords.RecordLength);
        }
        byteArray.Insert(0, (byte)byteArray.Count);
        return byteArray;
    }

    private static List<ICollection<ushort>> MakeFileRecordsValuesList(ReadOnlyMemory<byte> fileRecordsData)
    {
        var fileRecordsDataSpan = fileRecordsData[1..].Span;
        var fileRecordsValues = new List<ICollection<ushort>>();
        for (var recordStart = 0; recordStart < fileRecordsDataSpan.Length; ++recordStart)
        {
            var fileRecordsValue = new List<ushort>();
            var length = (fileRecordsDataSpan[recordStart] - 1) / 2;

            if (fileRecordsDataSpan[++recordStart] != FileReferenceType)
                throw new ModBusClientException("ModBus Client: Unrecognised file reference type.", ModBusExceptionCode.BadFileReferenceType);

            for (var record = 0; record < length; ++record)
            {
                fileRecordsValue.Add((ushort)((fileRecordsDataSpan[++recordStart] << 8) + fileRecordsDataSpan[++recordStart]));
            }

            fileRecordsValues.Add(fileRecordsValue);
        }

        return fileRecordsValues;
    }

    public async ValueTask WriteFileRecordsAsync(byte functionNumber, ICollection<FileRecordsData> fileRecordsToWrite, CancellationToken cancellationToken)
    {
        if ((fileRecordsToWrite.Count * 2 + fileRecordsToWrite.Sum(r => r.RecordLength * 2)) > MaximumFileRecordsValues)
            throw new ModBusClientException("ModBus Client: Too many values to write.", ModBusExceptionCode.TooManyUshortValues);

        var fileRecordsDataBytes = MakeFileRecordsDataByteArray(fileRecordsToWrite);

        await _modBusConnection.PerformFunctionAsync(functionNumber, [], [.. fileRecordsDataBytes], cancellationToken);
    }

    private static List<byte> MakeFileRecordsDataByteArray(ICollection<FileRecordsData> fileRecordsToWrite)
    {
        var byteArray = new List<byte>();
        foreach (var fileRecords in fileRecordsToWrite)
        {
            byteArray.Add(FileReferenceType);
            AddToList(byteArray, fileRecords.FileNumber);
            AddToList(byteArray, fileRecords.StartingRecordAddress);
            AddToList(byteArray, fileRecords.RecordLength);
            foreach (var recordData in fileRecords.Values)
            {
                AddToList(byteArray, recordData);
            }
        }
        byteArray.Insert(0, (byte)byteArray.Count);
        return byteArray;
    }

    private static void AddToList(List<byte> list, ushort value)
    {
        list.Add((byte)(value >> 8));
        list.Add((byte)(value & 0xFF));
    }

    public async ValueTask<DeviceIdentifierResult> ReadDeviceIdentifiersAsync(byte functionNumber, byte readDeviceId, byte objectId, CancellationToken cancellationToken)
    {
        const byte meiType = 14;

        var identifierData = await _modBusConnection.PerformFunctionAsync(functionNumber, [], [meiType, readDeviceId, objectId], cancellationToken);
        var identifierSpan = identifierData.Span;

        if (identifierSpan[0] != meiType)
            throw new ModBusClientException("ModBus Client: MEI Type mismatch.", ModBusExceptionCode.IncorrectMeiType);

        if (identifierSpan[1] != readDeviceId)
            throw new ModBusClientException("ModBus Client: Read Device Id mismatch.", ModBusExceptionCode.IncorrectReadDeviceId);

        // Unused for now: var conformityLevel = identifierSpan[2];
        bool moreFollows = identifierSpan[3] != 0;
        var nextObjectId = identifierSpan[4];

        var resultValues = new List<DeviceObject>();
        var bufferPos = 6;
        while (bufferPos < identifierSpan.Length)
        {
            var length = identifierSpan[bufferPos + 1];
            resultValues.Add(new DeviceObject(identifierSpan[bufferPos], identifierSpan.Slice(bufferPos + 2, length).ToArray()));
            bufferPos += length + 2;
        }

        return new DeviceIdentifierResult(moreFollows, nextObjectId, resultValues);
    }

    public ValueTask<ReadOnlyMemory<byte>> PerformFunctionAsync(byte functionNumber, ushort[] parameters, CancellationToken cancellationToken) =>
        _modBusConnection.PerformFunctionAsync(functionNumber, parameters, [], cancellationToken);

    public ValueTask<ReadOnlyMemory<byte>> PerformFunctionAsync(byte functionNumber, ushort[] parameters, byte[] values, CancellationToken cancellationToken) =>
        _modBusConnection.PerformFunctionAsync(functionNumber, parameters, values, cancellationToken);

    public void Close() =>
        _modBusConnection.Close();
}
