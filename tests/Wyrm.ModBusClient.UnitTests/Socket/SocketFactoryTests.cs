using Moq;
using Shouldly;
using System.Net;
using System.Net.Sockets;
using Wyrm.ModBusClient.Socket;

namespace Wyrm.ModBusClient.UnitTests.Socket;

public class SocketFactoryTests
{
    private readonly ISocketFactory _socketFactory = new SocketFactory();

    private readonly EndPoint _endPoint = Mock.Of<EndPoint>();

    [Fact]
    public void CreateSocket_Should_Create_SocketWrapper()
    {
        Mock.Get(_endPoint)
            .Setup(x => x.AddressFamily)
            .Returns(AddressFamily.InterNetwork);

        var result = _socketFactory.CreateSocket(_endPoint);

        result.ShouldNotBeNull();
    }
}
