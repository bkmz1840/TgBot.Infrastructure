using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace TgBot.Infrastructure.Helpers;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServicesOf<TServiceInterface>(this IServiceCollection services)
    {
        var interfaceType = typeof(TServiceInterface);
        
        var allRealizationsTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(x => interfaceType != x && interfaceType.IsAssignableFrom(x));

        foreach (var realization in allRealizationsTypes)
        {
            services.AddSingleton(interfaceType, realization);
        }

        return services;
    }
}