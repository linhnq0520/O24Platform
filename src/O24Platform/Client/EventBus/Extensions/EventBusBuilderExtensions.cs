using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using O24OpenAPI.Client.EventBus.Abstractions;
using O24OpenAPI.Contracts.Events;

namespace O24OpenAPI.Client.EventBus.Extensions;

public static class EventBusBuilderExtensions
{
    public static IEventBusBuilder AddSubscription<
        T,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TH
    >(this IEventBusBuilder eventBusBuilder)
        where T : IntegrationEvent
        where TH : class, IIntegrationEventHandler<T>
    {
        // Use keyed services to register multiple handlers for the same event type
        // the consumer can use IKeyedServiceProvider.GetKeyedService<IIntegrationEventHandler>(typeof(T)) to get all
        // handlers for the event type.
        eventBusBuilder.Services.AddKeyedTransient<IIntegrationEventHandler, TH>(typeof(T));

        eventBusBuilder.Services.Configure<EventBusSubscriptionInfo>(o =>
        {
            // Keep track of all registered event types and their name mapping. We send these event types over the message bus
            // and we don't want to do Type.GetType, so we keep track of the name mapping here.

            // This list will also be used to subscribe to events from the underlying message broker implementation.
            o.EventTypes[typeof(T).Name] = typeof(T);
        });

        return eventBusBuilder;
    }

    public static IEventBusBuilder AddSubscriptionsFromAssemblies(
        this IEventBusBuilder eventBusBuilder,
        params Assembly[] assembliesToScan
    )
    {
        if (assembliesToScan is null || assembliesToScan.Length == 0)
        {
            return eventBusBuilder;
        }

        // Lấy tất cả các type là class, không phải abstract từ các assembly
        var handlerTypes = assembliesToScan
            .SelectMany(a => a.GetTypes())
            .Where(s => s != null)
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var eventHandlerInterfaces = handlerType.GetInterfaces();
            var handlerInterfaces = eventHandlerInterfaces.Where(i =>
                i.IsGenericType
                && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)
            );

            foreach (var interfaceType in eventHandlerInterfaces)
            {
                if (interfaceType.GetGenericArguments().Length > 0)
                {
                    var eventType = interfaceType.GetGenericArguments()[0];

                    if (typeof(IntegrationEvent).IsAssignableFrom(eventType))
                    {
                        AddSubscription(eventBusBuilder, eventType, handlerType);
                    }
                }
            }
        }

        return eventBusBuilder;
    }

    /// <summary>
    /// Hàm helper để đăng ký một cặp Event-Handler, sử dụng các đối tượng Type.
    /// </summary>
    private static void AddSubscription(
        IEventBusBuilder eventBusBuilder,
        Type eventType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
            Type handlerType
    )
    {
        // Sử dụng overload không generic của AddKeyedTransient
        // serviceType: IIntegrationEventHandler
        // serviceKey: eventType (ví dụ: typeof(OrderCreatedIntegrationEvent))
        // implementationType: handlerType (ví dụ: typeof(OrderCreatedEmailHandler))
        eventBusBuilder.Services.AddKeyedTransient(
            typeof(IIntegrationEventHandler),
            eventType,
            handlerType
        );

        eventBusBuilder.Services.Configure<EventBusSubscriptionInfo>(o =>
        {
            // Tương tự hàm gốc, lưu lại mapping giữa tên event và Type của nó.
            if (!o.EventTypes.ContainsKey(eventType.Name))
            {
                o.EventTypes[eventType.Name] = eventType;
            }
        });
    }
}
