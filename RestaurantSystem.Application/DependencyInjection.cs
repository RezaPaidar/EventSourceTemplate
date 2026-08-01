using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RestaurantSystem.Application.Abstractions.Messaging;
using RestaurantSystem.Application.Abstractions.Persistence.Behaviors;

namespace RestaurantSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        var assembly = typeof(DependencyInjection).Assembly;
        var handlerInterface = typeof(IIntegrationEventHandler<>);

        var handlers = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false })
                .SelectMany(t => t.GetInterfaces(), (t, i) => new { Implementation = t, Interface = i })
                .Where(x => x.Interface.IsGenericType && x.Interface.GetGenericTypeDefinition() == handlerInterface);

        foreach (var handler in handlers)
        {
            services.AddScoped(handler.Interface, handler.Implementation);
        }

        return services;
    }
}