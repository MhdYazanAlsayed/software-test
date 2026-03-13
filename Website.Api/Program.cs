using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using ObserveTool.Extensions;
using Website.Services.Implementations;
using Website.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("Default") ??
    throw new NullReferenceException("Connection string was null");
var password = builder.Configuration["Monitoring:Password"] ?? throw new NullReferenceException("Cannot find the password of monitoring dashboard.");

//Add my log service
builder.Services.AddInProcessMonitoring(connectionString, config =>
{
    config.AutoMigrateOnStartup = true;
    config.FlushIntervalSeconds = 30;
    config.Security.EnableAuth = true;
    config.Security.Password = password;
});


builder.Services.AddSingleton<IFinderService, FinderService>();
builder.Services.AddCors(config =>
{
    config.AddDefaultPolicy(cors =>
    {
        var origions = builder.Configuration.GetSection("Origions").Get<string[]>();
        if (origions is not null)
        {
            foreach (var orig in origions)
            {
                cors = cors.WithOrigins(orig);
            }

            cors
            .AllowAnyHeader()
            .AllowAnyMethod();
        }
    });
});

builder.Services.AddHealthChecks();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Required for minimal APIs
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
app.UseRouting();
app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMonitorDashboard();
app.UseInProcessMonitoring();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();

