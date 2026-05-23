namespace O24OpenAPI.Contracts.Configuration.Client;

public class ServiceInfo
{
    public readonly long WFO_GRPC_TIMEOUT_IN_SECONDS = 60L;

    public string service_code { get; set; } = default!;

    public string service_name { get; set; } = default!;

    public string service_grpc_url { get; set; } = default!;

    public string service_status { get; set; } = default!;

    public string? service_grpc_active { get; set; } = "active";

    public long service_grpc_timeout_seconds { get; set; }

    public long service_ping_interval_seconds { get; set; }

    public string? service_static_token { get; set; }

    public string broker_virtual_host { get; set; } = default!;

    private string _brokerHostname = default!;

    public string broker_hostname
    {
        get => _brokerHostname;
        set => _brokerHostname = value;
    }

    public string broker_user_name { get; set; } = default!;

    public string broker_user_password { get; set; } = default!;

    public string broker_queue_name { get; set; } = default!;

    public string event_queue_name { get; set; } = default!;

    public int broker_port { get; set; }

    public string? o24openapi_grpc_url { get; set; }

    public long o24openapi_server_time { get; set; }

    public long o24openapi_grpc_timeout_seconds { get; set; } = 60;

    public long workflow_execution_timeout_seconds { get; set; } = 300;

    public string? ssl_active { get; set; }

    public string? ssl_cert_pass_pharse { get; set; }

    public string? ssl_cert_servername { get; set; }

    public string? ssl_cert_base64 { get; set; }

    public long concurrent_threads { get; set; }

    public long broker_reconnect_interval_in_seconds { get; set; }

    public int grpc_Max_Send_Message_Size_In_MB { get; set; } = 4;

    public int grpc_Max_Receive_Message_Size_In_MB { get; set; } = 4;

    public string? log_write_grpc_log { get; set; } = "N";

    public string? redis_server_name { get; set; }

    public int redis_server_port { get; set; } = 6379;

    public bool is_tracking { get; set; }

    public ServiceInfo() { }

    public ServiceInfo(ServiceInfoConfiguration _config)
    {
        service_code = _config.ServiceCode;
        service_name = _config.ServiceName;
        service_grpc_url = _config.ServiceGrpcUrl;
        service_status = _config.ServiceStatus;
        service_grpc_active = _config.ServiceGrpcActive;
        service_grpc_timeout_seconds = _config.ServiceGrpcTimeoutSeconds;
        service_ping_interval_seconds = _config.ServicePingIntervalSeconds;
        service_static_token = _config.ServiceStaticToken;
        broker_virtual_host = _config.BrokerVirtualHost;
        broker_hostname = _config.BrokerHostname;
        broker_user_name = _config.BrokerUsername;
        broker_user_password = _config.BrokerPassword;
        broker_queue_name = _config.BrokerQueueName;
        event_queue_name = _config.EventQueueName;
        broker_port = _config.BrokerPort;
        ssl_active = _config.SslActive;
        ssl_cert_pass_pharse = _config.SslCertPassPhrase;
        ssl_cert_servername = _config.SslCertServerName;
        ssl_cert_base64 = _config.SslCertBase64;
        concurrent_threads = _config.ConcurrentThreads;
        broker_reconnect_interval_in_seconds = _config.BrokerReconnectIntervalInSeconds;
        grpc_Max_Send_Message_Size_In_MB = _config.GrpcMaxSendMessageSizeInMB;
        grpc_Max_Receive_Message_Size_In_MB = _config.GrpcMaxReceiveMessageSizeInMB;
        log_write_grpc_log = _config.LogWriteGrpcLog;
        redis_server_name = _config.RedisServerName;
        redis_server_port = _config.RedisServerPort;
        is_tracking = _config.IsTracking;
    }
}
