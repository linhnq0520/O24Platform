using System.Text.Json.Nodes;
using FluentValidation.Results;
using LinKit.Json.Runtime;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using O24OpenAPI.Client.Scheme.Workflow;
using O24OpenAPI.Contracts.Configuration.Client;
using O24OpenAPI.Core;
using O24OpenAPI.Core.Helper;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.DBContext;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Extensions;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Services.Configuration;

namespace O24OpenAPI.Framework.Services.Queue;

public abstract class BaseQueue
{
    public static async Task<WFScheme> Invoke<TModel>(
        WFScheme workflow,
        Func<Task<object>> acquire,
        string keyName = "",
        bool removeSensitive = true,
        bool useTransactionScope = true,
        bool singleThread = true
    )
        where TModel : BaseTransactionModel
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(acquire);
        ArgumentNullException.ThrowIfNull(workflow.request?.request_header);

        object returnObject = null;
        O24OpenAPIService stepConfig = null;

        try
        {
            if (
                workflow.request.request_header.processing_version
                != Client.Enums.ProcessNumber.ExecuteCommand
            )
            {
                string stepCode = workflow.request.request_header.step_code;
                var mappingService = EngineContext.Current.Resolve<IO24OpenAPIMappingService>();
                stepConfig =
                    await mappingService.GetByStepCode(stepCode)
                    ?? throw new O24OpenAPIException($"Mapping not found for stepCode: {stepCode}");
            }

            if (singleThread && stepConfig != null && !stepConfig.IsInquiry)
            {
                useTransactionScope = true;
            }
            else if (stepConfig == null || stepConfig.IsInquiry)
            {
                useTransactionScope = false;
            }
            returnObject = await DoTransaction(
                workflow: workflow,
                acquire: acquire,
                isReverse: workflow.request.request_header.IsReversal(),
                isCompensated: false
            );

            if (returnObject is not null)
            {
                if (returnObject is JsonNode jsonNode)
                {
                    returnObject = jsonNode.ToJson()?.FromJson<object>();
                }
            }
            return string.IsNullOrEmpty(keyName)
                ? Success(workflow, returnObject)
                : Success(workflow, keyName, returnObject);
        }
        catch (SqlException ex) when (ex.Number == 1205)
        {
            throw new O24OpenAPIException(
                "system_busy",
                "System is busy now, please try again later"
            );
        }
        catch
        {
            throw;
        }
    }

    public static async Task<WFScheme> Invoke2<TRequestModel>(
        WFScheme workflow,
        StoredProcedureStepConfig storedProcedureParameters,
        Func<Task<bool>> checkValidation = null
    )
        where TRequestModel : BaseTransactionModel
    {
        return await Invoke2<TRequestModel>(
            workflow,
            storedProcedureParameters.StoredProcedureName,
            storedProcedureParameters.IsReplacePostingInContext,
            null
        );
    }

    public static async Task<WFScheme> Invoke2<TRequestModel>(
        WFScheme workflow,
        string storedProcedureName,
        bool replacePosting = false,
        Func<Task<bool>> checkValidation = null
    )
        where TRequestModel : BaseTransactionModel
    {
        // Early validation
        if (string.IsNullOrEmpty(storedProcedureName))
        {
            throw new ArgumentNullException(nameof(storedProcedureName));
        }

        // Calculate timeout once
        var (commandTimeout, errorMessage) = CalculateTimeout(workflow);
        if (commandTimeout <= 0)
        {
            throw new O24OpenAPIException(errorMessage);
        }

        // Validation check
        if (checkValidation != null)
        {
            try
            {
                if (!await checkValidation())
                {
                    return CreateSuccessResponse(workflow);
                }
            }
            catch (Exception ex)
            {
                throw new O24OpenAPIException(
                    $"Error occurs when executing service [{Singleton<O24OpenAPIClientConfiguration>.Instance.YourServiceID}] validation : {ex.Message}"
                );
            }
        }

        // Database operation
        using var dbContext = new ServiceDBContext(commandTimeout);
        var baseTransactionModel = await workflow.ToModel<BaseTransactionModel>();
        workflow.request.request_header.tx_context["base_transaction_model"] = baseTransactionModel;

        var response = dbContext.CallServiceStoredProcedure(
            storedProcedureName,
            workflow,
            workflow.request.request_header.IsReversal()
                ? ServiceDBContext.EnumIsReverse.R
                : ServiceDBContext.EnumIsReverse.N
        );

        // Format response
        response.response.status = WFScheme.RESPONSE.EnumResponseStatus.SUCCESS;
        response.response.data = JToken
            .Parse(response.response.data.ToString())
            .ConvertKeysToSnakeCase();

        return response;
    }

    public static async Task<WFScheme> InvokeCommandQuery(WFScheme workflow)
    {
        var model = await workflow.ToModel<ModelWithQuery>();
        if (workflow == null)
        {
            throw new ArgumentNullException(nameof(workflow), "Workflow scheme cannot be null.");
        }

        var result = await Invoke<ModelWithQuery>(
            workflow,
            async () =>
            {
                var executeQueryService = EngineContext.Current.Resolve<IExecuteQueryService>();
                return await executeQueryService.SqlQuery(model);
            }
        );
        return result;
    }

    private static (int timeout, string errorMessage) CalculateTimeout(WFScheme workflow)
    {
        var stepTimeout = workflow.request.request_header.step_timeout;
        var sendingUTC = workflow.request.request_header.utc_send_time;
        var currentTime = CommonHelper.ConvertToUnixTimestamp(DateTime.UtcNow);
        var trafficTime = currentTime - sendingUTC;

        var apiSettings = EngineContext.Current.Resolve<WebApiSettings>();
        var bufferTime = apiSettings.BufferTime == 0L ? 500 : apiSettings.BufferTime;

        var isReverse = workflow.request.request_header.IsReversal();
        var timeout = (int)(
            !isReverse
                ? ((stepTimeout - trafficTime * 2 - bufferTime) / 1000)
                : (stepTimeout / 1000)
        );

        var errorMessage =
            timeout <= 0
                ? $"The message sent to service [{Singleton<O24OpenAPIClientConfiguration>.Instance.YourServiceID}] was timeout"
                : string.Empty;

        return (timeout, errorMessage);
    }

    private static WFScheme CreateSuccessResponse(WFScheme workflow)
    {
        workflow.response.status = WFScheme.RESPONSE.EnumResponseStatus.SUCCESS;
        return workflow;
    }

    private static Task<object> DoTransaction(
        WFScheme workflow,
        Func<Task<object>> acquire,
        bool isReverse,
        bool isCompensated = false
    )
    {
        long timeoutSec = workflow.request.request_header.step_timeout;
        long startSec = workflow.request.request_header.utc_send_time;
        long currentTime = CommonHelper.GetCurrentDateAsLongNumber();
        long trafficTime = currentTime - startSec;

        WebApiSettings apiSettings = EngineContext.Current.Resolve<WebApiSettings>();
        long bufferTime = (apiSettings.BufferTime == 0L) ? 500 : apiSettings.BufferTime;

        if (isCompensated)
        {
            workflow.request.request_header.is_compensated = "Y";
        }

        if (timeoutSec - 2 * trafficTime - bufferTime <= 0 && !isReverse && !isCompensated)
        {
            throw new O24OpenAPIException("MessageOutdate", "Message is outdate");
        }

        return acquire();
    }

    private static WFScheme Success(WFScheme workflow, string name, object content)
    {
        workflow.response.data = content.ToDictionary(name);
        workflow.response.status = WFScheme.RESPONSE.EnumResponseStatus.SUCCESS;
        return workflow;
    }

    private static WFScheme Success(WFScheme workflow, object content)
    {
        if (content is string || content is ValueType) // Nếu là kiểu dữ liệu cơ bản
        {
            workflow.response.data = new { data = content };
        }
        else if ((content is IEnumerable<object> || content is Array) && content is not JObject) // Nếu là List, Array, IEnumerable
        {
            workflow.response.data = new { data = content };
        }
        else
        {
            workflow.response.data = content; // Nếu là object, JObject, Dictionary thì giữ nguyên
        }

        workflow.response.status = WFScheme.RESPONSE.EnumResponseStatus.SUCCESS;
        return workflow;
    }

    private static WFScheme Error<TModel>(
        WFScheme workflow,
        ValidationResult validationResult,
        string errorMessage = ""
    )
        where TModel : BaseTransactionModel
    {
        workflow.response.data = new { };
        if (string.IsNullOrEmpty(errorMessage))
        {
            workflow.response.error_code = "ERRROR";
            workflow.response.error_message = string.Join("\n", validationResult.Errors);
        }
        else
        {
            workflow.response.error_code = "ERROR";
            workflow.response.error_message = errorMessage;
        }
        if (validationResult.Errors.Any())
        {
            workflow.response.error_code = string.Join(
                "\n",
                validationResult.Errors.Select(e => e.ErrorCode)
            );
        }
        workflow.response.data = null;
        workflow.response.status = WFScheme.RESPONSE.EnumResponseStatus.ERROR;

        return workflow;
    }
}
