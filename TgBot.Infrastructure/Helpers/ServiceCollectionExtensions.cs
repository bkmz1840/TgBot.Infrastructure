using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TgBot.Infrastructure.Common.Settings;

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

    public static IServiceCollection AddSettings(
        this IServiceCollection services,
        TgBotApplication application,
        string environment)
    {
        var applicationType = application.GetType();
        var attribute = applicationType.GetCustomAttribute<ApplicationSettingsAttribute>();

        if (attribute is null)
        {
            throw new ApplicationException($"Implementation '{nameof(ISettings)}' for '{applicationType.Name}' is not found. " +
                                           $"Mark your application class using '{nameof(ApplicationSettingsAttribute)}'");
        }

        var settings = SettingsProvider.Get(attribute.SettingsClassType, environment);
        return services.AddSingleton(typeof(ISettings), settings);
    }
}