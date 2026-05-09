using System.Net;

namespace Wyrm.ModBusClient.Socket;

internal interface ISocketFactory
{
    ISocketWrapper CreateSocket(EndPoint endPoint);
}
