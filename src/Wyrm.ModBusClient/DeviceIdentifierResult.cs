namespace Wyrm.ModBusClient;

internal record DeviceIdentifierResult(bool MoreFollows, byte NextObjectId, ICollection<DeviceObject> ValuesRead);
