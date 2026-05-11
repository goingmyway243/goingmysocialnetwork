using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GoingMy.Notification.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly())
        );

        return services;
    }
}
