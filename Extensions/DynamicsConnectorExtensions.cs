using System.Net.Http.Headers;
using WebGate.Dynamics.Connector;

namespace GuedesPlace.DoorLabel.Extensions;

public static class DynamicsConnectorExtension {

    public static async Task UploadImageToEntity(this DynamicsConnector connector, string entityTypePlural, string entityId, string fieldName, byte[] data, string contentType, string fileName) {
        var path = $"{entityTypePlural}({entityId})/{fieldName}";

        using var request = connector.BuildRequestMessage(HttpMethod.Patch, path);
        var content = new ByteArrayContent(data);
        //content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        request.Headers.Add("x-ms-file-name", fileName);
        request.Content = content;
        var client = await connector.GetClientAsync();
        var result = await client.SendAsync(request);
        result.EnsureSuccessStatusCode();
    }
    public static async Task<byte[]> GetPictureDataAsync(this DynamicsConnector connector, string entityTypePlural, string entityId, string fieldName) {
        var path = $"{entityTypePlural}({entityId})/{fieldName}/$value?size=full";
        var xClient = await connector.GetClientAsync();
        using var request = connector.BuildRequestMessage(HttpMethod.Get, path);
        var response = await xClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }
}