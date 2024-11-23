using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using GuedesPlace.DoorLabel.Services;
using WebGate.Azure.TableUtils;
using Microsoft.Extensions.Configuration;
using GuedesPlace.DoorLabel.Models;
using Microsoft.Extensions.Azure;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddEnvironmentVariables();
    })
    
    .ConfigureServices((context, services) => {
        var configuration = context.Configuration;
        var storageConnection = configuration["AzureWebJobsStorage"];
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton<ExtendedAzureTableClientService>((x) =>
        {
            var extendedAzureTableService = new ExtendedAzureTableClientService(storageConnection);
            extendedAzureTableService.CreateAndRegisterTableClient<CrmEndpoint>("CrmEndpoint");
            extendedAzureTableService.CreateAndRegisterTableClient<DeviceStatus>("DeviceStatus");
            extendedAzureTableService.CreateAndRegisterTableClient<DeviceLog>("DeviceLog");
            return extendedAzureTableService;
        });
        services.AddSingleton<CrmEndpointService>();
        services.AddSingleton<DeviceEndpointService>();
        services.AddSingleton<GeneratePictureService>();
        services.AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddBlobServiceClient(storageConnection).WithName("pictures");
        });
    })
    .Build();

host.Run();
