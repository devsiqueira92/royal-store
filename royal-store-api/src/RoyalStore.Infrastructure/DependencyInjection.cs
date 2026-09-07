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
using RoyalStore.Application.Abstractions.Loyalty;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Options;
using RoyalStore.Domain.Const;
using RoyalStore.Domain.Identity;
using RoyalStore.Domain.Permissions;
using RoyalStore.Infrastructure.Authentication;
using RoyalStore.Infrastructure.Catalog;
using RoyalStore.Infrastructure.Email;
using RoyalStore.Infrastructure.Identity;
using RoyalStore.Infrastructure.Persistence;
using RoyalStore.Infrastructure.Persistence.Interceptors;
using RoyalStore.Infrastructure.Persistence.Loyalty;
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
            // Seller accounts are provisioned without an email (they sign in by
            // username). RequireUniqueEmail = true makes Identity's UserValidator
            // reject a null/empty email ("Email '' is invalid."). Email uniqueness
            // for real accounts is enforced per-tenant in the application layer
            // (RegisterTenant / CompleteOnboarding), which is what a shared-DB
            // multitenant app needs anyway.
            options.User.RequireUniqueEmail = false;
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

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var cookieName = configuration["AuthCookie:CookieName"] ?? "access_token";
                        if (context.Request.Cookies.TryGetValue(cookieName, out var token))
                            context.Token = token;

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyNames.AdminOrManager, policy =>
                policy.RequireRole(Role.Admin, Role.Manager));

            options.AddPolicy(PolicyNames.Seller, policy =>
                policy.RequireRole(Role.Seller)
                    .RequireClaim("onboardingStatus", "completed"));

            options.AddPolicy(PolicyNames.SellerOnboarding, policy =>
                policy.RequireRole(Role.Seller));

            options.AddPolicy(PolicyNames.Admin, policy =>
                policy.RequireRole(Role.Admin));

            options.AddPolicy(PolicyNames.SuperAdmin, policy =>
                policy.RequireRole(Role.SuperAdmin));

            options.AddPolicy(PolicyNames.StaffRead, policy =>
                policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(Role.Admin) ||
                    ctx.User.IsInRole(Role.Manager) ||
                    (ctx.User.IsInRole(Role.Seller) && ctx.User.HasClaim("onboardingStatus", "completed"))));

            foreach (var entry in PermissionCatalog.All)
            {
                options.AddPolicy(
                    PolicyNames.PermissionPrefix + entry.Name,
                    policy => policy.RequireClaim("permission", entry.Name));
            }
        });

        services.AddScoped<ICategoryUsageChecker, CategoryUsageChecker>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ITaxProfileTemplateRepository, TaxProfileTemplateRepository>();
        services.AddScoped<ITaxProfileTemplateUsageChecker, TaxProfileTemplateUsageChecker>();
        services.AddScoped<ISellerRepository, SellerRepository>();
        services.AddScoped<IPermissionGrantRepository, PermissionGrantRepository>();
        services.AddScoped<ICashShiftRepository, CashShiftRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IIbptTaxRepository, IbptTaxRepository>();
        services.AddScoped<IOriginOptionRepository, OriginOptionRepository>();
        services.AddScoped<IUnitMeasureOptionRepository, UnitMeasureOptionRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ILoyaltyTierRepository, LoyaltyTierRepository>();
        services.AddScoped<ILoyaltyTierUsageChecker, LoyaltyTierUsageChecker>();
        services.AddScoped<ICashbackLedger, CashbackLedger>();

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IEmailService, ConsoleEmailService>();
        services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
        services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
        services.AddScoped<ISecurityAuditService, SecurityAuditService>();
        services.AddScoped<IConfirmationResendRequestRepository, ConfirmationResendRequestRepository>();
        services.AddScoped<IEmailChangeRequestRepository, EmailChangeRequestRepository>();

        services.AddOptions<PasswordResetOptions>()
            .BindConfiguration("PasswordReset")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<ResendConfirmationOptions>()
            .BindConfiguration("ResendConfirmation")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<ChangePendingEmailOptions>()
            .BindConfiguration("ChangePendingEmail")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<AuthCookieOptions>()
            .BindConfiguration("AuthCookie")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<CorsOptions>()
            .BindConfiguration("Cors")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddCors(corsOptions =>
        {
            corsOptions.AddPolicy("frontend", policy =>
                policy.WithOrigins(configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
                    .AllowCredentials()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

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

            rateLimiterOptions.AddPolicy("change-pending-email", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = configuration.GetValue("ChangePendingEmail:IpPermitLimit", 20),
                        Window = TimeSpan.FromMinutes(
                            configuration.GetValue("ChangePendingEmail:IpWindowMinutes", 1))
                    }));
        });
    }
}
