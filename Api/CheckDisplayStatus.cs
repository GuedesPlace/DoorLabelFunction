using System.Security.Cryptography;
using System.Text;
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
    private readonly SHA256 sha256Hash = SHA256.Create();


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
        var crmURL = await _crmEndpointService.GetDynamicsURLAsync(crmEndpointId);
        var devices = await _deviceEndpointService.GetAllDevicesByCrmEndpointId(crmEndpointId);
        foreach (var device in devices)
        {
            await ProcessDevice(device, dynamicsConnection, crmURL);
        }
    }

    private async Task ProcessDevice(DeviceStatus device, DynamicsConnector dynamicsConnection, string crmURL)
    {
        var label = await _deviceEndpointService.GetRoomLabelAsync(device.CrmID, dynamicsConnection, crmURL);
        var labelString = JsonConvert.SerializeObject(label, Formatting.None);
        var labelHash = GetHash(sha256Hash,labelString);
        _logger.LogInformation($"labelString...: {labelString}");
        _logger.LogInformation($"labelHash: {labelHash} -> {device.PictureHash}");
        if (labelHash != device.PictureHash) {
            var picture = _pictureService.CreatePicture(label);
            var greyScaleBytes = picture.GreyScale.Select(i=>intToByteArray(i)).SelectMany(x=>x).ToArray();
            var pictureGreyScaleStorage = new PictureGreyScaleStorage(){GreyScale = picture.GreyScale, PictureHash=labelHash, GreyScaleBase64 = Convert.ToBase64String(greyScaleBytes)};
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
    private string GetHash(HashAlgorithm hashAlgorithm, string input) 
    {
        byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

        // Create a new Stringbuilder to collect the bytes
        // and create a string.
        var sBuilder = new StringBuilder();

        // Loop through each byte of the hashed data
        // and format each one as a hexadecimal string.
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        // Return the hexadecimal string.
        return sBuilder.ToString();
    }
    private bool CheckHash(string hash, string newHash) {
        StringComparer comparer = StringComparer.OrdinalIgnoreCase;

        return comparer.Compare(newHash, hash) == 0;
    }
}

