using Aspire.Hosting.Keycloak;

var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithRealmImport("./Realm");

builder.Build().Run();
