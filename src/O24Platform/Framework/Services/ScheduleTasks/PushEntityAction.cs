//using Microsoft.Extensions.DependencyInjection;
//using Newtonsoft.Json;
//using O24OpenAPI.Client;
//using O24OpenAPI.Client.Events;
//using O24OpenAPI.Client.Events.EventData;
//using O24OpenAPI.Client.Lib;
//using O24OpenAPI.Core.Enums;
//using O24OpenAPI.Core.Infrastructure;

//namespace O24OpenAPI.Framework.Services.ScheduleTasks;

///// <summary>
///// The push entity action class
///// </summary>
///// <seealso cref="IScheduleTask"/>
//public class PushEntityAction : IScheduleTask
//{
//    /// <summary>
//    /// Executes the last success
//    /// </summary>
//    /// <param name="lastSuccess">The last success</param>
//    /// <param name="serviceScope">The service scope</param>
//    public async Task Execute(DateTime? lastSuccess, IServiceScope serviceScope)
//    {
//        var entityAuditService = serviceScope.ServiceProvider.GetService<IEntityAuditService>();
//        var list = await entityAuditService.GetUnsentItems();
//        foreach (var item in list)
//        {
//            try
//            {
//                var eventData = new EntityEvent
//                {
//                    table_name = item.EntityName,
//                    primary_key = null,
//                    action_type = item.ActionType,
//                    data = JsonConvert.DeserializeObject<object>(item.Data),
//                };
//                var o24Event = new O24OpenAPIEvent<EntityEvent>(
//                    O24OpenAPIWorkflowEventTypeEnum.EntityAction
//                );
//                o24Event.EventData.data = eventData;

//                var queueClient = Singleton<QueueClient>.Instance;
//                await queueClient.SendMessage(
//                    QueueUtils.GetEventQueueName("DWH"),
//                    o24Event,
//                    "event"
//                );

//                item.Status = (int)SendEnum.Sent;
//            }
//            catch
//            {
//                item.Status = (int)SendEnum.Failed;
//            }
//            finally
//            {
//                await entityAuditService.UpdateAsync(item);
//            }
//        }
//    }
//}
