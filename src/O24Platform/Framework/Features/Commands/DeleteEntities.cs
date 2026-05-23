using System.Transactions;
using LinKit.Core.Cqrs;
using O24OpenAPI.Core.Caching;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Core.SeedWork;

namespace O24OpenAPI.Framework.Features.Commands;

public class DeleteEntitiesCommand : ICommand<bool>
{
    public List<EntityDeleteRequest> EntityDeleteRequestList { get; set; } = new();
}

public class EntityDeleteRequest
{
    public string EntityName { get; set; } = default!;
    public List<int> Ids { get; set; } = new();
}

[CqrsHandler]
public class DeleteEntitiesHandler(ITypeFinder typeFinder, IServiceProvider serviceProvider)
    : ICommandHandler<DeleteEntitiesCommand, bool>
{
    public async Task<bool> HandleAsync(
        DeleteEntitiesCommand request,
        CancellationToken cancellationToken = default
    )
    {
        if (request.EntityDeleteRequestList == null || request.EntityDeleteRequestList.Count == 0)
        {
            return true;
        }

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        foreach (var item in request.EntityDeleteRequestList)
        {
            if (item.Ids == null || !item.Ids.Any())
                continue;

            // 1️⃣ Tìm Entity Type
            var entityType =
                typeFinder.FindEntityTypeByName(item.EntityName)
                ?? throw new InvalidOperationException(
                    $"Entity type '{item.EntityName}' not found"
                );

            // 2️⃣ Tạo IRepository<TEntity>
            var repositoryType = typeof(IRepository<>).MakeGenericType(entityType);

            var repository =
                serviceProvider.GetService(repositoryType)
                ?? throw new InvalidOperationException(
                    $"Repository for '{item.EntityName}' not found"
                );

            // 3️⃣ Tìm method GetByIds
            var getByIdsMethod =
                repositoryType.GetMethod(
                    "GetByIds",
                    [
                        typeof(IList<int>),
                        typeof(Func<IStaticCacheManager, CacheKey>),
                        typeof(CancellationToken),
                    ]
                )
                ?? throw new InvalidOperationException(
                    $"GetByIds method not found in repository for '{item.EntityName}'"
                );

            // 4️⃣ Gọi GetByIds
            var getTask = getByIdsMethod.Invoke(repository, [item.Ids, null, cancellationToken]);

            if (getTask is not Task getAwaitable)
                continue;

            await getAwaitable;

            var resultProperty = getAwaitable.GetType().GetProperty("Result");

            var entities = resultProperty?.GetValue(getAwaitable);

            if (entities == null)
                continue;

            // 5️⃣ Tìm method BulkDelete
            var bulkDeleteMethod =
                repositoryType.GetMethod(
                    "BulkDelete",
                    [typeof(IList<>).MakeGenericType(entityType), typeof(CancellationToken)]
                )
                ?? throw new InvalidOperationException(
                    $"BulkDelete method not found in repository for '{item.EntityName}'"
                );

            // 6️⃣ Gọi BulkDelete
            var deleteTask = bulkDeleteMethod.Invoke(repository, [entities, cancellationToken]);

            if (deleteTask is Task deleteAwaitable)
            {
                await deleteAwaitable;
            }
        }

        scope.Complete();
        return true;
    }
}
