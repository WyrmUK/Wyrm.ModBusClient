using System.Net;
using System.Net.Sockets;

namespace Wyrm.ModBusClient.Socket;

internal sealed class SocketFactory : ISocketFactory
{
    public ISocketWrapper CreateSocket(EndPoint endPoint)
    {
        return new SocketWrapper(
            new System.Net.Sockets.Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
    }
}
