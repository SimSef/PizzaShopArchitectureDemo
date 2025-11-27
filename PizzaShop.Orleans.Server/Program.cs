using System;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Hosting;
using PizzaShop.Orleans.Contract;

var builder = Host.CreateApplicationBuilder(args);

// Register keyed Azure clients using Aspire helpers; Orleans expects the keys to
// match the configured ServiceKey names.
builder.AddKeyedAzureTableServiceClient("orleans-clustering");
builder.AddKeyedAzureTableServiceClient("orleans-reminders");
builder.AddKeyedAzureTableServiceClient("orleans-pubsub");
builder.AddKeyedAzureBlobServiceClient("orleans-grainstate");
builder.AddKeyedAzureQueueServiceClient("orleans-streams");
builder.UseOrleans();

var host = builder.Build();
await host.RunAsync();
