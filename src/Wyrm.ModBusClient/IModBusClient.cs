using System.Net;

namespace Wyrm.ModBusClient;

/// <summary>
/// Interface for a TCP ModBus client.
/// </summary>
public interface IModBusClient
{
    // https://www.modbus.org/file/secure/modbusprotocolspecification.pdf
    /// <summary>
    /// Gets and sets the Protocol Identifier (defaults to 0).
    /// This should only be set to something other than 0 if the vendor specific implementation requires it.
    /// </summary>
    ushort ProtocolIdentifier { get; set; }
    /// <summary>
    /// Gets and sets the Unit Identifier (defaults to 1).
    /// Typically this is either 1 or 255 (vendor specific).
    /// </summary>
    byte UnitIdentifier { get; set; }
    /// <summary>
    /// Gets and sets the Transaction Id (defaults to 1 initially).
    /// It is incremented after each command so you need to set it before calling any method if you want it to remain at a specific value.
    /// </summary>
    ushort TransactionId { get; set; }
    /// <summary>
    /// Gets and sets an optional framer function (defaults to null).
    /// If specified, this is called with the extended PDU (Unit Identifier + PDU) and should return the framed PDU.
    /// </summary>
    Func<IList<byte>, IList<byte>>? PduFramer { get; set; }
    /// <summary>
    /// Gets and sets an optional deframer function (defaults to null).
    /// If specified, this is called with the framed PDU (everything after the length) and should return the deframed PDU.
    /// The deframed PDU should represent a valid extended MODBUS PDU (Unit Identifier + PDU).
    /// </summary>
    Func<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>? PduDeframer { get; set; }

    /// <summary>
    /// Connects the client to a ModBus server.
    /// </summary>
    /// <param name="endPoint">The <see cref="IPEndPoint"/> to connect to.</param>
    /// <param name="cancellationToken">Token to cancel the connect.</param>
    /// <exception cref="ModBusClientException">Thrown if there is an error.</exception>
    ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken = default);
    /// <summary>
    /// Reads sequential coil values synchronously.
    /// </summary>
    /// <param name="startingCoil">The 0-based coil address to start reading from.</param>
    /// <param name="coilsToRead">The number of coils to read (from 1 to 2000).</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>An <see cref="ICollection{T}"/> of coil values.</returns>
    /// <exception cref="ModBusClientException">Thrown if there is an error.</exception>
    /// <exception cref="OperationCanceledException">Thrown if cancelled.</exception>
    ValueTask<ICollection<bool>> ReadCoilsAsync(ushort startingCoil, ushort coilsToRead, CancellationToken cancellationToken = default);
    /// <summary>
    /// Reads sequential discrete input values synchronously.
    /// </summary>
    /// <param name="startingInput">The 0-based input address to start reading from.</param>
    /// <param name="inputsToRead">The number of inputs to read (from 1 to 2000).</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>An <see cref="ICollection{T}"/> of discrete input values.</returns>
    /// <exception cref="ModBusClientException">Thrown if there is an error.</exception>
    /// <exception cref="OperationCanceledException">Thrown if cancelled.</exception>
    ValueTask<ICollection<bool>> ReadDiscreteInputsAsync(ushort startingInput, ushort inputsToRead, CancellationToken cancellationToken = default);
    /// <summary>
    /// Reads sequential holding registers synchronously.
    /// </summary>
    /// <param name="startingRegister">The 0-based holding register address to start reading from.</param>
    /// <param name="registersToRead">The number of registers to read (from 1 to 125).</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>An <see cref="ICollection{T}"/> of holding register values.</returns>
    /// <exception cref="ModBusClientException">Thrown if there is an error.</exception>
    /// <exception cref="OperationCanceledException">Thrown if cancelled.</exception>
    ValueTask<ICollection<ushort>> ReadHoldingRegistersAsync(ushort startingRegister, ushort registersToRead, CancellationToken cancellationToken = default);
    /// <summary>
    /// Reads sequential input registers synchronously.
    /// </summary>
    /// <param name="startingRegister">The 0-based input register address to start reading from.</param>
    /// <param name="registersToRead">The number of registers to read (from 1 to 125).</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>An <see cref="ICollection{T}"/> of input register values.</returns>
    /// <exception cref="ModBusClientException">Thrown if there is an error.</exception>
    /// <exception cref="OperationCanceledException">Thrown if cancelled.</exception>
    ValueTask<ICollection<ushort>> ReadInputRegistersAsync(ushort startingRegister, ushort registersToRead, CancellationToken cancellationToken = default);
    /// <summary>
    /// Writes a value to a single coil synchronously.
    /// </summary>
    /// <param name="coilToWrite">The 0-based coil address to write to.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <exception cref="ModBusClientException">Thrown if there is an error.</exception>
    /// <exception cref="OperationCanceledException">Thrown if cancelled.</exception>
    ValueTask WriteSingleCoilAsync(ushort coilToWrite, bool value, CancellationToken cancellationToken = default);
    /// <summary>
    /// Writes a value to a single holding register synchronously.
    /// </summary>
    /// <param name="registerToWrite">The 0-based holding register address to write to.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <exception cref="ModBusClientException">Thrown if there is an error.</exception>
    /// <exception cref="OperationCanceledException">Thrown if cancelled.</exception>
    ValueTask WriteSingleRegisterAsync(ushort registerToWrite, ushort value, CancellationToken cancellationToken = default);
    /// <summary>
    /// Writes sequential coil values synchronously.
    /// </summary>
    /// <param name="startingCoil">The 0-based coil address to start writing to.</param>
    /// <param name="values">The values to write (no more than 2000).</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <exception cref="ModBusClientException">Thrown if there is an error.</exception>
    /// <exception cref="OperationCanceledException">Thrown if cancelled.</exception>
    ValueTask WriteMultipleCoilsAsync(ushort startingCoil, ICollection<bool> values, CancellationToken cancellationToken = default);
    /// <summary>
    /// Writes sequential holding register values synchronously.
    /// </summary>
    /// <param name="startingRegister">The 0-based holding register address to start writing to.</param>
    /// <param name="values">The values to write (no more than 125).</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <exception cref="ModBusClientException">Thrown if there is an error.</exception>
    /// <exception cref="OperationCanceledException">Thrown if cancelled.</exception>
    ValueTask WriteMultipleRegistersAsync(ushort startingRegister, ICollection<ushort> values, CancellationToken cancellationToken = default);
    /// <summary>
    /// Reads records (registers) from files synchronously.
    /// There is a limit to the number of files and records that can be read in one go.
    /// For one <see cref="FileRecords"/> item it is 124 records.
    /// For two <see cref="FileRecords"/> items it is a total of 123 records.
    /// For three <see cref="FileRecords"/> items it is a total of 122 records.
    /// ...
    /// For 62 <see cref="FileRecords"/> items it is a total of 63 records.
    /// </summary>
    /// <param name="fileRecordsToRead">The files and their records to read.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>An <see cref="ICollection{T}"/> of <see cref="ICollection{T}"/>s of file record (register) values.</returns>
    /// <exception cref="ModBusClientException">Thrown if there is an error.</exception>
    /// <exception cref="OperationCanceledException">Thrown if cancelled.</exception>
    ValueTask<ICollection<ICollection<ushort>>> ReadFileRecordAsync(ICollection<FileRecords> fileRecordsToRead, CancellationToken cancellationToken = default);
    /// <summary>
    /// Writes records (registers) to files synchronously.
    /// There is a limit to the number of files and records that can be written in one go.
    /// For one <see cref="FileRecords"/> item it is 124 records.
    /// For two <see cref="FileRecords"/> items it is a total of 123 records.
    /// For three <see cref="FileRecords"/> items it is a total of 122 records.
    /// ...
    /// For 62 <see cref="FileRecords"/> items it is a total of 63 records.
    /// </summary>
    /// <param name="fileRecordsToWrite">The fies and their record values to write.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <exception cref="ModBusClientException">Thrown if there is an error.</exception>
    /// <exception cref="OperationCanceledException">Thrown if cancelled.</exception>
    ValueTask WriteFileRecordAsync(ICollection<FileRecordsData> fileRecordsToWrite, CancellationToken cancellationToken = default);
    /// <summary>
    /// Modifies the contents of a holding register with an AND mask and an OR mask synchronously.
    /// Contents = (Contents AND andMask) OR (orMask AND (NOT andMask))
    /// </summary>
    /// <param name="registerToWrite">The 0-based holding register address to write to.</param>
    /// <param name="andMask">The AND mask to use.</param>
    /// <param name="orMask">The OR mask to use.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <exception cref="ModBusClientException">Thrown if there is an error.</exception>
    /// <exception cref="OperationCanceledException">Thrown if cancelled.</exception>
    ValueTask MaskWriteRegisterAsync(ushort registerToWrite, ushort andMask, ushort orMask, CancellationToken cancellationToken = default);
    /// <summary>
    /// Writes and reads multiple registers in one transaction synchronously.
    /// The write is performed first.
    /// </summary>
    /// <param name="startingReadRegister">The 0-based holding register address to start reading from.</param>
    /// <param name="registersToRead">The number of registers to read (from 1 to 125).</param>
    /// <param name="startingWriteRegister">The 0-based holding register address to start writing to.</param>
    /// <param name="values">The values to write (no more than 121).</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>An <see cref="ICollection{T}"/> of holding register values.</returns>
    /// <exception cref="ModBusClientException">Thrown if there is an error.</exception>
    /// <exception cref="OperationCanceledException">Thrown if cancelled.</exception>
    ValueTask<ICollection<ushort>> ReadWriteMultipleRegistersAsync(ushort startingReadRegister, ushort registersToRead, ushort startingWriteRegister, ICollection<ushort> values, CancellationToken cancellationToken = default);
    /// <summary>
    /// Reads the values from a FIFO queue of registers but does not clear them synchronously.
    /// </summary>
    /// <param name="fifoPointerAddress">The 0-based FIFO queue address to read from.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>An <see cref="ICollection{T}"/> of FIFO queue register values.</returns>
    /// <exception cref="ModBusClientException">Thrown if there is an error.</exception>
    /// <exception cref="OperationCanceledException">Thrown if cancelled.</exception>
    ValueTask<ICollection<ushort>> ReadFifoQueueAsync(ushort fifoPointerAddress, CancellationToken cancellationToken = default);
    /// <summary>
    /// Reads the identification and additional information from the device synchronously.
    /// </summary>
    /// <param name="readDeviceId">The id of the data to read.</param>
    /// <param name="objectId">The id of the object to read from.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>An <see cref="ICollection{T}"/> of <see cref="DeviceObject"/>s.</returns>
    /// <exception cref="ModBusClientException">Thrown if there is an error.</exception>
    /// <exception cref="OperationCanceledException">Thrown if cancelled.</exception>
    ValueTask<ICollection<DeviceObject>> ReadDeviceIdentifierAsync(ReadDeviceIdentifier readDeviceId, byte objectId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Issues a request to read sequential holding registers asynchronously.
    /// </summary>
    /// <param name="startingRegister">The 0-based holding register address to start reading from.</param>
    /// <param name="registersToRead">The number of registers to read (from 1 to 125).</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <exception cref="ModBusClientException">Thrown if there is an error.</exception>
    /// <exception cref="OperationCanceledException">Thrown if cancelled.</exception>
    ValueTask ReadHoldingRegistersRequestAsync(ushort startingRegister, ushort registersToRead, CancellationToken cancellationToken = default);
    /// <summary>
    /// Issues a request to read sequential input registers asynchronously.
    /// </summary>
    /// <param name="startingRegister">The 0-based input register address to start reading from.</param>
    /// <param name="registersToRead">The number of registers to read (from 1 to 125).</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <exception cref="ModBusClientException">Thrown if there is an error.</exception>
    /// <exception cref="OperationCanceledException">Thrown if cancelled.</exception>
    ValueTask ReadInputRegistersRequestAsync(ushort startingRegister, ushort registersToRead, CancellationToken cancellationToken = default);
    /// <summary>
    /// Reads input or holding registers data asynchronously.
    /// Use this when the server issues multiple responses.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the read.</param>
    /// <returns>A <see cref="UshortDataResponse"/> item with the response data.</returns>
    /// <exception cref="ModBusClientException">Thrown if there is an error.</exception>
    /// <exception cref="OperationCanceledException">Thrown if cancelled.</exception>
    ValueTask<UshortDataResponse> ReadRegistersResponseDataAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Closes the client connection to a ModBus server.
    /// </summary>
    void Close();
}
