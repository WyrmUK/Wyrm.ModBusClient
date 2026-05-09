using Moq;
using Shouldly;
using System.Net;
using Wyrm.ModBusClient.Socket;

namespace Wyrm.ModBusClient.UnitTests.Socket;

public class ModBusSocketFactoryTests
{
    private readonly IModBusSocketFactory _modBusSocketFactory;

    private readonly ISocketFactory _socketFactory = Mock.Of<ISocketFactory>();
    private readonly EndPoint _endPoint = Mock.Of<EndPoint>();
    private readonly ISocketWrapper _socketWrapper = Mock.Of<ISocketWrapper>();

    public ModBusSocketFactoryTests()
    {
        _modBusSocketFactory = new ModBusSocketFactory(_socketFactory);
    }

    [Fact]
    public void CreateSocket_Should_Create_SocketWrapper()
    {
        Mock.Get(_socketFactory)
            .Setup(x => x.CreateSocket(_endPoint))
            .Returns(_socketWrapper);

        var result = _modBusSocketFactory.CreateSocket(_endPoint);

        result.ShouldNotBeNull();
        Mock.Get(_socketFactory)
            .Verify(x => x.CreateSocket(_endPoint), Times.Once);
    }
}
