# Wyrm.ModBusClient
A .Net TCP ModBus client library.

## Usage
Add the ModBus client into dependency injection using the AddModBusClient extension method.
```csharp
using Wyrm.ModBusClient.DependencyInjection;
...
services.AddModBusClient();
```
By default, it is added as a Singleton but you can specify the lifetime.
```csharp
services.AddModBusClient(ServiceLifetime.Scoped);
```
Then inject the IModBusClient interface into your class.

Call ConnectAsync before calling any of the ModBus methods.
Although the client can remain connected, it's better to connect, perform the function or functions, and then close it.
You can connect the client again after closing it.
Make sure you dispose of the client when finished with it.

## Functions
Only the TCP specific functions have been implemented.
You can set the Unit Identifier before calling any function (it will keep the value set for subsequent functions).
```csharp
await _modBusClient.ConnectAsync(endPoint, ct);
...
_modBusClient.UnitIdentifier = 0xFF;
var coils = await _modBusClient.ReadCoilsAsync(1, 5, ct);
...
_modBusClient.Close();
```
You can also set the Transaction Id before calling any function. The Transaction Id increments after each function is called so if you need it to always be a specific value then you need to set it before calling a function.
```csharp
await _modBusClient.ConnectAsync(endPoint, ct);
...
_modBusClient.TransactionId = 0x5959;
var coils = await _modBusClient.ReadCoilsAsync(1, 5, ct);
...
_modBusClient.Close();
```
It is also possible to set the Protocol Identifier (for subsequent functions). Usually this is always 0, but certain vendors may require a different number.
```csharp
await _modBusClient.ConnectAsync(endPoint, ct);
_modBusClient.ProtocolIdentifier = 1;
...
_modBusClient.Close();
```

### Implemented Functions

| Function                        |
|---------------------------------|
| ReadCoilsAsync                  |
| ReadDiscreteInputsAsync         |
| ReadHoldingRegistersAsync       |
| ReadInputRegistersAsync         |
| WriteSingleCoilAsync            |
| WriteSingleRegisterAsync        |
| WriteMultipleCoilsAsync         |
| WriteMultipleRegistersAsync     |
| ReadFileRecordAsync             |
| WriteFileRecordAsync            |
| MaskWriteRegisterAsync          |
| ReadWriteMultipleRegistersAsync |
| ReadFifoQueueAsync              |
| ReadDeviceIdentifierAsync       |