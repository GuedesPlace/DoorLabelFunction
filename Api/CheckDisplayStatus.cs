using GuedesPlace.DoorLabel.Models;
using GuedesPlace.DoorLabel.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebGate.Dynamics.Connector;

namespace GuedesPlace.DoorLabel;
public class CheckDisplayStatus(ILogger<CheckDisplayStatus> logger, CrmEndpointService crmEndpointService, DeviceEndpointService deviceEndpointService, GeneratePictureService pictureService)
{
    private readonly ILogger _logger = logger;
    private readonly CrmEndpointService _crmEndpointService = crmEndpointService;
    private readonly DeviceEndpointService _deviceEndpointService = deviceEndpointService;
    private readonly GeneratePictureService _pictureService = pictureService;


    [Function("CheckDisplayStatus")]
    public async Task Run([TimerTrigger("0 */30 * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        var allCrmEndpoints = await _crmEndpointService.GetAllCrmEnpointIds();
        foreach (var crmEndpointId in allCrmEndpoints)
        {
            await ProcessEndpoint(crmEndpointId);
        }

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
        }
    }

    private async Task ProcessEndpoint(string crmEndpointId)
    {
        var dynamicsConnection = await _crmEndpointService.GetDynamicsConnectorAsync(crmEndpointId);
        if (dynamicsConnection == null)
        {
            _logger.LogError($"No Connector found for: {crmEndpointId}");
            return;
        }
        var devices = await _deviceEndpointService.GetAllDevicesByCrmEndpointId(crmEndpointId);
        foreach (var device in devices)
        {
            await ProcessDevice(device, dynamicsConnection);
        }
    }

    private async Task ProcessDevice(DeviceStatus device, DynamicsConnector dynamicsConnection)
    {
        var label = await _deviceEndpointService.GetRoomLabelAsync(device.CrmID, dynamicsConnection);
        var labelString = JsonConvert.SerializeObject(label, Formatting.None);
        var labelHash = $"0x{labelString.GetHashCode():X8}_{device.CrmID}";
        _logger.LogInformation($"labelString...: {labelString}");
        _logger.LogInformation($"labelHash: {labelHash} -> {device.PictureHash}");
        if (labelHash != device.PictureHash) {
            var picture = _pictureService.CreatePicture(label);
            var pictureGreyScaleStorage = new PictureGreyScaleStorage(){GreyScale = picture.GreyScale, PictureHash=labelHash};
            await _deviceEndpointService.UploadAndSavePicture( device, dynamicsConnection,picture.Data, pictureGreyScaleStorage);
            device.PictureHash = labelHash;
            await _deviceEndpointService.UpdateDeviceStatus(device);
        }
    }

    private byte[] intToByteArray(int i) {
        byte[] bytes = new byte[2];
        bytes[0] = (byte)(i >> 8);
        bytes[1] = (byte)i;
        return bytes;
    }
}

