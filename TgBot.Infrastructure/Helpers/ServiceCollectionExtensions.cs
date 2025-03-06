using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace TgBot.Infrastructure.Helpers;

internal static class ServiceCollectionExtensions
{
    private static readonly IEnumerable<Type> AllTypes;

    static ServiceCollectionExtensions() 
    {
        var entryAssembly = Assembly.GetEntryAssembly() ?? throw new ArgumentException("Assembly of application is not found");
        var libAssembly = Assembly.GetExecutingAssembly();

        AllTypes = entryAssembly
            .GetTypes()
            .Concat(libAssembly.GetTypes());
    }

    public static IServiceCollection AddServicesOf<TServiceInterface>(this IServiceCollection services)
    {
        var interfaceType = typeof(TServiceInterface);

        foreach (var realization in AllTypes.Where(x => interfaceType != x && !x.IsInterface && interfaceType.IsAssignableFrom(x)))
        {
            services.AddSingleton(interfaceType, realization);
        }

        return services;
    }
}