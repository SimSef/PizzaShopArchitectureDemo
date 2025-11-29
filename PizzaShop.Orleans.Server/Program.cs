using System;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Hosting;
using Orleans;
using PizzaShop.Orleans.Contract;
using PizzaShop.Orleans.Server;

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
});

var host = builder.Build();
await host.RunAsync();
