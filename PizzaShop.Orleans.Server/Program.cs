using System;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Hosting;
using PizzaShop.Orleans.Contract;

var builder = Host.CreateApplicationBuilder(args);

// Explicitly register keyed Azure clients using Aspire-provided connection strings.
// Orleans expects keyed services matching the configured ServiceKey names.
string GetConnection(string name) =>
    builder.Configuration.GetConnectionString(name)
    ?? throw new InvalidOperationException($"Missing connection string '{name}'.");

builder.Services.AddKeyedSingleton<TableServiceClient>("orleans-clustering",
    (_, _) => new TableServiceClient(GetConnection("orleans-clustering")));
builder.Services.AddKeyedSingleton<TableServiceClient>("orleans-reminders",
    (_, _) => new TableServiceClient(GetConnection("orleans-reminders")));
builder.Services.AddKeyedSingleton<TableServiceClient>("orleans-pubsub",
    (_, _) => new TableServiceClient(GetConnection("orleans-pubsub")));
builder.Services.AddKeyedSingleton<BlobServiceClient>("orleans-grainstate",
    (_, _) => new BlobServiceClient(GetConnection("orleans-grainstate")));
builder.Services.AddKeyedSingleton<QueueServiceClient>("orleans-streams",
    (_, _) => new QueueServiceClient(GetConnection("orleans-streams")));

// Let Orleans configure clustering, storage, reminders, and streaming
// based on configuration (including Aspire-provided connection info).
// Add PubSubStore for the AzureQueue streaming provider.
builder.UseOrleans(siloBuilder =>
{
    siloBuilder.AddAzureTableGrainStorage(
        "PubSubStore",
        options =>
        {
            var sp = siloBuilder.Services.BuildServiceProvider();
            options.TableServiceClient = sp.GetRequiredKeyedService<TableServiceClient>("orleans-pubsub");
            options.TableName = "OrleansPubSub";
        });
});

var host = builder.Build();
await host.RunAsync();
