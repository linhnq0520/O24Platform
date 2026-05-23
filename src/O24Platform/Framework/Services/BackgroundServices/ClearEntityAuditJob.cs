using LinKit.Core.BackgroundJobs;
using LinKit.Core.Cqrs;
using O24OpenAPI.Framework.Repositories;

namespace O24OpenAPI.Framework.Services.BackgroundServices;

[BackgroundJob("ClearEntityAuditJob")]
public class ClearEntityAuditJob : BackgroundJobCommand { }

[CqrsHandler]
internal class ClearEntityAuditJobHandler(IEntityAuditRepository entityAuditRepository)
    : ICommandHandler<ClearEntityAuditJob>
{
    public async Task<Unit> HandleAsync(
        ClearEntityAuditJob command,
        CancellationToken cancellationToken = default
    )
    {
        await entityAuditRepository.ClearOldAudits();
        return Unit.Value;
    }
}
