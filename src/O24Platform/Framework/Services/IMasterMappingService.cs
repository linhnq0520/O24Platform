using O24OpenAPI.Framework.Domain;

namespace O24OpenAPI.Framework.Services;

public interface IMasterMappingService
{
    /// <summary>Gets a master mapping by master class name</summary>
    /// <param name="masterClass"></param>
    /// <returns></returns>
    Task<MasterMapping> GetByMasterClass(string masterClass);
}
