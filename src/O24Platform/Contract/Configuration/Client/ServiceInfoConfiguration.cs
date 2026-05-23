using O24OpenAPI.Core.Configuration;

namespace O24OpenAPI.Contracts.Configuration.Client;

public class ServiceInfoConfiguration : IConfig
{
    /// <summary>
    /// Gets or sets the value of the service code
    /// /// </summary>
    public string ServiceCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the service name
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the service grpc url
    /// </summary>
    public string ServiceGrpcUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the service status
    /// </summary>
    public string ServiceStatus { get; set; } = "A";

    /// <summary>
    /// Gets or sets the value of the service grpc active
    /// </summary>
    public string ServiceGrpcActive { get; set; } = "Y";

    /// <summary>
    /// Gets or sets the value of the service grpc timeout seconds
    /// </summary>
    public long ServiceGrpcTimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the value of the service ping interval seconds
    /// </summary>
    public long ServicePingIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the value of the service static token
    /// </summary>
    public string? ServiceStaticToken { get; set; }

    /// <summary>
    /// Gets or sets the value of the broker virtual host
    /// </summary>
    public string BrokerVirtualHost { get; set; } = "/";

    /// <summary>
    /// Gets or sets the value of the broker hostname
    /// </summary>
    public string BrokerHostname { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the broker username
    /// </summary>
    public string BrokerUsername { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the broker password
    /// </summary>
    public string BrokerPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the broker queue name
    /// </summary>
    public string BrokerQueueName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the event queue name
    /// </summary>
    public string EventQueueName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the broker port
    /// </summary>
    public int BrokerPort { get; set; } = 5672;

    /// <summary>
    /// Gets or sets the value of the ssl active
    /// </summary>
    public string SslActive { get; set; } = "N";

    /// <summary>
    /// Gets or sets the value of the ssl cert pass phrase
    /// </summary>
    public string? SslCertPassPhrase { get; set; }

    /// <summary>
    /// Gets or sets the value of the ssl cert server name
    /// </summary>
    public string? SslCertServerName { get; set; }

    /// <summary>
    /// Gets or sets the value of the ssl cert base 64
    /// </summary>
    public string? SslCertBase64 { get; set; }

    /// <summary>
    /// Gets or sets the value of the concurrent threads
    /// </summary>
    public long ConcurrentThreads { get; set; } = 10;

    /// <summary>
    /// Gets or sets the value of the broker reconnect interval in seconds
    /// </summary>
    public long BrokerReconnectIntervalInSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the value of the grpc max send message size in mb
    /// </summary>
    public int GrpcMaxSendMessageSizeInMB { get; set; } = 4;

    /// <summary>
    /// Gets or sets the value of the grpc max receive message size in mb
    /// </summary>
    public int GrpcMaxReceiveMessageSizeInMB { get; set; } = 4;

    /// <summary>
    /// Gets or sets the value of the log write grpc log
    /// </summary>
    public string LogWriteGrpcLog { get; set; } = "N";

    /// <summary>
    /// Gets or sets the value of the redis server name
    /// </summary>
    public string RedisServerName { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the redis server port
    /// </summary>
    public int RedisServerPort { get; set; } = 6379;

    /// <summary>
    /// Gets or sets the value of the is tracking
    /// </summary>
    public bool IsTracking { get; set; } = false;
}
