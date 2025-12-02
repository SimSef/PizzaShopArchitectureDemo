using System;
using System.Diagnostics;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Hosting;
using Orleans;
using PizzaShop.Orleans.Contract;
using PizzaShop.Orleans.Server;

var activitySource = new ActivitySource("PizzaShop.Orleans.Server.Seeding");

var builder = Host.CreateApplicationBuilder(args);

builder.AddObservability();

// Register keyed Azure clients using Aspire helpers; Orleans expects the keys to
// match the configured ServiceKey names.
builder.AddKeyedAzureTableServiceClient("orleans-clustering");
builder.AddKeyedAzureTableServiceClient("orleans-reminders");
builder.AddKeyedAzureTableServiceClient("orleans-pubsub");
builder.AddKeyedAzureBlobServiceClient("orleans-grainstate");
builder.AddKeyedAzureQueueServiceClient("orleans-streams");
builder.UseOrleans(siloBuilder =>
{
    siloBuilder.UseDashboard(options =>
    {
        options.HostSelf = true;
        options.Host = "0.0.0.0";
        options.Port = 8081;
    });

    siloBuilder.AddStartupTask(async (serviceProvider, cancellationToken) =>
    {
        using var activity = activitySource.StartActivity("SeedDemoUser");
        activity?.SetTag("pizza.user.seed.step", "start");

        var grainFactory = serviceProvider.GetRequiredService<IGrainFactory>();

        // Demo user that matches the Keycloak test user in PizzaShop-realm.json.
        // We use the stable subject / nameidentifier value as the grain key.
        const string subjectId = "d55983a2-f6b4-4017-adbc-321fbc62bfec";
        activity?.SetTag("pizza.user.subject_id", subjectId);

        try
        {
            var userGrain = grainFactory.GetGrain<IUserGrain>(subjectId);

            await userGrain.SetProfileAsync(
                username: "testuser",
                email: "testuser@example.com",
                firstName: "Test",
                lastName: "User");

            activity?.SetTag("pizza.user.seed.step", "completed");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    });
});

var host = builder.Build();
await host.RunAsync();
