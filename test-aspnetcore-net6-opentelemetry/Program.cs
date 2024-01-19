// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

Console.WriteLine("Hello, World!");

const string ServiceName = "test-app";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenTelemetry()
    .WithTracing(builder =>
    {
        builder.AddSource(ServiceName)
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService(serviceName: ServiceName, serviceVersion: "1.0"))
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.EnrichWithException = (activity, exception) =>
                {
                    activity?.SetTag("message", exception.Message);
                    activity?.SetTag("stackTrace", exception.StackTrace);
                };
            })
            .AddOtlpExporter(opt =>
            {
                opt.Endpoint = new Uri("http://devenv:4317");
                opt.Protocol = OtlpExportProtocol.Grpc;
            });
    });
var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

ActivitySource _source = new ActivitySource("test-app", "1.0");

app.MapGet("ping", context =>
{
    using var activity = _source.StartActivity("test activity");
    activity?.SetTag("test", "test-value");

    activity?.SetStatus(ActivityStatusCode.Ok);
    return Task.FromResult(activity?.RootId);
});
app.MapGet("ping500", context =>
{
    using var activity = _source.StartActivity("test activity");
    activity?.SetTag("test", "test-value");

    try
    {
        throw new Exception("Random exception");
    }
    finally
    {
        activity?.SetStatus(ActivityStatusCode.Error);
    }

    activity?.SetStatus(ActivityStatusCode.Ok);
    return Task.FromResult(activity?.RootId);
});

app.Run("http://localhost:8888");

// var builder = new WebHostBuilder();
// var app = builder
//     .UseKestrel(options => options.Listen(IPAddress.Parse("127.0.0.1"), 8888))
//     .UseContentRoot(Directory.GetCurrentDirectory())
//     .UseWebRoot(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
//     .ConfigureServices(collection =>
//     {
//         collection.AddControllers();
//     })
//     .Configure(app =>
//     {
//         app.UseRouting();
//         app.UseStaticFiles(new StaticFileOptions()
//         {
//             FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(),  "wwwroot")),
//             // RequestPath = "/"
//         });
//     }).Build();

// var builder = Host.CreateDefaultBuilder()
//     .ConfigureAppConfiguration((context, configurationBuilder) =>
//     {
//         
//     })
//     .ConfigureServices((context, collection) =>
//     {
//         collection
//             .AddOpenTelemetry()
//             .WithTracing(builder =>
//             {
//                 builder.AddSource(ServiceName)
//                     .SetResourceBuilder(ResourceBuilder.CreateDefault()
//                         .AddService(serviceName: ServiceName, serviceVersion: "1.0"))
//                     .AddAspNetCoreInstrumentation()
//                     .AddOtlpExporter(opt =>
//                     {
//                         opt.Endpoint = new Uri("http://devenv:4317");
//                         opt.Protocol = OtlpExportProtocol.Grpc;
//                     });
//             });
//
//         collection.AddControllers();
//     })
//     .ConfigureWebHostDefaults(builder => builder.UseKestrel(options =>
//         {
//             options.AddServerHeader = false;
//             options.Limits.MinRequestBodyDataRate = new MinDataRate(100.0, TimeSpan.FromSeconds(10.0));
//             options.Limits.MinResponseDataRate = new MinDataRate(100.0, TimeSpan.FromSeconds(10.0));
//         })
//         .UseUrls($"http://*:8081"));

await app.RunAsync();