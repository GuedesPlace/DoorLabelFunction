using GuedesPlace.DoorLabel.Models;
using Microsoft.Extensions.Logging;
using WebGate.Azure.TableUtils;
using WebGate.Dynamics.Connector;
using WebGate.Dynamics.Util;

namespace GuedesPlace.DoorLabel.Services;

public class CrmEndpointService (ILogger<CrmEndpointService> logger, ExtendedAzureTableClientService tableClientService) {
    private readonly ILogger<CrmEndpointService> _logger = logger;
    private readonly TypedAzureTableClient<CrmEndpoint> _tableClient =  tableClientService.GetTypedTableClient<CrmEndpoint>();
    private readonly Dictionary<string,DynamicsConnector> _connectorRegistry = [];
    private readonly Dictionary<string,string> _crmEndpointById = [];

    public async Task<DynamicsConnector> GetDynamicsConnectorAsync (string crmEndpointId ) {
        if (!_connectorRegistry.ContainsKey( crmEndpointId )) {
            var endpointResult = await _tableClient.GetByIdAsync(crmEndpointId,"endpoint");
            if (endpointResult == null) {
                return null;
            }
            RegisterConnectorInRegistry(endpointResult.Entity);
        }
        return _connectorRegistry[crmEndpointId];
    }

    public async Task<string> GetDynamicsURLAsync (string crmEndpointId ) {
        if (!_connectorRegistry.ContainsKey( crmEndpointId )) {
            var endpointResult = await _tableClient.GetByIdAsync(crmEndpointId,"endpoint");
            if (endpointResult == null) {
                return null;
            }
            RegisterConnectorInRegistry(endpointResult.Entity);
        }
        return _crmEndpointById[crmEndpointId];
    }


    private void RegisterConnectorInRegistry(CrmEndpoint crmEndpoint) {
        var connector = new DynamicsConnectorBuilder().WithResource(crmEndpoint.CrmURL).WithApplicationId(crmEndpoint.ApplicationId).WithApplicationSecret(crmEndpoint.ClientSecret).WithTenant(crmEndpoint.TenantId).Build();
        _connectorRegistry.Add(crmEndpoint.Id,connector);
        _crmEndpointById.Add(crmEndpoint.Id, crmEndpoint.CrmURL);
    }

    public async Task<DynamicsConnector> RegisterNewConnector(CrmEndpoint crmEndpoint) {
        var endpointResult = await _tableClient.InsertOrReplaceAsync(crmEndpoint.Id,"endpoint",crmEndpoint);
        RegisterConnectorInRegistry(crmEndpoint);
        return _connectorRegistry[crmEndpoint.Id];
    }
    public async Task<DynamicsConnector> UpdateConnector(CrmEndpoint crmEndpoint) {
        var endpointResult = await _tableClient.InsertOrMergeAsync(crmEndpoint.Id,"endpoint",crmEndpoint);
        RegisterConnectorInRegistry(crmEndpoint);
        return _connectorRegistry[crmEndpoint.Id];
    }
    public async Task<List<string>> GetAllCrmEnpointIds() {
        var all = await _tableClient.GetAllAsync("endpoint");
        return all.Select(ep=>ep.Entity.Id).ToList();
    }
}