namespace Wyrm.ModBusClient;

/// <summary>
/// Exception codes.
/// </summary>
public enum ModBusExceptionCode
{
    /// <summary>
    /// The function is not an allowable action on the server, or the server is in the wrong state.
    /// </summary>
    IllegalFunction = 1,
    /// <summary>
    /// The data address is not an allowable address on the server.
    /// </summary>
    IllegalDataAddress = 2,
    /// <summary>
    /// A value contained in the query data field is not an allowable value for the server.
    /// </summary>
    IllegalDataValue = 3,
    /// <summary>
    /// An unrecoverable error occurred while the server was attempting to perform the requested action.
    /// </summary>
    ServerDeviceFailure = 4,
    /// <summary>
    /// Specialized. The server has accepted the request and is processing it, but a long duration of time will be required to do so.
    /// </summary>
    Acknowledge = 5,
    /// <summary>
    /// Specialized. The server is engaged in processing a long duration program command.
    /// </summary>
    ServerDeviceBusy = 6,

    /// <summary>
    /// Specialized use in conjunction with function codes 20 and 21 and reference type 6, to indicate that the extended file area failed to pass a consistency check.
    /// </summary>
    MemoryParityError = 8,

    /// <summary>
    /// Specialized use in conjunction with gateways, indicates that the gateway was unable to allocate an internal communication path from the input port to the output port for processing the request.
    /// </summary>
    GatewayPathUnavailable = 10,
    /// <summary>
    /// Specialized use in conjunction with gateways, indicates that no response was obtained from the target device.
    /// </summary>
    GatewayTargetDeviceFailedToRespond = 11,

    /// <summary>
    /// No data was received from the server.
    /// </summary>
    NoDataReceived = 256,
    /// <summary>
    /// A socket error occurred - see the Inner Exception for details.
    /// </summary>
    SocketError = 257,
    /// <summary>
    /// A socket connection error occurred.
    /// </summary>
    ConnectionError = 258,
    /// <summary>
    /// The socket is not currently connected to a server.
    /// </summary>
    SocketNotConnected = 259,
    /// <summary>
    /// Not enough data to validate a response was received.
    /// </summary>
    InsufficientData = 260,
    /// <summary>
    /// The received transaction id didn't match the request.
    /// </summary>
    IncorrectTransactionIdReceived = 261,
    /// <summary>
    /// The received function code disn't match the request.
    /// </summary>
    IncorrectFunctionReceived = 262,
    /// <summary>
    /// Not enough data was received for the requested function.
    /// </summary>
    InsufficientDataReceived = 263,
    /// <summary>
    /// More than 2000 bit values in request.
    /// </summary>
    TooManyBitValues = 264,
    /// <summary>
    /// More than 125 ushort values in request.
    /// </summary>
    TooManyUshortValues = 265,
    /// <summary>
    /// The file reference typre received was not recognised.
    /// </summary>
    BadFileReferenceType = 266,
    /// <summary>
    /// The received MEI Type didn't match the request.
    /// </summary>
    IncorrectMeiType = 267,
    /// <summary>
    /// The received Read Device Id didn't match the request.
    /// </summary>
    IncorrectReadDeviceId = 268
}
