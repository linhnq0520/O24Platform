using Microsoft.Extensions.DependencyInjection;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Logging.Helpers;

namespace O24OpenAPI.Framework.Utils;

public static class TaskUtils
{
    public static Task RunAsync(Func<Task> function)
    {
        return Task.Run(async () =>
        {
            using IServiceScope scope = EngineContext.Current.CreateScope();
            try
            {
                await function();
            }
            catch (Exception ex)
            {
                BusinessLogHelper.Error(ex, ex.Message);
            }
            finally
            {
                AsyncScope.Clear();
            }
        });
    }

    public static Task RunInNewScope(Func<IServiceScope, Task> function)
    {
        return Task.Run(async () =>
        {
            using IServiceScope scope = EngineContext.Current.CreateScope();
            try
            {
                await function(scope);
            }
            catch (Exception ex)
            {
                BusinessLogHelper.Error(ex, ex.Message);
            }
            finally
            {
                AsyncScope.Clear();
            }
        });
    }
}
