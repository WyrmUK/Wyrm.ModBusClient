using System.Net;

namespace Wyrm.ModBusClient.Socket;

internal class ModBusSocketFactory(
    ISocketFactory _socketFactory) : IModBusSocketFactory
{
    public IModBusSocket CreateSocket(EndPoint endPoint)
    {
        return new ModBusSocket(
            _socketFactory.CreateSocket(endPoint));
    }
}
