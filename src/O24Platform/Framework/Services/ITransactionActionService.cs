using O24OpenAPI.Core.Domain;
using O24OpenAPI.Framework.Models;

namespace O24OpenAPI.Framework.Services;

public interface ITransactionActionService
{
    Task<string> CreateGLEntry<T>(
        string transId,
        BaseTransactionModel transaction,
        T master,
        decimal amount,
        string sysAccountName,
        string dorc,
        string crossBranchCode,
        string crossCurrencyCode,
        decimal baseCurrencyAmount,
        int accountingGroup = 1
    )
        where T : BaseEntity;

    Task<string> Execute<T>(
        BaseTransactionModel transaction,
        T master,
        string code,
        decimal amount,
        string crossBranchCode = "",
        string crossCurrencyCode = "",
        decimal baseCurrencyAmount = 0m,
        string statementCode = "",
        string refNumber = "",
        int accountingGroup = 1,
        bool IsUpdateMaster = true
    )
        where T : BaseEntity;

    Task PostGL(
        string transId,
        string transTableName,
        string transactionNumber,
        DateTime valueDate,
        string sysAccountName,
        string glAccount,
        decimal amount,
        string dorc,
        string branchCode,
        string currencyCode,
        string crossBranch,
        string crossCurrency,
        decimal baseAmount,
        bool isReverse = false,
        int accountingGroup = 1
    );
}
