using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace TgBot.Infrastructure.Common.Settings;

internal static class SettingsProvider
{
    private const string ApplicationSettingsJsonFileName = "settings";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        Converters = { new JsonStringEnumConverter() }
    };

    public static ISettings Get(Type settingsClassType, string environment)
    {
        var settingsFile = FindSettingsFile(environment);

        var settings = string.IsNullOrEmpty(settingsFile)
            ? Activator.CreateInstance(settingsClassType)
            : JsonSerializer.Deserialize(File.ReadAllText(settingsFile), settingsClassType, JsonOptions);

        if (settings is null)
        {
            throw new ApplicationException($"Could not create instance of '{settingsClassType.Name}'");
        }
        
        return (ISettings)settings;
    }

    private static string? FindSettingsFile(string environment)
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;

        var fileNameSuffix = string.IsNullOrEmpty(environment)
            ? string.Empty
            : $"-{environment}";
        
        var fileName = $"{ApplicationSettingsJsonFileName}{fileNameSuffix}.json";
        var options = new EnumerationOptions { RecurseSubdirectories = true };
        
        return Directory.GetFiles(basePath, fileName, options).FirstOrDefault();
    }
}