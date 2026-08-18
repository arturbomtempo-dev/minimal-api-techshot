var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "TuneTrail API is running.");

app.Run();
