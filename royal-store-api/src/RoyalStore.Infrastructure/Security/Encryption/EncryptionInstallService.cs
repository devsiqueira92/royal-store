using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoyalStore.Application.Abstractions;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Infrastructure.Persistence.Encryption;

namespace RoyalStore.Infrastructure.Security.Encryption;

public sealed class EncryptionInstallService : IInstallService
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<EncryptionOptions>()
            .BindConfiguration(EncryptionOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Singleton: stateless after construction, and the EF model (built once per process)
        // captures it in its value converters.
        services.AddSingleton<IEncryptionService, AesGcmEncryptionService>();
        services.AddSingleton<BlindIndexInterceptor>();
    }
}
