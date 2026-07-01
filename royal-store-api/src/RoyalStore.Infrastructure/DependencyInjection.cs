using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RoyalStore.Application.Abstractions;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.Abstractions.Catalog;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Email;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Options;
using RoyalStore.Domain.Const;
using RoyalStore.Domain.Identity;
using RoyalStore.Infrastructure.Authentication;
using RoyalStore.Infrastructure.Catalog;
using RoyalStore.Infrastructure.Email;
using RoyalStore.Infrastructure.Identity;
using RoyalStore.Infrastructure.Persistence;
using RoyalStore.Infrastructure.Persistence.Interceptors;
using RoyalStore.Infrastructure.Persistence.Repositories;
using RoyalStore.Infrastructure.Security;
using RoyalStore.Infrastructure.Services;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;

namespace RoyalStore.Infrastructure;

public sealed class DependencyInjection : IInstallService
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddSingleton<AuditInterceptor>();

        services.AddDbContext<RoyalStoreContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<RoyalStoreContext>());
        services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();
        services.AddScoped<ITenantContext, JwtTenantContext>();

        services.AddIdentityCore<UserEntity>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireLowercase = false;
            options.User.RequireUniqueEmail = true;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<RoyalStoreContext>()
            .AddDefaultTokenProviders();

        services.AddOptions<JwtOptions>()
            .BindConfiguration("Jwt")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var jwtSection = configuration.GetSection("Jwt");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSection["SecretKey"] ?? string.Empty)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSection["Audience"],
                    ValidateLifetime = true,
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.Zero,
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyNames.AdminOrManager, policy =>
                policy.RequireRole(Role.Admin, Role.Manager));

            options.AddPolicy(PolicyNames.Seller, policy =>
                policy.RequireRole(Role.Seller));

            options.AddPolicy(PolicyNames.Admin, policy =>
                policy.RequireRole(Role.Admin));
        });

        services.AddScoped<ICategoryUsageChecker, CategoryUsageChecker>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISellerRepository, SellerRepository>();
        services.AddScoped<ICashShiftRepository, CashShiftRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IEmailService, ConsoleEmailService>();
        services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
        services.AddScoped<ISecurityAuditService, SecurityAuditService>();
        services.AddScoped<IConfirmationResendRequestRepository, ConfirmationResendRequestRepository>();

        services.AddOptions<PasswordResetOptions>()
            .BindConfiguration("PasswordReset")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<ResendConfirmationOptions>()
            .BindConfiguration("ResendConfirmation")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.Configure<DataProtectionTokenProviderOptions>(o =>
            o.TokenLifespan = TimeSpan.FromMinutes(
                configuration.GetValue<int>("PasswordReset:TokenExpiryMinutes", 60)));

        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            rateLimiterOptions.AddPolicy("resend-confirmation-email", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = configuration.GetValue("ResendConfirmation:IpPermitLimit", 20),
                        Window = TimeSpan.FromMinutes(
                            configuration.GetValue("ResendConfirmation:IpWindowMinutes", 1))
                    }));
        });
    }
}
