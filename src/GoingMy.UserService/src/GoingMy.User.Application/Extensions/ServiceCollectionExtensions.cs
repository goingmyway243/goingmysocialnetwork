#nullable enable

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GoingMy.User.Application.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers MediatR and all Application layer handlers.</summary>
    public static IServiceCollection AddUserApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

        return services;
    }
}
