using System.Net;

namespace Wyrm.ModBusClient.Socket;

internal interface IModBusSocketFactory
{
    IModBusSocket CreateSocket(EndPoint endPoint);
}
