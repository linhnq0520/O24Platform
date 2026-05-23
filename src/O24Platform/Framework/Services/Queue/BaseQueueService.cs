using System.Transactions;
using FluentValidation.Results;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using O24OpenAPI.Client.Scheme.Workflow;
using O24OpenAPI.Contracts.Configuration.Client;
using O24OpenAPI.Core;
using O24OpenAPI.Core.Helper;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.DBContext;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Logging.Helpers;

namespace O24OpenAPI.Framework.Services.Queue;

/// <summary>
/// The base queue service class
/// </summary>
public abstract class BaseQueueService
{
    /// <summary>
    /// The transaction date
    /// </summary>
    public const string TRANSACTION_DATE = "transaction_date";

    /// <summary>
    /// The transaction id
    /// </summary>
    public const string TRANSACTION_ID = "transaction_id";

    /// <summary>
    /// Successes the workflow
    /// </summary>
    /// <param name="workflow">The workflow</param>
    /// <param name="name">The name</param>
    /// <param name="content">The content</param>
    /// <returns>The workflow</returns>
    public static WorkflowScheme Success(WorkflowScheme workflow, string name, object content)
    {
        workflow.Response.Data = content.ToDictionary(name);
        workflow.Response.Status = WorkflowScheme.RESPONSE.EnumResponseStatus.SUCCESS;
        return workflow;
    }

    /// <summary>
    /// Successes the workflow
    /// </summary>
    /// <param name="workflow">The workflow</param>
    /// <param name="content">The content</param>
    /// <returns>The workflow</returns>
    public static WorkflowScheme Success(WorkflowScheme workflow, object content)
    {
        workflow.Response.Data = content;
        workflow.Response.Status = WorkflowScheme.RESPONSE.EnumResponseStatus.SUCCESS;
        return workflow;
    }

    /// <summary>
    /// Errors the workflow
    /// </summary>
    /// <typeparam name="TModel">The model</typeparam>
    /// <param name="workflow">The workflow</param>
    /// <param name="validationResult">The validation result</param>
    /// <param name="errorMessage">The error message</param>
    /// <returns>The workflow</returns>
    public static WorkflowScheme Error<TModel>(
        WorkflowScheme workflow,
        ValidationResult validationResult,
        string errorMessage = ""
    )
        where TModel : BaseTransactionModel
    {
        workflow.Response.Data = (new { });
        if (string.IsNullOrEmpty(errorMessage))
        {
            workflow.Response.ErrorCode = "ERRROR";
            workflow.Response.ErrorMessage = string.Join("\n", validationResult.Errors);
        }
        else
        {
            workflow.Response.ErrorCode = "ERROR";
            workflow.Response.ErrorMessage = errorMessage;
        }

        if (validationResult.Errors.Any())
        {
            workflow.Response.ErrorCode = string.Join(
                "\n",
                validationResult.Errors.Select(e => e.ErrorCode)
            );
        }

        workflow.Response.Data = null;
        workflow.Response.Status = WorkflowScheme.RESPONSE.EnumResponseStatus.ERROR;
        try
        {
            //EngineContext
            //    .Current.Resolve<IGrpcService>()
            //    ?.SendCentralizedLog(workflow.Response.ErrorMessage);
        }
        catch { }

        return workflow;
    }

    /// <summary>
    /// Errors the workflow
    /// </summary>
    /// <param name="workflow">The workflow</param>
    /// <param name="exception">The exception</param>
    /// <returns>The workflow</returns>
    private static WorkflowScheme Error(WorkflowScheme workflow, O24OpenAPIException exception)
    {
        workflow.Response.Data = (new { });
        workflow.Response.ErrorCode = exception.ErrorCode;
        workflow.Response.ErrorMessage = exception.Message;
        workflow.Response.Status = WorkflowScheme.RESPONSE.EnumResponseStatus.ERROR;
        return workflow;
    }

    /// <summary>
    /// Errors the workflow
    /// </summary>
    /// <param name="workflow">The workflow</param>
    /// <param name="exception">The exception</param>
    /// <returns>The workflow</returns>
    private static WorkflowScheme Error(WorkflowScheme workflow, Exception exception)
    {
        workflow.Response.ErrorCode = "ERROR";
        workflow.Response.Data = (new { });
        workflow.Response.ErrorMessage = exception.Message;
        workflow.Response.Status = WorkflowScheme.RESPONSE.EnumResponseStatus.ERROR;
        return workflow;
    }

    /// <summary>
    /// Gets the transaction date using the specified workflow
    /// </summary>
    /// <param name="workflow">The workflow</param>
    /// <returns>The date time</returns>
    public static DateTime GetTransactionDate(WorkflowScheme workflow)
    {
        return workflow.Request.RequestHeader.TxContext.ContainsKey("transaction_date")
            ? (DateTime)workflow.Request.RequestHeader.TxContext["transaction_date"]
            : new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>
    /// Invokes the workflow
    /// </summary>
    /// <typeparam name="TModel">The model</typeparam>
    /// <param name="workflow">The workflow</param>
    /// <param name="acquire">The acquire</param>
    /// <param name="keyName">The key name</param>
    /// <param name="removeSensitive">The remove sensitive</param>
    /// <param name="replacePosting">The replace posting</param>
    /// <param name="crosservice">The crosservice</param>
    /// <exception cref="O24OpenAPIException">system_busy System is busy now, please try again later</exception>
    /// <returns>A task containing the workflow scheme</returns>
    public static async Task<WorkflowScheme> Invoke<TModel>(
        WorkflowScheme workflow,
        Func<Task<object>> acquire,
        string keyName = "",
        bool removeSensitive = true,
        bool replacePosting = false,
        bool crosservice = false
    )
        where TModel : BaseTransactionModel
    {
        TModel model1 = await workflow.ToModel<TModel>(crosservice);
        TModel model2 = model1;

        object returnObject = null;
        try
        {
            object result = await DoTransaction(
                model2.TransactionNumber,
                workflow,
                acquire,
                model2.IsReverse,
                removeSensitive: removeSensitive,
                replacePosting: replacePosting
            );
            returnObject = result;
            result = null;
        }
        catch (SqlException ex)
        {
            if (ex.Number == 1205)
            {
                throw new O24OpenAPIException(
                    "system_busy",
                    "System is busy now, please try again later"
                );
            }
            BusinessLogHelper.Error(ex, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            BusinessLogHelper.Error(ex, ex.Message);
            throw;
        }

        return !string.IsNullOrEmpty(keyName)
            ? Success(workflow, keyName, returnObject)
            : Success(workflow, returnObject);
    }

    /// <summary>
    /// Invokes the 2 using the specified workflow
    /// </summary>
    /// <typeparam name="TRequestModel">The request model</typeparam>
    /// <param name="workflow">The workflow</param>
    /// <param name="storedProcedureName">The stored procedure name</param>
    /// <param name="replacePosting">The replace posting</param>
    /// <exception cref="O24OpenAPIException"></exception>
    /// <exception cref="O24OpenAPIException"></exception>
    /// <returns>The workflow scheme</returns>
    public static async Task<WorkflowScheme> Invoke2<TRequestModel>(
        WorkflowScheme workflow,
        string storedProcedureName,
        bool replacePosting = false
    )
        where TRequestModel : BaseTransactionModel
    {
        TRequestModel requestModel = await workflow.ToModel<TRequestModel>();
        TRequestModel model = requestModel;
        requestModel = default(TRequestModel);
        long StepTimeoutInMillisecond = workflow.Request.RequestHeader.StepTimeout;
        long sendingUTC = workflow.Request.RequestHeader.UtcSendTime;
        long currentTime = CommonHelper.ConvertToUnixTimestamp(DateTime.UtcNow);
        long trafficTime = currentTime - sendingUTC;
        WebApiSettings apiSettings = EngineContext.Current.Resolve<WebApiSettings>();
        long bufferTime = apiSettings.BufferTime == 0L ? 500L : apiSettings.BufferTime;
        int CommandTimeout = !model.IsReverse
            ? (int)((StepTimeoutInMillisecond - trafficTime * 2L - bufferTime) / 1000L)
            : (int)(StepTimeoutInMillisecond / 1000L);
        ServiceDBContext db_context =
            CommandTimeout > 0
                ? new ServiceDBContext(CommandTimeout)
                : throw new O24OpenAPIException(
                    "The message sent to service ["
                        + Singleton<O24OpenAPIClientConfiguration>.Instance.YourServiceID
                        + "] was timeout"
                );
        WorkflowScheme returnWorkflowScheme;
        WorkflowScheme workflowScheme;
        try
        {
            returnWorkflowScheme = db_context.CallServiceStoredProcedure(
                storedProcedureName,
                workflow,
                model.IsReverse
                    ? ServiceDBContext.EnumIsReverse.R
                    : ServiceDBContext.EnumIsReverse.N
            );
            returnWorkflowScheme.Response.Status = WorkflowScheme
                .RESPONSE
                .EnumResponseStatus
                .SUCCESS;

            workflowScheme = returnWorkflowScheme;
        }
        catch (Exception e)
        {
            db_context = null;
            db_context?.Dispose();
            var parser = JToken.Parse(e.Message);
            throw new O24OpenAPIException(
                parser["errorcode"]?.ToString(),
                parser["errormessage"]?.ToString()
            );
        }

        return workflowScheme;
    }

    /// <summary>
    /// Invokes the command query using the specified workflow
    /// </summary>
    /// <param name="workflow">The workflow</param>
    /// <exception cref="ArgumentNullException">Workflow scheme cannot be null.</exception>
    /// <returns>The result</returns>
    public static async Task<WorkflowScheme> InvokeCommandQuery(WorkflowScheme workflow)
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

    /// <summary>
    /// Invokes the command dml using the specified workflow
    /// </summary>
    /// <param name="workflow">The workflow</param>
    /// <exception cref="ArgumentNullException">Workflow scheme cannot be null.</exception>
    /// <returns>The result</returns>
    public static async Task<WorkflowScheme> InvokeCommandDML(WorkflowScheme workflow)
    {
        var model = await workflow.ToModel<ActionRequestModel>();
        if (workflow == null)
        {
            throw new ArgumentNullException(nameof(workflow), "Workflow scheme cannot be null.");
        }

        var result = await Invoke<ActionRequestModel>(
            workflow,
            async () =>
            {
                var executeQueryService = EngineContext.Current.Resolve<IExecuteQueryService>();
                return await executeQueryService.ExecuteDML(model);
            }
        );
        return result;
    }

    /// <summary>
    /// Invokes the workflow scheme
    /// </summary>
    /// <param name="workflowScheme">The workflow scheme</param>
    /// <param name="fullClassName">The full class name</param>
    /// <param name="methodName">The method name</param>
    /// <param name="assemblyName">The assembly name</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentNullException">Cannot find Method with name [{methodName}] in class [{fullClassName}]]</exception>
    /// <exception cref="ArgumentNullException">Cannot find class with Name=[{fullClassName} in assembly [{assemblyName}]]</exception>
    /// <exception cref="ArgumentNullException">Workflow scheme cannot be null.</exception>
    /// <returns>A task containing the workflow scheme</returns>
    public static async Task<WorkflowScheme> InvokeAsync(
        WorkflowScheme workflowScheme,
        string fullClassName,
        string methodName,
        string assemblyName
    )
    {
        if (workflowScheme == null)
        {
            throw new ArgumentNullException(
                nameof(workflowScheme),
                "Workflow scheme cannot be null."
            );
        }

        var type =
            Type.GetType(fullClassName + ", " + assemblyName)
            ?? throw new ArgumentNullException(
                $"Cannot find class with Name=[{fullClassName} in assembly [{assemblyName}]]"
            );
        var method =
            type.GetMethod(methodName)
            ?? throw new ArgumentNullException(
                $"Cannot find Method with name [{methodName}] in class [{fullClassName}]]"
            );
        object classInstance =
            EngineContext.Current.ResolveUnregistered(type)
            ?? throw new ArgumentNullException(
                "Cannot create an instance of [" + fullClassName + "]"
            );
        var task = (Task<WorkflowScheme>)method.Invoke(classInstance, [workflowScheme]);
        return await task;
    }

    /// <summary>
    /// Removes the old data transaction using the specified command timeout in second
    /// </summary>
    /// <param name="commandTimeoutInSecond">The command timeout in second</param>
    /// <param name="workflow">The workflow</param>
    private static async Task RemoveOldDataTransaction(
        int commandTimeoutInSecond,
        WorkflowScheme workflow
    )
    {
        ServiceDBContext db_context = new(commandTimeoutInSecond);
        try
        {
            BaseTransactionModel baseTransactionModel =
                await workflow.ToModel<BaseTransactionModel>();
            if (
                !baseTransactionModel.IsTransactionReverse
                && !baseTransactionModel.IsTransactionCompensated
            )
            {
                workflow.Request.RequestHeader.TxContext["base_transaction_model"] =
                    baseTransactionModel;
                db_context.CallServiceStoredProcedure(
                    O24OpenAPICommonDefaults.RemoveOldDataStoredProcedureName,
                    workflow,
                    baseTransactionModel.IsReverse
                        ? ServiceDBContext.EnumIsReverse.R
                        : ServiceDBContext.EnumIsReverse.N
                );
            }
        }
        finally
        {
            ((IDisposable)db_context)?.Dispose();
        }
    }

    /// <summary>
    /// Does the transaction using the specified transaction number
    /// </summary>
    /// <param name="transactionNumber">The transaction number</param>
    /// <param name="workflow">The workflow</param>
    /// <param name="acquire">The acquire</param>
    /// <param name="isReverse">The is reverse</param>
    /// <param name="addInformation">The add information</param>
    /// <param name="removeSensitive">The remove sensitive</param>
    /// <param name="replacePosting">The replace posting</param>
    /// <param name="IsCompensated">The is compensated</param>
    /// <param name="useTransactionScope">The use transaction scope</param>
    /// <param name="isInquiry">The is inquiry</param>
    /// <exception cref="O24OpenAPIException">MessageOutdate Message is outdate</exception>
    /// <exception cref="TimeoutException">The operation has timed out.</exception>
    /// <exception cref="O24OpenAPIException">Timeout </exception>
    /// <exception cref="O24OpenAPIException">Timeout The operation has timed out.</exception>
    /// <returns>The result</returns>
    private static async Task<dynamic> DoTransaction(
        string transactionNumber,
        WorkflowScheme workflow,
        Func<Task<object>> acquire,
        bool isReverse,
        Func<Task<WorkflowScheme>> addInformation = null,
        bool removeSensitive = true,
        bool replacePosting = false,
        bool IsCompensated = false,
        bool useTransactionScope = true,
        bool isInquiry = true
    )
    {
        long timeoutSec = workflow.Request.RequestHeader.StepTimeout;
        long startSec = workflow.Request.RequestHeader.UtcSendTime;
        long currentTime = CommonHelper.ConvertToUnixTimestamp(DateTime.UtcNow);
        long trafficTime = currentTime - startSec;
        WebApiSettings apiSettings = EngineContext.Current.Resolve<WebApiSettings>();
        long bufferTime = (apiSettings.BufferTime == 0L) ? 500 : apiSettings.BufferTime;

        if (IsCompensated)
        {
            workflow.Request.RequestHeader.is_compensated = "Y";
        }

        if (timeoutSec - 2 * trafficTime - bufferTime <= 0 && !isReverse && !IsCompensated)
        {
            throw new O24OpenAPIException("MessageOutdate", "Message is outdate");
        }

        dynamic result = null;
        var timeout =
            (timeoutSec == 0 || IsCompensated || isReverse)
                ? TimeSpan.MaxValue
                : TimeSpan.FromMilliseconds(timeoutSec);

        using (CancellationTokenSource cts = new())
        {
            try
            {
                cts.Token.ThrowIfCancellationRequested();
                Task<object> acquireTask = acquire();
                if (await Task.WhenAny(acquireTask, Task.Delay(timeout, cts.Token)) == acquireTask)
                {
                    result = await acquireTask;
                }
                else
                {
                    cts.Cancel();
                    throw new TimeoutException("The operation has timed out.");
                }

                if (useTransactionScope)
                {
                    using TransactionScope tx = new(
                        TransactionScopeOption.RequiresNew,
                        new TransactionOptions
                        {
                            IsolationLevel = IsolationLevel.ReadCommitted,
                            Timeout = timeout,
                        },
                        TransactionScopeAsyncFlowOption.Enabled
                    );
                    if (!string.IsNullOrEmpty(transactionNumber) && !isInquiry)
                    {
                        await RemoveOldDataTransaction((int)timeoutSec, workflow);
                    }

                    if (addInformation != null)
                    {
                        workflow = await addInformation();
                    }

                    if (!isReverse)
                    {
                        workflow = await AddBusinessInformation(workflow);
                    }

                    tx.Complete();
                }
            }
            catch (OperationCanceledException)
            {
                throw new O24OpenAPIException("Timeout", "The operation has timed out.");
            }
            catch (TimeoutException ex)
            {
                throw new O24OpenAPIException("Timeout", ex.Message);
            }
            catch
            {
                throw;
            }
        }

        return result;
    }

    /// <summary>
    /// Invokes the workflow
    /// </summary>
    /// <typeparam name="TModel">The model</typeparam>
    /// <param name="workflow">The workflow</param>
    /// <param name="acquire">The acquire</param>
    /// <param name="addInformation">The add information</param>
    /// <param name="keyName">The key name</param>
    /// <param name="removeSensitive">The remove sensitive</param>
    /// <param name="replacePosting">The replace posting</param>
    /// <exception cref="O24OpenAPIException">system_busy System is busy now, please try again later</exception>
    /// <returns>A task containing the workflow scheme</returns>
    public static async Task<WorkflowScheme> Invoke<TModel>(
        WorkflowScheme workflow,
        Func<Task<object>> acquire,
        Func<Task<WorkflowScheme>> addInformation,
        string keyName = "",
        bool removeSensitive = true,
        bool replacePosting = false
    )
        where TModel : BaseTransactionModel
    {
        TModel model1 = await workflow.ToModel<TModel>();
        TModel model2 = model1;
        object returnObject = null;
        bool isCompensated = workflow.Request.RequestHeader.is_compensated == "Y";
        try
        {
            object result = await DoTransaction(
                model2.TransactionNumber,
                workflow,
                acquire,
                model2.IsReverse,
                addInformation,
                removeSensitive,
                replacePosting,
                isCompensated
            );
            returnObject = result;
            result = null;
        }
        catch (SqlException ex)
        {
            if (ex.Number == 1205)
            {
                throw new O24OpenAPIException(
                    "system_busy",
                    "System is busy now, please try again later"
                );
            }

            throw;
        }
        catch (Exception)
        {
            throw;
        }

        return !string.IsNullOrEmpty(keyName)
            ? Success(workflow, keyName, returnObject)
            : Success(workflow, returnObject);
    }

    /// <summary>
    /// Adds the business information using the specified workflow
    /// </summary>
    /// <param name="workflow">The workflow</param>
    /// <returns>The workflow</returns>
    private static async Task<WorkflowScheme> AddBusinessInformation(WorkflowScheme workflow)
    {
        if (!workflow.Request.RequestHeader.TxContext.ContainsKey("transaction_date"))
        {
            IWorkContext workContext = EngineContext.Current.Resolve<IWorkContext>();
            DateTime tranDt = await workContext.GetWorkingDate(true);
            workflow.Request.RequestHeader.TxContext.Add(
                "transaction_date",
                tranDt.Date.ToString("o")
            );
            workContext = null;
        }

        return workflow;
    }

    // private async Task<WorkflowScheme> AddPostings(
    //     WorkflowScheme workflow,
    //     string transactionNumber,
    //     bool replacePostings)
    // {
    //     string postingNode = "postings";
    //     List<GLEntries> glEntries = await EngineContext.Current.Resolve<IGLEntriesService>()
    //         .ListByTransactionNumber(transactionNumber);
    //     if (glEntries == null || glEntries.Count == 0)
    //         return workflow;
    //     foreach (GLEntries gl in glEntries)
    //     {
    //         gl.Posted = true;
    //         await gl.Update(publishEvent: false);
    //     }
    //
    //     if (!workflow.request.RequestHeader.tx_context.ContainsKey(postingNode))
    //     {
    //         workflow.request.RequestHeader.tx_context.Add(postingNode, (object)glEntries);
    //         return workflow;
    //     }
    //
    //     object currentNodePostings = workflow.request.RequestHeader.tx_context[postingNode];
    //     List<GLEntries> currentEntries = new List<GLEntries>();
    //     if (currentNodePostings != null)
    //     {
    //         string jsonText = JsonSerializer.Serialize<object>(currentNodePostings);
    //         currentEntries = JsonSerializer.Deserialize<List<GLEntries>>(jsonText);
    //         jsonText = (string)null;
    //     }
    //
    //     if (replacePostings)
    //         currentEntries.Clear();
    //     currentEntries.AddRange((IEnumerable<GLEntries>)glEntries);
    //     workflow.request.RequestHeader.tx_context[postingNode] = (object)currentEntries;
    //     return workflow;
    // }
}
