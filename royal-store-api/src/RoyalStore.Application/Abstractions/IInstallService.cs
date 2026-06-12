using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RoyalStore.Application.Abstractions;

public interface IInstallService
{
    void Install(IServiceCollection services, IConfiguration configuration);
}
