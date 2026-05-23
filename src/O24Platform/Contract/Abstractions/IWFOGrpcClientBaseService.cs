using O24OpenAPI.Contracts.Configuration.Client;

namespace O24OpenAPI.Contracts.Abstractions;

public interface IWFOGrpcClientBaseService
{
    Task<string> ExecuteWorkflowAsync(string workflowInputJson);
    Task<string> RegisterServiceGrpcEndpointAsync(
        string serviceCode,
        string serviceHandleName,
        string grpcEndpointURL,
        string instanceID,
        string serviceAssemblyName
    );
    Task<ServiceInfo> QueryServiceInfoAsync(
        string fromServiceCode,
        string toServiceCode,
        string instanceID
    );
    Task<ServiceInfo> GetServiceInstanceByServiceHandleNameAsync(string serviceHandleName);
}
