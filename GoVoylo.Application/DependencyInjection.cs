using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using GoVoylo.Application.Common.Behaviors;

namespace GoVoylo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(configuration => {
            configuration.RegisterServicesFromAssembly(assembly);
            
            // Wire pipelines in order of execution
            configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            // 2. Validate the request parameters second (New line added!)
            configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
