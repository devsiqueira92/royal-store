using System.Reflection;
using RoyalStore.Application.Abstractions;

namespace RoyalStore.API.Extensions;

internal static class InstallServicesExtension
{
    internal static IServiceCollection InstallServicesFromAssemblies(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        var installers = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IInstallService).IsAssignableFrom(t) && t is { IsAbstract: false, IsInterface: false })
            .Select(Activator.CreateInstance)
            .Cast<IInstallService>();

        foreach (var installer in installers)
            installer.Install(services, configuration);

        return services;
    }
}
