using System.Globalization;
using System.Text.Json;
using FluentValidation.Results;
using Humanizer;
using O24OpenAPI.Client.Scheme.Workflow;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Helper;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Models.O24OpenAPI;
using O24OpenAPI.Framework.Services;

namespace O24OpenAPI.Framework.Extensions;

public static class WFSchemeExtension
{
    public static async Task<T> ToModel<T>(
        this WFScheme value,
        JsonSerializerOptions jsonSerializerOptions = null
    )
        where T : BaseTransactionModel
    {
        try
        {
            string text = JsonSerializer.Serialize(value, SerializerOptions.SerializeOptions);
            var workflow = O24Utils.DeserializeWorkflow<T>(
                text,
                jsonSerializerOptions ?? SerializerOptions.JsonSerializerOptions
            );
            T model = workflow.request.request_body.data;
            WFInput wfInput = workflow.request.request_body.workflow_input;
            bool isReverse = value.request.request_header.is_compensated == "Y";
            model.IsTransactionCompensated = isReverse;
            model.TransactionCode = wfInput.workflowid;
            model.SubCode = value.request.request_header.step_code;
            model.ServiceSysDate = DateTime.UtcNow;
            model.TransactionDate = value.GetTransactionDate();
            model.TransactionNumber = value.GetTransactionNumber();
            model.TransactionType = value.request.request_header.tx_context.GetStringValue(
                "transaction_type"
            );
            model.ChannelId = value.request.request_header.channel_id;
            model.ValueDate = value.GetValueDate();
            model.Language = wfInput.lang;
            model.RefId = value.request.request_header.execution_id;
            model.ReferenceId = wfInput.reference_id;
            model.ReferenceCode = wfInput.reference_code;
            model.BusinessCode = wfInput.business_code;
            model.Description ??= wfInput.description;
            model.IsReverse = isReverse;
            model.Token = wfInput.token;
            model.RequestBody = JsonSerializer.Serialize(value.request.request_body);
            model.Status = "N";
            model.Amount1 = 0m;

            await ProcessApprovalExecutionId(model, value);
            //await ProcessReversalExecutionId(model, value);

            if (!model.IgnoreSession)
            {
                var tokenService = EngineContext.Current.Resolve<IJwtTokenService>();
                model.CurrentLoginname = tokenService.GetLoginNameFromToken(model.Token);
                model.CurrentUsername = tokenService.GetUsernameFromToken(model.Token);
                model.CurrentUserCode = tokenService.GetUserCodeFromToken(model.Token);
                model.CurrentBranchCode = tokenService.GetBranchCodeFromToken(model.Token);
            }

            model.Description =
                wfInput.fields.GetValueOrDefault("description")?.ToString() ?? model.Description;
            model.UserApprove =
                wfInput.fields.GetValueOrDefault("user_approve")?.ToString()
                ?? value.GetUserApprove();

            return model;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"WFScheme parsing failed at '{ex.Path ?? "unknown"}'. {ex.Message}",
                ex
            );
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static async Task ProcessApprovalExecutionId<T>(T model, WFScheme value)
        where T : BaseTransactionModel
    {
        string approvalId = value.request.request_header.approval_execution_id;
        if (string.IsNullOrEmpty(approvalId))
        {
            await EngineContext.Current.Resolve<IWorkContext>().SetCurrentUserApprove("");
            return;
        }

        model.RefId = approvalId;
        var transactionService = EngineContext.Current.Resolve<ITransactionService>();
        var originTransaction = await transactionService.GetByRefId(approvalId);

        if (originTransaction != null)
        {
            UpdateModelFromTransaction(model, originTransaction);
            model.UserApprove = await EngineContext
                .Current.Resolve<IWorkContext>()
                .GetCurrentUserApprove();
        }
        else
        {
            SetUserContextFromWorkflow(model, value);
        }

        if (!string.IsNullOrEmpty(model.CurrentUserCode))
        {
            model.IgnoreSession = true;
        }
    }

    /// <summary>
    /// Processes the reversal execution id using the specified model
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="model">The model</param>
    /// <param name="value">The value</param>
    private static async Task ProcessReversalExecutionId<T>(T model, WFScheme value)
        where T : BaseTransactionModel
    {
        string reversalId = value.request.request_header.reversal_execution_id;
        if (string.IsNullOrEmpty(reversalId))
        {
            return;
        }

        model.IsTransactionReverse = true;
        if (model.IsReverse)
        {
            model.IsForceReverse = true;
        }

        model.RefId = reversalId;
        model.IsReverse = true;

        var transactionService = EngineContext.Current.Resolve<ITransactionService>();
        var originTransaction = await transactionService.GetByRefId(reversalId);

        if (originTransaction != null)
        {
            UpdateModelFromTransaction(model, originTransaction);
            model.IgnoreSession = true;
        }
    }

    /// <summary>
    /// Updates the model from transaction using the specified model
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="model">The model</param>
    /// <param name="originTransaction">The origin transaction</param>
    private static void UpdateModelFromTransaction<T>(T model, Transaction originTransaction)
        where T : BaseTransactionModel
    {
        model.ValueDate = originTransaction.ValueDate.Date;
        model.TransactionDate = originTransaction.TransactionDate;

        if (originTransaction.TransactionDate == originTransaction.TransactionDate.Date)
        {
            DateTime now = DateTime.UtcNow;
            DateTime dt = originTransaction.TransactionDate;
            model.TransactionDate = new DateTime(
                dt.Year,
                dt.Month,
                dt.Day,
                now.Hour,
                now.Minute,
                now.Second,
                now.Millisecond
            );
        }

        model.CurrentBranchCode = originTransaction.BranchCode;
        model.CurrentLoginname = originTransaction.LoginName;
        model.CurrentUsername = originTransaction.UserName;
        model.CurrentUserCode = originTransaction.UserCode;
    }

    /// <summary>
    /// Sets the user context from workflow using the specified model
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="model">The model</param>
    /// <param name="value">The value</param>
    private static void SetUserContextFromWorkflow<T>(T model, WFScheme value)
        where T : BaseTransactionModel
    {
        model.CurrentBranchCode = value.request.request_header.tx_context.GetStringValue(
            "branch_code"
        );
        model.CurrentLoginname = value.request.request_header.tx_context.GetStringValue(
            "login_name"
        );
        model.CurrentUsername = value.request.request_header.tx_context.GetStringValue("user_name");
        model.CurrentUserCode = value.request.request_header.tx_context.GetStringValue("user_code");
    }

    /// <summary>
    /// Gets the transaction date using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The date time</returns>
    public static DateTime GetTransactionDate(this WFScheme value)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(
            value.request.request_header.utc_send_time
        );
    }

    /// <summary>
    /// Gets the value date using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The date time</returns>
    public static DateTime GetValueDate(this WFScheme value)
    {
        string key = "transaction_date";
        DateTime transactionDate = value.GetTransactionDate();
        try
        {
            if (value.request.request_header.tx_context.TryGetValue(key, out object valueDate))
            {
                return DateTime.Parse(valueDate.ToString(), null, DateTimeStyles.RoundtripKind);
            }
        }
        catch { }

        return transactionDate.Date;
    }

    /// <summary>
    /// Gets the transaction number using the specified workflow
    /// </summary>
    /// <param name="workflow">The workflow</param>
    /// <returns>The string</returns>
    public static string GetTransactionNumber(this WFScheme workflow)
    {
        return workflow.request.request_header.tx_context.GetStringValue("transaction_number");
    }

    /// <summary>
    /// Gets the user approve using the specified workflow
    /// </summary>
    /// <param name="workflow">The workflow</param>
    /// <returns>The string</returns>
    public static string GetUserApprove(this WFScheme workflow)
    {
        return workflow.request.request_header.tx_context.GetStringValue("user_approve");
    }

    /// <summary>
    /// Returns the dictionary using the specified errors
    /// </summary>
    /// <param name="errors">The errors</param>
    /// <returns>The dictionary</returns>
    public static Dictionary<string, object> ToDictionary(this List<ValidationFailure> errors)
    {
        Dictionary<string, object> dictionary = [];
        foreach (ValidationFailure error in errors)
        {
            string str = error.PropertyName.Underscore();
            if (!dictionary.ContainsKey(str))
            {
                dictionary.Add(str, error.ErrorMessage);
            }
        }

        return dictionary;
    }

    /// <summary>
    /// Returns the dictionary using the specified model
    /// </summary>
    /// <param name="model">The model</param>
    /// <param name="name">The name</param>
    /// <returns>A dictionary of string and object</returns>
    public static Dictionary<string, object> ToDictionary(this WFScheme model, string name)
    {
        return new Dictionary<string, object>() { { name.Underscore(), model } };
    }
}
