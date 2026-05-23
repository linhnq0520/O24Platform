using System.Globalization;
using FluentValidation.Results;
using Humanizer;
using Newtonsoft.Json;
using O24OpenAPI.Client.Scheme.Workflow;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Services;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The 24 open api extensions class
/// </summary>
public static class O24OpenApiExtensions
{
    /// <summary>
    /// Returns the model using the specified value
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="value">The value</param>
    /// <param name="crossservice">The crossservice</param>
    /// <returns>The model</returns>
    public static async Task<T> ToModel<T>(this WorkflowScheme value, bool crossservice = false)
        where T : BaseTransactionModel
    {
        string text = "";
        T model;
        WorkflowInput wfInput;
        if (crossservice)
        {
            text = JsonConvert.SerializeObject(value);
            model = text.ToModel<T>(true);
            wfInput = text.ToWorkflowInput<T>(true);
        }
        else
        {
            text = JsonSerializer.Serialize(value);
            model = text.ToModel<T>();
            wfInput = text.ToWorkflowInput<T>();
        }

        IJwtTokenService tokenService = EngineContext.Current.Resolve<IJwtTokenService>();
        model.AppCode = value.Request.RequestHeader.Channel;
        model.TransactionCode = wfInput.workflowid;
        model.SubCode = value.Request.RequestHeader.StepCode;
        model.ServiceSysDate = DateTime.UtcNow;
        model.TransactionDate = value.GetTransactionDate();
        model.TransactionNumber = value.GetTransactionNumber();
        model.TransactionType = value.Request.RequestHeader.TxContext.GetStringValue(
            "transaction_type"
        );
        model.ChannelId = value.Request.RequestHeader.Channel;
        model.ValueDate = value.GetValueDate();
        model.Postings = value.GetContextPostings();
        model.Language = wfInput.Lang;
        model.RefId = value.Request.RequestHeader.ExecutionId;
        model.ReferenceId = wfInput.reference_id;
        model.ReferenceCode = wfInput.reference_code;
        model.BusinessCode = wfInput.business_code;
        model.ClientDeviceId = value.Request.RequestHeader.ClientDeviceId;
        if (string.IsNullOrEmpty(model.Description))
        {
            model.Description = wfInput.description;
        }

        model.IsReverse = value.Request.RequestHeader.is_compensated.Equals("Y");
        model.Token = wfInput.Token;
        if (crossservice)
        {
            model.RequestBody = JsonConvert.SerializeObject(value.Request.RequestBody);
        }
        else
        {
            model.RequestBody = JsonSerializer.Serialize(value.Request.RequestBody);
        }

        DateTime dateTime;
        if (!string.IsNullOrEmpty(value.Request.RequestHeader.ApprovalExecutionId))
        {
            model.RefId = value.Request.RequestHeader.ApprovalExecutionId;
            Transaction originTransaction = await EngineContext
                .Current.Resolve<ITransactionService>()
                .GetByRefId(model.RefId);
            if (originTransaction != null)
            {
                // ISSUE: variable of a boxed type
                dateTime = originTransaction.ValueDate;
                DateTime date1 = dateTime.Date;
                model.ValueDate = date1;
                model.TransactionDate = originTransaction.TransactionDate;
                DateTime transactionDate = originTransaction.TransactionDate;
                dateTime = originTransaction.TransactionDate;
                DateTime date2 = dateTime.Date;
                if (transactionDate == date2)
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

                model.CurrentBranchCode = value.Request.RequestHeader.TxContext.GetStringValue(
                    "branch_code",
                    originTransaction.BranchCode
                );
                model.CurrentLoginname = value.Request.RequestHeader.TxContext.GetStringValue(
                    "login_name",
                    originTransaction.LoginName
                );
                model.CurrentUsername = value.Request.RequestHeader.TxContext.GetStringValue(
                    "user_name",
                    originTransaction.UserName
                );
                model.CurrentUserCode = value.Request.RequestHeader.TxContext.GetStringValue(
                    "user_code",
                    originTransaction.UserCode
                );
                T obj = model;
                string str = await EngineContext
                    .Current.Resolve<IWorkContext>()
                    .GetCurrentUserApprove();
                obj.UserApprove = str;
                obj = default(T);
                str = null;
            }
            else
            {
                model.CurrentBranchCode = value.Request.RequestHeader.TxContext.GetStringValue(
                    "current_branch_code"
                );
                model.CurrentLoginname = value.Request.RequestHeader.TxContext.GetStringValue(
                    "current_loginname"
                );
                model.CurrentUsername = value.Request.RequestHeader.TxContext.GetStringValue(
                    "current_username"
                );
                model.CurrentUserCode = value.Request.RequestHeader.TxContext.GetStringValue(
                    "current_user_code"
                );
            }

            if (!string.IsNullOrEmpty(model.CurrentUserCode))
            {
                model.IgnoreSession = true;
            }

            originTransaction = null;
        }
        else
        {
            await EngineContext.Current.Resolve<IWorkContext>().SetCurrentUserApprove("");
        }

        if (!string.IsNullOrEmpty(value.Request.RequestHeader.ReversalExecutionId))
        {
            if (model.IsReverse)
            {
                model.IsForceReverse = true;
            }

            model.RefId = value.Request.RequestHeader.ReversalExecutionId;
            model.IsReverse = true;
            Transaction originTransaction = await EngineContext
                .Current.Resolve<ITransactionService>()
                .GetByRefId(model.RefId);
            if (originTransaction != null)
            {
                // ISSUE: variable of a boxed type
                dateTime = originTransaction.ValueDate;
                DateTime date3 = dateTime.Date;
                model.ValueDate = date3;
                model.TransactionDate = originTransaction.TransactionDate;
                DateTime transactionDate = originTransaction.TransactionDate;
                dateTime = originTransaction.TransactionDate;
                DateTime date4 = dateTime.Date;
                if (transactionDate == date4)
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
                model.IgnoreSession = true;
            }

            originTransaction = null;
        }
        if (!model.IgnoreSession)
        {
            string token = model.Token;

            if (string.IsNullOrEmpty(model.Token))
            {
                token = value.Request.RequestHeader.TxContext.GetStringValue("token");
            }

            model.CurrentLoginname = tokenService.GetLoginNameFromToken(token);
            model.CurrentUsername = tokenService.GetUsernameFromToken(token);
            model.CurrentUserCode = tokenService.GetUserCodeFromToken(token);
            model.CurrentBranchCode = tokenService.GetBranchCodeFromToken(token);
        }

        model.Status = "N";
        if (wfInput.fields != null && wfInput.fields.ContainsKey("description"))
        {
            model.Description = wfInput.fields.GetValueOrDefault("description").ToString();
        }

        if (wfInput.fields != null && wfInput.fields.ContainsKey("user_approve"))
        {
            object approveValue = wfInput.fields.GetValueOrDefault("user_approve");
            if (approveValue == null)
            {
                model.UserApprove = null;
            }
            else
            {
                model.UserApprove = approveValue.ToString();
            }

            approveValue = null;
        }

        if (string.IsNullOrEmpty(model.UserApprove))
        {
            model.UserApprove = value.GetUserApprove();
        }

        model.Amount1 = 0M;
        T model1 = model;
        text = null;
        model = default(T);
        wfInput = null;
        tokenService = null;
        return model1;
    }

    /// <summary>
    /// Gets the transaction date using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The date time</returns>
    public static DateTime GetTransactionDate(this WorkflowScheme value)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(
            value.Request.RequestHeader.UtcSendTime
        );
    }

    /// <summary>
    /// Gets the value date using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The date time</returns>
    public static DateTime GetValueDate(this WorkflowScheme value)
    {
        string key = "transaction_date";
        DateTime transactionDate = value.GetTransactionDate();
        try
        {
            if (value.Request.RequestHeader.TxContext.ContainsKey(key))
            {
                return DateTime.Parse(
                    value.Request.RequestHeader.TxContext[key].ToString(),
                    null,
                    DateTimeStyles.RoundtripKind
                );
            }
        }
        catch { }

        return transactionDate.Date;
    }

    /// <summary>
    /// Gets the context postings using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>A list of gl entries</returns>
    public static List<GLEntries> GetContextPostings(this WorkflowScheme value)
    {
        if (value.Request.RequestHeader.TxContext.ContainsKey("postings"))
        {
            object obj = value.Request.RequestHeader.TxContext["postings"];
            if (obj != null)
            {
                return JsonSerializer.Deserialize<List<GLEntries>>(JsonSerializer.Serialize(obj));
            }
        }

        return [];
    }

    /// <summary>
    /// Gets the transaction number using the specified workflow
    /// </summary>
    /// <param name="workflow">The workflow</param>
    /// <returns>The string</returns>
    public static string GetTransactionNumber(this WorkflowScheme workflow)
    {
        return workflow.Request.RequestHeader.TxContext.GetStringValue("transaction_number");
    }

    /// <summary>
    /// Gets the user approve using the specified workflow
    /// </summary>
    /// <param name="workflow">The workflow</param>
    /// <returns>The string</returns>
    public static string GetUserApprove(this WorkflowScheme workflow)
    {
        return workflow.Request.RequestHeader.TxContext.GetStringValue("user_approve");
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
    internal static Dictionary<string, object> ToDictionary(this object model, string name)
    {
        return new Dictionary<string, object>() { { name.Underscore(), model } };
    }
}
