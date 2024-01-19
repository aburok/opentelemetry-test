// See https://aka.ms/new-console-template for more information

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

public class Program
{
    public static async Task Main(string[] args)
    {
        using var host = CreateHostBuilder(args).Build();
        await host.RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder()
            // .ConfigureOpenTelemetry()
            .ConfigureLogging((hostContext, logging) =>
            {
                logging
                    .AddConsole()
                    .AddConfiguration(hostContext.Configuration.GetSection("Logging"));
            })
            .ConfigureServices(collection =>
            {
                collection.AddOpenTelemetry()
                    .ConfigureResource(builder =>
                    {
                        builder.AddAttributes(
                            new Dictionary<string, object>() { { "test.resource", "test-value" } });
                    })
                    .WithTracing(builder =>
                    {
                        builder.AddSource("aspnetcore-controller-api")
                            .ConfigureResource(rc =>
                            {
                                rc.AddAttributes(new Dictionary<string, object>()
                                    { { "test.resource", "test-value" } });
                            })
                            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                                .AddService(serviceName: "aspnetcore-controller-api", serviceVersion: "1.0"))
                            .AddAspNetCoreInstrumentation(options =>
                            {
                                options.RecordException = true;
                                options.EnrichWithException = (activity, exception) =>
                                {
                                    activity?.SetTag("message", exception.Message);
                                    activity?.SetTag("stackTrace", exception.StackTrace);
                                };
                            })
                            .AddConsoleExporter()
                            // .AddZipkinExporter(opt =>
                            // {
                            //     opt.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTEL_EXPORTER_ZIPKIN_ENDPOINT"));
                            // })
                            .AddOtlpExporter(opt =>
                            {
                                opt.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTEL_TRACE_EXPORTER_OTLP_ENDPOINT"));
                            });
                    });
            })
            .ConfigureWebHostDefaults(builder => builder.UseKestrel(options =>
                {
                    options.AddServerHeader = false;
                    options.Limits.MinRequestBodyDataRate = new MinDataRate(100.0, TimeSpan.FromSeconds(10.0));
                    options.Limits.MinResponseDataRate = new MinDataRate(100.0, TimeSpan.FromSeconds(10.0));
                })
                .UseUrls($"http://*:8899")
                .UseStartup<Startup>());
    }
}

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc(delegate(MvcOptions o) { })
            .AddControllersAsServices();
        services.AddCors();
    }

    public void Configure(IApplicationBuilder builder, IWebHostEnvironment _)
    {
        builder.UseRouting();
        builder.UseCors("CorsPolicy");
        builder.UseEndpoints(ep => ep.MapControllers());
    }
}