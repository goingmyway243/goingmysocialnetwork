using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GoingMy.Upload.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUploadApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

        return services;
    }
}
