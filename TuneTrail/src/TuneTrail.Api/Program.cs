using TuneTrail.Api.IoC.Configs;

var builder = WebApplication.CreateBuilder(args);

builder.AddDatabaseConfiguration();

var app = builder.Build();

app.MapGet("/", () => "TuneTrail API is running.");

app.Run();
