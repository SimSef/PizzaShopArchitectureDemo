var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator();
var clusteringTable = storage.AddTables("orleans-clustering");
var grainStorage = storage.AddBlobs("orleans-grainstate");
var remindersTable = storage.AddTables("orleans-reminders");
var pubsubTable = storage.AddTables("orleans-pubsub");
var streamQueues = storage.AddQueues("orleans-streams");

var orleans = builder.AddOrleans("default")
    .WithClustering(clusteringTable)
    .WithGrainStorage("Default", grainStorage)
    .WithReminders(remindersTable)
    .WithGrainStorage("PubSubStore", pubsubTable)
    .WithStreaming("AzureQueueProvider", streamQueues);

var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithRealmImport("./Realm");

var silo = builder.AddProject<Projects.PizzaShop_Orleans_Server>("orleans-server")
    .WithReference(orleans)
    .WithEndpoint(name: "dashboard", port: 8081, targetPort: 8081, isProxied: false, scheme: "http");

var web = builder.AddProject<Projects.PizzaShop_Web_Bff>("web-bff")
    .WithReference(orleans.AsClient())
    .WithReference(keycloak)
    // Keycloak BFF configuration for PizzaShop.Web
    // Values mirror PizzaShop-realm.json and can be overridden as needed.
    .WithEnvironment("Keycloak__Authority", "http://localhost:8080/realms/PizzaShop")
    .WithEnvironment("Keycloak__ClientId", "PizzaShopWeb")
    .WithEnvironment("Keycloak__ClientSecret", "pizza-shop-web-secret")
    // SPA origin used by CORS for the BFF pattern.
    .WithEnvironment("Spa__Origin", "http://localhost:3000")
    .WaitFor(silo)
    .WaitFor(keycloak);

var reactApp = builder.AddJavaScriptApp("pizzashop-web", "../pizzashop-web")
    .WithRunScript("start")
    .WithBuildScript("build")
    .WithEndpoint(name: "http", port: 3000, targetPort: 3000, isProxied: false, scheme: "http")
    .WithReference(web)
    .WaitFor(web);

builder.Build().Run();
