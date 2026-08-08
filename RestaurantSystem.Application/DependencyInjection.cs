using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RestaurantSystem.Application.Abstractions.Events;
using RestaurantSystem.Application.Abstractions.Messaging;
using RestaurantSystem.Application.Abstractions.Persistence.Behaviors;

namespace RestaurantSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        var handlerInterface = typeof(IIntegrationEventHandler<>);

        var integrationHandlerInterface = typeof(IIntegrationEventHandler<>);
        var domainHandlerInterface = typeof(IDomainEventHandler<>);

        var handlers = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false })
                .SelectMany(t => t.GetInterfaces(), (t, i) => new { Implementation = t, Interface = i })
                .Where(x => x.Interface.IsGenericType &&
                 (x.Interface.GetGenericTypeDefinition() == handlerInterface ||
                 x.Interface.GetGenericTypeDefinition() == domainHandlerInterface));

        foreach (var handler in handlers)
        {
            services.AddScoped(handler.Interface, handler.Implementation);
        }

        return services;
    }
}