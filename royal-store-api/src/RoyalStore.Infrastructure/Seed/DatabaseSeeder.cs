using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RoyalStore.Domain.Const;
using RoyalStore.Infrastructure.Persistence;

namespace RoyalStore.Infrastructure.Seed;

public static class DatabaseSeeder
{
    // Usamos IServiceProvider para poder ser chamado depois que a injeção estiver pronta
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        // Cria um escopo para pegar serviços transientes/scoped
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            string[] systemRoles = { Role.Admin, Role.Seller, Role.Manager };

            foreach (var roleName in systemRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<RoyalStoreContext>>();
            logger.LogError(ex, "Ocorreu um erro ao popular as Roles no banco de dados.");
        }
    }
}