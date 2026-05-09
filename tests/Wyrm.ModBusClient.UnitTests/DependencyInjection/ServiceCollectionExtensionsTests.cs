using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Wyrm.ModBusClient.Connection;
using Wyrm.ModBusClient.DependencyInjection;
using Wyrm.ModBusClient.Socket;

namespace Wyrm.ModBusClient.UnitTests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddModBusClient_Should_Add_ModBusClient(bool withLogging)
    {
        var loggingServices = new ServiceCollection();
        if (withLogging)
        {
            loggingServices.AddLogging();
        }
        var services = loggingServices.AddModBusClient(ServiceLifetime.Scoped);

        if (withLogging)
        {
            services.ShouldNotContain(s => s.Lifetime == ServiceLifetime.Singleton && s.ServiceType == typeof(ILoggerFactory) && s.ImplementationType == typeof(NullLoggerFactory));
        }
        else
        {
            services.ShouldContain(s => s.Lifetime == ServiceLifetime.Singleton && s.ServiceType == typeof(ILoggerFactory) && s.ImplementationType == typeof(NullLoggerFactory));
            services.ShouldContain(s => s.Lifetime == ServiceLifetime.Singleton && s.ServiceType == typeof(ILogger<>) && s.ImplementationType == typeof(Logger<>));
        }

        services.ShouldContain(s => s.Lifetime == ServiceLifetime.Singleton && s.ServiceType == typeof(ISocketFactory) && s.ImplementationType == typeof(SocketFactory));
        services.ShouldContain(s => s.Lifetime == ServiceLifetime.Singleton && s.ServiceType == typeof(IModBusSocketFactory) && s.ImplementationType == typeof(ModBusSocketFactory));
        services.ShouldContain(s => s.Lifetime == ServiceLifetime.Scoped && s.ServiceType == typeof(IModBusConnection) && s.ImplementationType == typeof(ModBusConnection));
        services.ShouldContain(s => s.Lifetime == ServiceLifetime.Scoped && s.ServiceType == typeof(IModBusCommand) && s.ImplementationType == typeof(ModBusCommand));
        services.ShouldContain(s => s.Lifetime == ServiceLifetime.Scoped && s.ServiceType == typeof(IModBusClient) && s.ImplementationType == typeof(ModBusClient));
    }
}
