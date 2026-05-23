namespace O24OpenAPI.Core.Domain.Users;

public class UserContext
{
    public string? UserId { get; private set; } = string.Empty;
    public string? UserCode { get; private set; } = string.Empty;
    public string? UserName { get; private set; } = string.Empty;
    public string? UserChannel { get; private set; } = string.Empty;
    public string? LoginName { get; private set; } = string.Empty;
    public string? UserBaseCurrency { get; private set; } = string.Empty;
    public string? ContractNumber { get; private set; } = string.Empty;

    public void SetUserContext(UserContext userContext)
    {
        if (userContext == null)
        {
            return;
        }

        UserId = userContext.UserId;
        UserCode = userContext.UserCode;
        UserName = userContext.UserName;
        UserChannel = userContext.UserChannel;
        LoginName = userContext.LoginName;
        UserBaseCurrency = userContext.UserBaseCurrency;
        ContractNumber = userContext.ContractNumber;
    }

    public void SetUserContext(UserContextTemplate userContext)
    {
        if (userContext == null)
        {
            return;
        }

        UserId = userContext.UserId;
        UserCode = userContext.UserCode;
        UserName = userContext.UserName;
        UserChannel = userContext.UserChannel;
        LoginName = userContext.LoginName;
        UserBaseCurrency = userContext.UserBaseCurrency;
        ContractNumber = userContext.ContractNumber;
    }

    public void SetUserId(string? userId)
    {
        UserId = userId ?? UserId;
    }

    public void SetUserCode(string? userCode)
    {
        UserCode = userCode ?? UserCode;
    }

    public void SetUserName(string? userName)
    {
        UserName = userName ?? UserName;
    }

    public void SetUserChannel(string? userChannel)
    {
        UserChannel = userChannel ?? UserChannel;
    }

    public void SetLoginName(string? loginName)
    {
        LoginName = loginName ?? LoginName;
    }

    public void SetUserBaseCurrency(string? userBaseCurrency)
    {
        UserBaseCurrency = userBaseCurrency ?? UserBaseCurrency;
    }

    public void SetContractNumber(string? contractNumber)
    {
        ContractNumber = contractNumber ?? ContractNumber;
    }
}

public class UserContextTemplate
{
    public string? UserId { get; set; }
    public string? UserCode { get; set; }
    public string? UserName { get; set; }
    public string? UserChannel { get; set; }
    public string? LoginName { get; set; }
    public string? UserBaseCurrency { get; set; }
    public string? ContractNumber { get; set; }
}
