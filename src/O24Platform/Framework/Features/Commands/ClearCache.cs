using System.Text.Json.Serialization;
using LinKit.Core.Cqrs;
using O24OpenAPI.Core.Caching;

namespace O24OpenAPI.Framework.Features.Commands;

public class ClearCacheCommand : ICommand<bool>
{
    [JsonPropertyName("target")]
    public string Target { get; set; } = string.Empty;
}

[CqrsHandler]
public class ClearCacheHandler(IStaticCacheManager staticCacheManager)
    : ICommandHandler<ClearCacheCommand, bool>
{
    public async Task<bool> HandleAsync(
        ClearCacheCommand command,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrEmpty(command.Target))
        {
            await staticCacheManager.Clear();
        }
        else
        {
            await staticCacheManager.ClearByWord(command.Target);
        }
        return true;
    }
}
