using GuedesPlace.DoorLabel.Models;
using GuedesPlace.DoorLabel.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GuedesPlace.DoorLabel;
public class GetLabel(ILogger<GetLabel> logger, GeneratePictureService pictureService, DeviceEndpointService deviceEndpointService)
{
    private readonly ILogger<GetLabel> _logger = logger;
    private readonly GeneratePictureService _pictureService = pictureService;
    private readonly DeviceEndpointService _deviceEndpointService = deviceEndpointService;


    [Function("GetLabelArray")]
    public async Task<IActionResult> GetLabelArrayRun([HttpTrigger(AuthorizationLevel.Function, "post", Route = "device/{macId}/{hash}")] HttpRequest req, string macId, string hash)
    {
        var deviceStatus = await _deviceEndpointService.GetDeviceStatus(macId);
        if (deviceStatus == null)
        {
            return new NoContentResult();
        }
        var currentLog = await req.ReadFromJsonAsync<DeviceLog>();
        if (currentLog != null)
        {
            currentLog.MacAsId = macId;
            currentLog.LogDate = DateTime.UtcNow;
            await _deviceEndpointService.WriteLog(currentLog);
        }
        if (deviceStatus.PictureHash == hash)
        {
            return new OkObjectResult(new { status = "nochange" });
        }
        var greyScale = await _deviceEndpointService.GetPictureGreyScaleStorageAsync(deviceStatus);
        return new OkObjectResult(new { status = "changed", hash = greyScale.PictureHash });
    }
    [Function("GetLabelPicture")]
    public async Task<IActionResult> GetLabelPictureRun([HttpTrigger(AuthorizationLevel.Function, "get", Route = "deviceimage/{macId}")] HttpRequest req, string macId)
    {
        var deviceStatus = await _deviceEndpointService.GetDeviceStatus(macId);
        if (deviceStatus == null)
        {
            return new NoContentResult();
        }
        var picture = await _deviceEndpointService.RetrieveDeviceImage(deviceStatus);
        return new FileContentResult(picture.ToArray(), "image/png");
    }
}


