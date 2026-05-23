using System.Text.Json;
using Linh.CodeEngine.Core;
using LinKit.Core.Abstractions;
using LinKit.Core.Cqrs;
using LinKit.Json.Runtime;
using O24OpenAPI.Client.Enums;
using O24OpenAPI.Client.Scheme.Workflow;
using O24OpenAPI.Core;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Data;
using O24OpenAPI.Framework.Abstractions;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Exceptions;
using O24OpenAPI.Framework.Extensions;
using O24OpenAPI.Framework.Helpers;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Repositories;
using O24OpenAPI.Framework.Services.Configuration;
using O24OpenAPI.Framework.Services.Queue;
using O24OpenAPI.Logging.Helpers;

namespace O24OpenAPI.Framework.Services;

public class StoredProcedureStepConfig
{
    public string StoredProcedureName { get; set; } = "";

    public bool IsReplacePostingInContext { get; set; } = false;
}

public interface IO24OpenAPIServiceManager
{
    Task<WFScheme> ConsumeWorkflow(WFScheme workflow);
}

[RegisterService(Lifetime.Scoped)]
public class O24OpenAPIServiceManager(
    IEntityAuditRepository entityAuditRepository,
    IO24OpenAPIDataProvider dataProvider
) : IO24OpenAPIServiceManager
{
    public async Task<WFScheme> ConsumeWorkflow(WFScheme workflow)
    {
        if (workflow == null)
        {
            return null;
        }

        try
        {
            WorkContext workContext = EngineContext.Current.Resolve<WorkContext>();
            workflow.SetWorkContext(workContext);

            if (workflow.request.request_header.processing_version == ProcessNumber.ExecuteCommand)
            {
                return await HandleExecuteCommand(workflow);
            }

            string stepCode = workflow.request.request_header.step_code;
            O24OpenAPIService stepConfig = await GetMappingByCode(stepCode);

            if (workflow.request.request_header.processing_version == ProcessNumber.StoredProcedure)
            {
                return await HandleStoredProcedureWorkflow(workflow, stepConfig);
            }

            return await HandleStandardWorkflow(workflow, stepConfig);
        }
        catch (Exception ex)
        {
            return await ErrorWorkflow(workflow, ex);
        }
    }

    private static bool TryParseStoredProcedureParameter(
        string input,
        out StoredProcedureStepConfig result
    )
    {
        try
        {
            result = JsonSerializer.Deserialize<StoredProcedureStepConfig>(input);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    private static async Task<WFScheme> HandleStoredProcedureWorkflow(
        WFScheme workflow,
        O24OpenAPIService mapping
    )
    {
        try
        {
            string fullClassName = mapping.FullClassName;
            string methodName = mapping.MethodName;
            BaseTransactionModel baseTranModel = await workflow.ToModel<BaseTransactionModel>();
            string json = JsonSerializer.Serialize(baseTranModel);
            Dictionary<string, object> dictionary = JsonSerializer.Deserialize<
                Dictionary<string, object>
            >(json);
            workflow.request.request_header.tx_context = dictionary;
            Console.WriteLine($"wfScheme == {JsonSerializer.Serialize(workflow)}");
            if (
                TryParseStoredProcedureParameter(
                    fullClassName,
                    out StoredProcedureStepConfig parameterConfig
                )
            )
            {
                return await BaseQueue.Invoke2<BaseTransactionModel>(workflow, parameterConfig);
            }
            return await workflow.InvokeAsync(fullClassName, methodName);
        }
        catch (Exception ex)
        {
            return await ErrorWorkflow(workflow, ex);
        }
    }

    private async Task<WFScheme> HandleStandardWorkflow(
        WFScheme workflow,
        O24OpenAPIService mapping
    )
    {
        try
        {
            if (
                workflow.request.request_header.is_compensated != "Y"
                || (
                    workflow.request.request_header.is_compensated == "Y"
                    && mapping.CheckAutoReverse() != true
                )
            )
            {
                if (mapping.IsModuleExecute == true)
                {
                    object executionResult = await DynamicCodeEngine.ExecuteAsync(
                        moduleName: mapping.FullClassName,
                        methodName: mapping.MethodName,
                        parameters: [workflow]
                    );
                    return executionResult as WFScheme;
                }

                IWorkflowStepInvoker stepInvoker;
                IMediator mediator;

                try
                {
                    stepInvoker =
                        EngineContext.Current.Resolve<IWorkflowStepInvoker>(mapping.MediatorKey)
                        ?? throw new InvalidOperationException(
                            $"IWorkflowStepInvoker resolved null. Key='{mapping.MediatorKey}'"
                        );

                    mediator = EngineContext.Current.Resolve<IMediator>(mapping.MediatorKey);
                }
                catch (KeyNotFoundException ex)
                {
                    BusinessLogHelper.Error(
                        ex,
                        $"Cannot resolve workflow services. Key='{mapping.MediatorKey}', Step='{mapping.StepCode}'."
                    );
                    return await workflow.InvokeAsync(mapping.FullClassName, mapping.MethodName);
                }
                catch (InvalidOperationException ex)
                {
                    BusinessLogHelper.Error(
                        ex,
                        $"Cannot resolve workflow services. Key='{mapping.MediatorKey}', Step='{mapping.StepCode}'."
                    );
                    return await workflow.InvokeAsync(mapping.FullClassName, mapping.MethodName);
                }

                return await BaseQueue.Invoke<BaseTransactionModel>(
                    workflow,
                    () =>
                        stepInvoker.InvokeAsync(
                            mapping.StepCode,
                            workflow,
                            mediator,
                            CancellationToken.None
                        )
                );
            }
            else
            {
                var entityAudits = await entityAuditRepository.GetByExecutionIdAsync(
                    workflow.request.request_header.execution_id
                );
                string revertSql = await EntityAuditReverter.GenerateRevertSqlBlock(
                    entityAudits,
                    workflow.request.request_header.execution_id
                );

                if (!string.IsNullOrWhiteSpace(revertSql))
                {
                    await dataProvider.ExecuteNonQuery(revertSql);
                    _ = entityAuditRepository.BulkDelete(entityAudits);
                }
                workflow.response.status = WFScheme.RESPONSE.EnumResponseStatus.SUCCESS;
                workflow.response.data = JsonSerializer.Serialize(new { });
                return workflow;
            }
        }
        catch (Exception ex)
        {
            return await ErrorWorkflow(workflow, ex);
        }
    }

    private static async Task<WFScheme> HandleExecuteCommand(WFScheme workflow)
    {
        try
        {
            var invoker = new CommandInvoker();
            return await invoker.InvokeCommand(workflow);
        }
        catch (Exception ex)
        {
            return await ErrorWorkflow(workflow, ex);
        }
    }

    private static async Task<O24OpenAPIService> GetMappingByCode(string stepCode)
    {
        IO24OpenAPIMappingService service =
            EngineContext.Current.Resolve<IO24OpenAPIMappingService>()
            ?? throw new O24OpenAPIException("Mapping service could not be resolved.");

        O24OpenAPIService mapping =
            await service.GetByStepCode(stepCode)
            ?? throw new O24OpenAPIException(
                $"Step code '{stepCode}' not found in O24OpenAPIService. Please check service registration."
            );

        return mapping;
    }

    private static async Task<WFScheme> ErrorWorkflow(WFScheme workflow, Exception e)
    {
        string errorCode = "ERROR";
        Exception ex = e;
        if (e.InnerException != null)
        {
            ex = e.InnerException;
        }
        string errorMessage = ex.Message;
        if (ex is O24OpenAPIException o24OpenAPIException)
        {
            string err = o24OpenAPIException.ErrorCode;
            if (!string.IsNullOrEmpty(err))
            {
                errorCode = err;
            }
        }
        if (ex is O24Exception o24Exception)
        {
            string err = o24Exception.ErrorCode;
            if (!string.IsNullOrWhiteSpace(err))
            {
                errorCode = err;
                ex = await o24Exception.ToO24OpenAPIExceptionAsync();
                errorMessage = ex.Message;
            }
        }
        else if (ex is ExceptionWithNextAction exceptionWithNextAction)
        {
            string err = exceptionWithNextAction.ErrorCode;
            if (!string.IsNullOrEmpty(err))
            {
                errorCode = err;
                errorMessage = ex.Message;
            }

            string action = exceptionWithNextAction.NextAction;

            if (!string.IsNullOrEmpty(action))
            {
                workflow.response.error_next_action = action;
            }
        }
        else
        {
            try
            {
                StoreProcedureErrorModel errorResponse =
                    JsonSerializer.Deserialize<StoreProcedureErrorModel>(
                        ex.Message,
                        new JsonSerializerOptions
                        {
                            AllowTrailingCommas = true,
                            PropertyNameCaseInsensitive = true,
                        }
                    );

                if (
                    errorResponse != null
                    && !string.IsNullOrWhiteSpace(errorResponse.error_message)
                )
                {
                    errorCode = !string.IsNullOrEmpty(errorResponse.error_code)
                        ? errorResponse.error_code
                        : errorCode;
                    errorMessage = errorResponse.error_message;
                }
            }
            catch (JsonException) { }
            catch (Exception) { }
        }
        workflow.response.error_code = errorCode;
        workflow.response.error_message = errorMessage;
        workflow.response.raw_error = e.ToString();
        try
        {
            var errorDict = ex.ToDictionary("ERROR_MESSAGE");
            workflow.response.data =
                errorDict?.ToJson() ?? JsonSerializer.Serialize(new { ERROR_MESSAGE = ex.Message });
        }
        catch
        {
            workflow.response.data = JsonSerializer.Serialize(new { ERROR_MESSAGE = ex.Message });
        }
        workflow.response.status = WFScheme.RESPONSE.EnumResponseStatus.ERROR;
        if (ex is NotFoundException exception)
        {
            workflow.response.data = exception.BaseModel;
        }

        var executionStepId = workflow.request.request_header.execution_id;
        BusinessLogHelper.Error(
            ex,
            $"{workflow.request.request_header.step_code} - {executionStepId}: {errorCode} - {errorMessage}"
        );
        return workflow;
    }
}
