using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Wyrm.ModBusClient.Connection;
using Wyrm.ModBusClient.Socket;

namespace Wyrm.ModBusClient.DependencyInjection;

/// <summary>
/// Extensions for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the required services for the ModBus client.
    /// </summary>
    /// <param name="services">An <see cref="IServiceCollection"/> to add to.</param>
    /// <param name="serviceLifetime">The <see cref="ServiceLifetime"/> for the client (defaults to Singleton).</param>
    /// <returns>The <see cref="IServiceCollection"/> with the added ModBus client services.</returns>
    public static IServiceCollection AddModBusClient(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
    {
        if (services.All(s => s.ServiceType != typeof(ILoggerFactory)))
        {
            services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        }
        services.AddSingleton<ISocketFactory, SocketFactory>();
        services.AddSingleton<IModBusSocketFactory, ModBusSocketFactory>();
        services.Add(new ServiceDescriptor(typeof(IModBusConnection), typeof(ModBusConnection), serviceLifetime));
        services.Add(new ServiceDescriptor(typeof(IModBusCommand), typeof(ModBusCommand), serviceLifetime));
        services.Add(new ServiceDescriptor(typeof(IModBusClient), typeof(ModBusClient), serviceLifetime));
        return services;
    }
}
