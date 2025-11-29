using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Extensions.Hosting;

namespace PizzaShop.Orleans.Server;

public static class ObservabilityRegistration
{
    public static IHostApplicationBuilder AddObservability(this IHostApplicationBuilder builder)
    {
        const string serviceName = "PizzaShop.Orleans.Server";
        const string serviceNamespace = "PizzaShop";
        var serviceVersion = GetInformationalVersion() ?? "0.0.0";

        var resource = ResourceBuilder.CreateDefault().AddService(
            serviceName: serviceName,
            serviceNamespace: serviceNamespace,
            serviceVersion: serviceVersion,
            serviceInstanceId: Environment.MachineName);

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(rb => rb.AddService(
                serviceName: serviceName,
                serviceNamespace: serviceNamespace,
                serviceVersion: serviceVersion,
                serviceInstanceId: Environment.MachineName))
            .WithTracing(tracer =>
            {
                tracer
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource("Microsoft.Orleans.Runtime")
                    .AddSource("Microsoft.Orleans.Application")
                    .AddOtlpExporter();
            })
            .WithMetrics(meter =>
            {
                meter
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddMeter("Microsoft.Orleans")
                    .AddOtlpExporter();
            });

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.ParseStateValues = true;
            logging.SetResourceBuilder(resource);
            logging.AddOtlpExporter();
        });

        return builder;
    }

    private static string? GetInformationalVersion()
        => Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;
}
