using RoyalStore.API.Authorization;
using RoyalStore.API.Extensions;
using RoyalStore.API.Middleware;
using RoyalStore.Infrastructure;
using RoyalStore.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.InstallServicesFromAssemblies(
    builder.Configuration,
    typeof(AssemblyReference).Assembly,
    typeof(RoyalStore.Application.AssemblyReference).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

await DatabaseSeeder.SeedRolesAsync(app.Services);

app.UseAuthentication();
app.UseMiddleware<AuthorizationAuditMiddleware>();
app.UseAuthorization();

app.RegisterEndpointDefinitions();

app.Run();
