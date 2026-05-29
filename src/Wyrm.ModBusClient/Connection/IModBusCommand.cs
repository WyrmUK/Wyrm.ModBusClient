using System.Net;

namespace Wyrm.ModBusClient.Connection;

internal interface IModBusCommand
{
    ushort ProtocolIdentifier { get; set; }
    byte UnitIdentifier { get; set; }
    ushort TransactionId { get; set; }
    Func<IList<byte>, IList<byte>>? PduFramer { get; set; }
    Func<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>? PduDeframer { get; set; }
    ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken);
    ValueTask<ICollection<bool>> ReadBitValuesAsync(byte functionNumber, ushort startingAddress, ushort bitsToRead, CancellationToken cancellationToken);
    ValueTask<ICollection<ushort>> ReadUshortValuesAsync(byte functionNumber, ushort startingAddress, ushort ushortsToRead, CancellationToken cancellationToken);
    ValueTask WriteBitValuesAsync(byte functionNumber, ushort startingCoil, ICollection<bool> values, CancellationToken cancellationToken);
    ValueTask WriteUshortValuesAsync(byte functionNumber, ushort startingAddress, ICollection<ushort> values, CancellationToken cancellationToken);
    ValueTask<ICollection<ushort>> ReadWriteUshortValuesAsync(byte functionNumber, ushort startingReadAddress, ushort ushortsToRead, ushort startingWriteAddress, ICollection<ushort> values, CancellationToken cancellationToken);
    ValueTask<ICollection<ushort>> ReadFifoUshortValuesAsync(byte functionNumber, ushort startingAddress, CancellationToken cancellationToken);
    ValueTask<ICollection<ICollection<ushort>>> ReadFileRecordsAsync(byte functionNumber, ICollection<FileRecords> fileRecordsToRead, CancellationToken cancellationToken);
    ValueTask WriteFileRecordsAsync(byte functionNumber, ICollection<FileRecordsData> fileRecordsToWrite, CancellationToken cancellationToken);
    ValueTask<DeviceIdentifierResult> ReadDeviceIdentifiersAsync(byte functionNumber, byte readDeviceId, byte objectId, CancellationToken cancellationToken);
    ValueTask<ReadOnlyMemory<byte>> PerformFunctionAsync(byte functionNumber, ushort[] parameters, CancellationToken cancellationToken);
    ValueTask<ReadOnlyMemory<byte>> PerformFunctionAsync(byte functionNumber, ushort[] parameters, byte[] values, CancellationToken cancellationToken);
    ValueTask<ushort> ReadUshortValuesRequestAsync(byte functionNumber, ushort startingAddress, ushort ushortsToRead, CancellationToken cancellationToken);
    ValueTask<UshortDataResponse> ReadUshortValuesResponseAsync(CancellationToken cancellationToken);
    void Close();
}
