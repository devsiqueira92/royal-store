using RoyalStore.API.Authorization;
using RoyalStore.API.Extensions;
using RoyalStore.API.Middleware;
using RoyalStore.Infrastructure;
using RoyalStore.Infrastructure.Seed;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilog();

builder.Services.AddOpenApi();

builder.Services.InstallServicesFromAssemblies(
    builder.Configuration,
    typeof(AssemblyReference).Assembly,
    typeof(RoyalStore.Application.AssemblyReference).Assembly);

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseCors("frontend");

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseRateLimiter();

await DatabaseSeeder.SeedRolesAsync(app.Services);
await ExpenseCategorySeeder.SeedDefaultsForExistingTenantsAsync(app.Services);

app.UseAuthentication();
app.UseMiddleware<LogEnrichmentMiddleware>();
app.UseMiddleware<AuthorizationAuditMiddleware>();
app.UseAuthorization();

app.RegisterEndpointDefinitions();

app.Run();
