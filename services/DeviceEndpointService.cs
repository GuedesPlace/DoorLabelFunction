using Azure.Storage.Blobs;
using GuedesPlace.DoorLabel.Extensions;
using GuedesPlace.DoorLabel.Models;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using WebGate.Azure.TableUtils;
using WebGate.Dynamics.Connector;
using Newtonsoft.Json.Linq;
using System.Text;
using Newtonsoft.Json;
using Azure.Storage.Blobs.Models;

namespace GuedesPlace.DoorLabel.Services;

public class DeviceEndpointService(ILogger<DeviceEndpointService> logger, ExtendedAzureTableClientService tableClientService, IAzureClientFactory<BlobServiceClient> blobClientFactory)
{

    private readonly ILogger<DeviceEndpointService> _logger = logger;
    private readonly TypedAzureTableClient<DeviceStatus> _tableClient = tableClientService.GetTypedTableClient<DeviceStatus>();
    private readonly TypedAzureTableClient<DeviceLog> _tableClientDeviceLog = tableClientService.GetTypedTableClient<DeviceLog>();
    private readonly BlobContainerClient _picturesContainer = blobClientFactory.CreateClient("pictures").GetBlobContainerClient("pictures");

    public async Task<List<DeviceStatus>> GetAllDevicesByCrmEndpointId(string crmEndpointId)
    {
        var all = await _tableClient.GetAllAsync(crmEndpointId);
        return all.Select(x => x.Entity).ToList();
    }
    public async Task RegisterDeviceStatus(DeviceStatus deviceStatus)
    {
        var result = await _tableClient.InsertOrReplaceAsync(deviceStatus.MacAsId, deviceStatus.CrmEndpointId, deviceStatus);
        return;
    }
    public async Task UpdateDeviceStatus(DeviceStatus deviceStatus)
    {
        var result = await _tableClient.InsertOrMergeAsync(deviceStatus.MacAsId, deviceStatus.CrmEndpointId, deviceStatus);
        return;
    }
    public async Task<DeviceStatus> GetDeviceStatus(string macAsId)
    {
        var query = $"RowKey eq '{macAsId}'";
        var result = await _tableClient.GetAllByQueryAsync(query);
        if (result.Count == 0)
        {
            return null;
        }
        if (result.Count > 1)
        {
            throw new Exception("Only one Device should be registered per MacID");
        }
        return result.FirstOrDefault().Entity;
    }
    public async Task<BinaryData> RetrieveDeviceImage(DeviceStatus deviceStatus)
    {
        var blobClient = _picturesContainer.GetBlobClient($"{deviceStatus.CrmEndpointId}/{deviceStatus.CrmID}.png");
        var result = await blobClient.DownloadContentAsync();
        return result.Value.Content;
    }
    public async Task<RoomLabel> GetRoomLabelAsync(string deviceId, DynamicsConnector connector, string crmURL)
    {
        var query = $"gp_roomdisplaies({deviceId})?$expand=gp_roomdisplay_SystemUser_SystemUser($select=fullname,title),gp_configuration";
        var result = await connector.GetAsync<DynamicsRoomDisplay>(query);
        if (result == null)
        {
            return null;
        }
        var pictureData = await RetrievePossiblePictureData(result.Configuration, connector, crmURL);
        return new RoomLabel()
        {
            Name = result.Name,
            Elements = result.Users.Select(user => new RoomLabelElement() { Name = user.Name, Title = user.Title }).OrderBy(e1 => e1.Name).ToList(),
            Configuration = result.Configuration,
            picture = pictureData
        };
    }

    private async Task<byte[]?> RetrievePossiblePictureData(DynamicsDisplayConfiguration configuration, DynamicsConnector connector, string crmURL)
    {
        if (!configuration.gp_has_picture || string.IsNullOrEmpty(configuration.gp_picture_url))
        {
            return null;
        }
        return await connector.GetPictureDataAsync("gp_displayconfigurations", configuration.gp_displayconfigurationid, "gp_picture");
    }

    public async Task UploadAndSavePicture(DeviceStatus deviceStatus, DynamicsConnector connector, BinaryData content, PictureGreyScaleStorage pictureGreyScaleStorage)
    {
        var blobClient = _picturesContainer.GetBlobClient($"{deviceStatus.CrmEndpointId}/{deviceStatus.CrmID}.png");
        var blobClient2 = _picturesContainer.GetBlobClient($"{deviceStatus.CrmEndpointId}/{deviceStatus.CrmID}.json");
        await blobClient.UploadAsync(content, true);
        var updatePayLoad = new JObject() {
            {"gp_picture",content.ToArray()}
        };
        await connector.PatchAsync<JObject>($"gp_roomdisplaies({deviceStatus.CrmID})", updatePayLoad);
        await StorePictureStorage(blobClient2, pictureGreyScaleStorage);
    }
    private async Task StorePictureStorage(BlobClient blobClient, PictureGreyScaleStorage pictureGreyScaleStorage)
    {
        byte[] byteArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(pictureGreyScaleStorage));
        MemoryStream stream = new MemoryStream(byteArray);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = "application/json"
            }
        };
        await blobClient.UploadAsync(stream, uploadOptions);
    }
    public async Task<PictureGreyScaleStorage> GetPictureGreyScaleStorageAsync(DeviceStatus deviceStatus)
    {
        var blobClient = _picturesContainer.GetBlobClient($"{deviceStatus.CrmEndpointId}/{deviceStatus.CrmID}.json");
        BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
        string blobContents = downloadResult.Content.ToString();
        return JsonConvert.DeserializeObject<PictureGreyScaleStorage>(blobContents);
    }
    public async Task WriteLog(DeviceLog deviceLog)
    {
        var logDate = deviceLog.LogDate;
        if (logDate.HasValue)
        {
            await _tableClientDeviceLog.InsertOrReplaceAsync(logDate.Value.ToString("yyyy-MM-dd-hh-mm-ss"), deviceLog.MacAsId, deviceLog);

        }
    }
}