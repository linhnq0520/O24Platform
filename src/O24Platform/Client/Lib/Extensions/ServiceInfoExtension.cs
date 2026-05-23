using O24OpenAPI.Contracts.Abstractions;
using O24OpenAPI.Contracts.Configuration.Client;
using O24OpenAPI.Core.Infrastructure;

namespace O24OpenAPI.Client.Lib.Extensions;

public static class ServiceInfoExtension
{
    public static async Task QueryServiceInfo(
        this ServiceInfo serviceInfo,
        string pFromServiceCode,
        string pToServiceCode,
        string pInstanceID
    )
    {
        IWFOGrpcClientBaseService wfoGrpcClientService =
            EngineContext.Current.Resolve<IWFOGrpcClientBaseService>()
            ?? throw new InvalidOperationException(
                "IWFOGrpcClientBaseService is not registered in the DI container."
            );

        ServiceInfo serviceInfoResult = await wfoGrpcClientService.QueryServiceInfoAsync(
            fromServiceCode: pFromServiceCode,
            toServiceCode: pToServiceCode,
            instanceID: pInstanceID
        );
        serviceInfo.service_code = serviceInfoResult.service_code;
        serviceInfo.service_name = serviceInfoResult.service_name;
        serviceInfo.service_grpc_url = serviceInfoResult.service_grpc_url;
        serviceInfo.service_status = serviceInfoResult.service_status;
        serviceInfo.service_grpc_active = serviceInfoResult.service_grpc_active;
        serviceInfo.service_grpc_timeout_seconds = serviceInfoResult.service_grpc_timeout_seconds;
        serviceInfo.service_ping_interval_seconds = serviceInfoResult.service_ping_interval_seconds;
        serviceInfo.service_static_token = serviceInfoResult.service_static_token;
        serviceInfo.broker_virtual_host = serviceInfoResult.broker_virtual_host;
        serviceInfo.broker_hostname = serviceInfoResult.broker_hostname;
        serviceInfo.broker_user_name = serviceInfoResult.broker_user_name;
        serviceInfo.broker_user_name = serviceInfoResult.broker_user_name;
        serviceInfo.broker_user_password = serviceInfoResult.broker_user_password;
        serviceInfo.broker_queue_name = serviceInfoResult.broker_queue_name;
        serviceInfo.event_queue_name = serviceInfoResult.event_queue_name;
        serviceInfo.broker_port = serviceInfoResult.broker_port;
        serviceInfo.concurrent_threads = serviceInfoResult.concurrent_threads;
        serviceInfo.ssl_active = serviceInfoResult.ssl_active;
        if (serviceInfo.ssl_active?.Equals("Y", StringComparison.OrdinalIgnoreCase) == true)
        {
            serviceInfo.ssl_cert_pass_pharse = serviceInfoResult.ssl_cert_pass_pharse;
            serviceInfo.ssl_cert_servername = serviceInfoResult.ssl_cert_servername;
            serviceInfo.ssl_cert_base64 = serviceInfoResult.ssl_cert_base64;
        }
        serviceInfo.o24openapi_grpc_url = serviceInfoResult.o24openapi_grpc_url;
        serviceInfo.o24openapi_server_time = serviceInfoResult.o24openapi_server_time;
        serviceInfo.o24openapi_grpc_timeout_seconds =
            serviceInfoResult.o24openapi_grpc_timeout_seconds;
        serviceInfo.broker_reconnect_interval_in_seconds =
            serviceInfoResult.broker_reconnect_interval_in_seconds;
        serviceInfo.grpc_Max_Send_Message_Size_In_MB =
            serviceInfoResult.grpc_Max_Send_Message_Size_In_MB;
        serviceInfo.grpc_Max_Receive_Message_Size_In_MB =
            serviceInfoResult.grpc_Max_Receive_Message_Size_In_MB;
        serviceInfo.log_write_grpc_log = serviceInfoResult.log_write_grpc_log;
        serviceInfo.redis_server_name = serviceInfoResult.redis_server_name;
        serviceInfo.redis_server_port = serviceInfoResult.redis_server_port;
        serviceInfo.is_tracking = serviceInfoResult.is_tracking;
        serviceInfo.workflow_execution_timeout_seconds =
            serviceInfoResult.workflow_execution_timeout_seconds;
    }
}
