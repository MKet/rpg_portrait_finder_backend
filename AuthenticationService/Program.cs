using AuthenticationService.Data.Contexts;
using AuthenticationService.Data.Repositories;
using AuthenticationService.Data.Repositories.Implementations;
using AuthenticationService.Services;
using AuthenticationService.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using JWT.Algorithms;

var builder = WebApplication.CreateBuilder(args);

var keyVaultEndpoint = new Uri(builder.Configuration["VaultUri"]);
builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<EntityAuthenticationContext>(contextbuilder =>
{
    contextbuilder.UseSqlServer(builder.Configuration["ConnectionString"]);
});

builder.Services.AddHttpClient();
builder.Services.AddScoped(provider => new SecretClient(keyVaultEndpoint, new DefaultAzureCredential(true)));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJwtAlgorithm, AzureRSAJwtAlgorithm>();
builder.Services.AddScoped<IAuthenticationService, JwtAuthenticationService>();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    
    EntityAuthenticationContext context = scope.ServiceProvider.GetRequiredService<EntityAuthenticationContext>();
    context.Database.Migrate();
    app.UseSwagger();
    app.UseSwaggerUI();
    await context.SeedAsync();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
