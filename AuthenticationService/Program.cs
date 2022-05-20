using AuthenticationService.Data.Contexts;
using AuthenticationService.Data.Repositories;
using AuthenticationService.Data.Repositories.Implementations;
using AuthenticationService.Services;
using AuthenticationService.Services.Implementations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<EntityAuthenticationContext>(contextbuilder =>
{
    contextbuilder.UseSqlServer(builder.Configuration["ConnectionString"]);
});
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthenticationService, JwtAuthenticationService>(
    provider => 
        new JwtAuthenticationService(
            provider.GetRequiredService<IUserRepository>(), 
            builder.Configuration["RSA_private"],
            builder.Configuration["RSA_public"],
            provider.GetService<ILogger<JwtAuthenticationService>>()
            )
        );

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
