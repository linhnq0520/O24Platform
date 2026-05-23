namespace O24OpenAPI.Logging.Enums;

/// <summary>
/// Defines the type of communication channel being logged.
/// </summary>
public enum LogType
{
    RestApi,
    Grpc,
    RabbitMq,
    Business,
    ScheduleTask,
}
