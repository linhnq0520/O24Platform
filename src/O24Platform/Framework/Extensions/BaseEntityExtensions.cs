using System.ComponentModel;
using System.Reflection;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.Helpers;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Services;

namespace O24OpenAPI.Framework.Extensions;

public static class BaseEntityExtensions
{
    public static string GetMasterStringField(this BaseEntity entity, string propertyName)
    {
        if (entity == null || string.IsNullOrWhiteSpace(propertyName))
        {
            return string.Empty;
        }

        var property = entity
            .GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property == null)
        {
            return string.Empty;
        }

        var value = property.GetValue(entity);
        if (value == null)
        {
            return string.Empty;
        }

        var converter = TypeDescriptor.GetConverter(property.PropertyType);
        return converter != null
            ? converter.ConvertToInvariantString(value) ?? string.Empty
            : value.ToString();
    }

    public static async Task<string> GetMasterRelatedStringFieldAsync(
        this BaseEntity entity,
        string propertyName
    )
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return string.Empty;
        }

        // Trường hợp không có dấu ':', xử lý như bình thường
        var parts = propertyName.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            return entity.GetMasterStringField(propertyName);
        }

        // Phần sau dấu ':' có thể chứa "TableName/KeyField/TargetField"
        var mappingParts = parts[1].Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (mappingParts.Length < 3)
        {
            return entity.GetMasterStringField(propertyName);
        }

        var tableName = mappingParts[0];
        var keyField = mappingParts[1];
        var targetField = mappingParts[2];

        var keyValue = entity.GetMasterStringField(keyField);
        if (string.IsNullOrWhiteSpace(keyValue))
        {
            return string.Empty;
        }

        var searchInput = new Dictionary<string, string> { { keyField, keyValue } };

        var result = await DynamicRepositoryHelper.DynamicGetByFields<BaseEntity>(
            tableName,
            searchInput
        );

        return result?.GetMasterStringField(targetField) ?? string.Empty;
    }

    public static async Task<string> ExecuteTran<T>(
        this T master,
        BaseTransactionModel transaction,
        string code,
        decimal amount,
        string crossBranchCode = "",
        string crossCurrencyCode = "",
        decimal baseCurrencyAmount = 0m,
        string statementCode = "",
        string refNumber = "",
        int accountingGroup = 1
    )
        where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(master);
        Console.WriteLine(
            $"======================Master:${master}================================"
        );
        ITransactionActionService actionService =
            EngineContext.Current.Resolve<ITransactionActionService>();
        return await actionService.Execute(
            transaction,
            master,
            code,
            amount,
            crossBranchCode,
            crossCurrencyCode,
            baseCurrencyAmount,
            statementCode,
            refNumber,
            accountingGroup
        );
    }
}
