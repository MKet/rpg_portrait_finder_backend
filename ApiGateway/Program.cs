using Ocelot.Middleware;
using Ocelot.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", true, true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true);
builder.Configuration.AddJsonFile("ocelot.json");

builder.Services.AddOcelot();

var app = builder.Build();
app.UseOcelot().Wait();
app.Run();
