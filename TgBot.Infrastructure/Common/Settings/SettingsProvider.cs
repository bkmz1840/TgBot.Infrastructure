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
        var json = File.ReadAllText(settingsFile);
        
        var settings = JsonSerializer.Deserialize(json, settingsClassType, JsonOptions) ??
                       Activator.CreateInstance(settingsClassType);

        if (settings is null)
        {
            throw new ApplicationException($"Could not create instance of '{settingsClassType.Name}'" +
                                           $"from settings file '{settingsFile}'");
        }
        
        return (ISettings)settings;
    }

    private static string FindSettingsFile(string environment)
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;

        var fileNameSuffix = string.IsNullOrEmpty(environment)
            ? string.Empty
            : $"-{environment}";
        
        var fileName = $"{ApplicationSettingsJsonFileName}{fileNameSuffix}.json";
        var options = new EnumerationOptions { RecurseSubdirectories = true };
        
        var settingsFiles = Directory.GetFiles(basePath, fileName, options);

        return settingsFiles.Length switch
        {
            0 => throw new FileNotFoundException($"Settings file not found '{fileName}' not found in project"),
            
            > 1 => throw new ApplicationException($"Found several '{fileName}' settings files. " +
                                                  $"Provide only one settings file for environment '{environment}'"),
            _ => settingsFiles.Single()
        };
    }
}