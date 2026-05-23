using System.Data;
using Dapper;

namespace O24OpenAPI.Data.Extensions;

public static class CommandDefinitionExtension
{
    public static CommandDefinition CreateCommand(
        this string sql,
        object? parameters,
        IDbTransaction? transaction = null,
        CancellationToken cancellationToken = default
    )
    {
        var command = new CommandDefinition(
            commandText: sql,
            parameters: parameters,
            cancellationToken: cancellationToken,
            transaction: transaction
        );
        return command;
    }
}
