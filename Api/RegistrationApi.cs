using GuedesPlace.DoorLabel.Models;
using GuedesPlace.DoorLabel.Services;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GuedesPlace.DoorLabel;
public class RegistrationApi(ILogger<GetLabel> logger, CrmEndpointService crmEndpointService, DeviceEndpointService deviceEndpointService)
{
    private readonly ILogger<GetLabel> _logger = logger;
    private readonly CrmEndpointService _service = crmEndpointService;
    private readonly DeviceEndpointService _deviceEndpointService = deviceEndpointService;
    

    [Function("RegisterCRMEndpoint")]
    public async Task<IActionResult> RegisterCRMEndpointRun([HttpTrigger(AuthorizationLevel.Admin, "post", Route = "registration/crmendpoint")] HttpRequest req)
    {
        _logger.LogInformation("Register new CRM Endpoint");
        var endpoint = await req.ReadFromJsonAsync<CrmEndpoint>();
        endpoint.Id = Guid.NewGuid().ToString();
        await _service.RegisterNewConnector(endpoint);
        return new OkObjectResult(endpoint);
    }
    [Function("UpdateCRMEndpoint")]
    public async Task<IActionResult> UpdateCRMEndpointRun([HttpTrigger(AuthorizationLevel.Admin, "put", Route = "registration/crmendpoint/{id}")] HttpRequest req, string id)
    {
        _logger.LogInformation("Register new CRM Endpoint");
        var endpoint = await req.ReadFromJsonAsync<CrmEndpoint>();
        endpoint.Id = id;
        await _service.UpdateConnector(endpoint);
        return new OkObjectResult(endpoint);
    }
    [Function("RegisterDeviceEndpoint")]
    public async Task<IActionResult> RegisterDeviceEndpointRun([HttpTrigger(AuthorizationLevel.Admin, "post", Route = "registration/device")] HttpRequest req)
    {
        _logger.LogInformation("Register new CRM Endpoint");
        var device = await req.ReadFromJsonAsync<DeviceStatus>();
        await _deviceEndpointService.RegisterDeviceStatus(device);
        return new OkObjectResult(device);
    }
    [Function("UpdateDeviceEndpoint")]
    public async Task<IActionResult> UpdateDeviceEndpointRun([HttpTrigger(AuthorizationLevel.Admin, "put", Route = "registration/device/{id}")] HttpRequest req, string id)
    {
        _logger.LogInformation("Register new CRM Endpoint");
        var deviceStatus = await req.ReadFromJsonAsync<DeviceStatus>();
        deviceStatus.CrmID = id;
        await _deviceEndpointService.UpdateDeviceStatus(deviceStatus);
        return new OkObjectResult(deviceStatus);
    }

}
