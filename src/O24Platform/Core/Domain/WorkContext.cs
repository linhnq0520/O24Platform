using O24OpenAPI.Core.Domain.Users;
using O24OpenAPI.Core.Extensions;

namespace O24OpenAPI.Core.Domain;

public class WorkContext
{
    public string CurrentChannel { get; private set; } = default!;
    public UserContext UserContext { get; private set; } = new();
    public string WorkingLanguage { get; private set; } = "en";
    public string ExecutionLogId
    {
        get { return ExecutionId; }
    }
    public string ExecutionId { get; private set; } = Guid.NewGuid().ToString("N");
    public Dictionary<string, object> DeviceRequest { get; set; } = [];

    public void SetWorkContext(WorkContext workContext)
    {
        if (workContext == null)
        {
            return;
        }

        CurrentChannel = workContext.CurrentChannel;
        UserContext = workContext.UserContext;
        WorkingLanguage = workContext.WorkingLanguage;
        ExecutionId = workContext.ExecutionId;
    }

    public void SetWorkContext(WorkContextTemplate? workContext)
    {
        if (workContext == null)
        {
            return;
        }

        CurrentChannel = workContext.CurrentChannel ?? CurrentChannel;
        UserContext.SetUserContext(workContext.UserContext ?? new UserContextTemplate());
        WorkingLanguage = workContext.WorkingLanguage ?? WorkingLanguage;
        ExecutionId = workContext.ExecutionId ?? ExecutionId;
    }

    public void SetCurrentChannel(string? channel)
    {
        if (!string.IsNullOrWhiteSpace(channel))
        {
            CurrentChannel = channel;
        }
    }

    public void SetUserContext(UserContext userContext)
    {
        if (userContext == null)
        {
            return;
        }

        UserContext = userContext;
    }

    public void SetWorkingLanguage(string? language)
    {
        if (!string.IsNullOrWhiteSpace(language))
        {
            WorkingLanguage = language;
        }
    }

    public void SetExecutionId(string executionId)
    {
        if (executionId.HasValue())
        {
            ExecutionId = executionId;
        }
    }

    public void SetDeviceRequest(Dictionary<string, object> deviceRequest)
    {
        if (deviceRequest == null)
        {
            return;
        }

        DeviceRequest = deviceRequest;
    }
}

public class WorkContextTemplate
{
    public string? CurrentChannel { get; set; }
    public UserContextTemplate? UserContext { get; set; }
    public string? WorkingLanguage { get; set; }
    public string? ExecutionId { get; set; }
}
