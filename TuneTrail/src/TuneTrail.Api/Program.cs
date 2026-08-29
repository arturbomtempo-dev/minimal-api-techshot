using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi;
using TuneTrail.Api.IoC.Configs;
using TuneTrail.Api.IoC.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddDatabaseConfiguration();

builder.Services.RegisterServices();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Version = "v1",
            Title = "TuneTrail API",
            Description = "Personal music listening log built with .NET Minimal API.",
        }
    );

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "TuneTrail API";
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TuneTrail API v1");
    });
}

app.UseHttpsRedirection();

app.RegisterModules();

app.Run();
