#nullable enable

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GoingMy.Post.Application.Extensions;

/// <summary>
/// Extension methods for configuring Post.Application services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MediatR services to the dependency injection container.
    /// Registers all handlers from the Application assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPostApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly())
        );

        return services;
    }
}
