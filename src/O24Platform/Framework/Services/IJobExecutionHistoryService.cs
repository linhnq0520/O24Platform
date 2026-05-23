using O24OpenAPI.Core;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Models;

namespace O24OpenAPI.Framework.Services;

public interface IJobExecutionHistoryService
{
    Task<JobExecutionLog> GetById(int id);
    Task<IList<JobExecutionLog>> GetAll();
    Task<JobExecutionLog> AddAsync(JobExecutionLog JobExecutionHistory);
    Task UpdateAsync(JobExecutionLog JobExecutionHistory);
    Task DeleteAsync(JobExecutionLog JobExecutionHistory);
    Task<IPagedList<JobExecutionLog>> SimpleSearch(SimpleSearchModel model);
    Task<JobExecutionLog> GetByJobName(string jobName);
}
