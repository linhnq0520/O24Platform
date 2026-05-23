using O24OpenAPI.Core.SeedWork;
using O24OpenAPI.Data.System.Linq;
using O24OpenAPI.Framework.Domain;

namespace O24OpenAPI.Framework.Services;

public class MasterMappingService(IRepository<MasterMapping> mappingRepository)
    : IMasterMappingService
{
    private readonly IRepository<MasterMapping> _mappingRepository = mappingRepository;

    public async Task<MasterMapping> GetByMasterClass(string masterClass)
    {
        if (string.IsNullOrWhiteSpace(masterClass))
        {
            throw new ArgumentException("masterClass is required.", nameof(masterClass));
        }

        var normalized = masterClass.Trim().ToLower();

        return await _mappingRepository.Table.FirstOrDefaultAsync(a =>
            a.MasterClass.ToLower().Trim() == normalized
        );
    }
}
