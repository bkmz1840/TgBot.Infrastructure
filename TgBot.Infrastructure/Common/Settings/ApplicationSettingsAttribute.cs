namespace TgBot.Infrastructure.Common.Settings;

[AttributeUsage(AttributeTargets.Class)]
public class ApplicationSettingsAttribute : Attribute
{
    public readonly Type SettingsClassType;
    
    public ApplicationSettingsAttribute(Type settingsClassType)
    {
        var settingsInterfaceType = typeof(ISettings);

        if (settingsClassType == settingsInterfaceType ||
            settingsClassType.IsInterface ||
            !settingsInterfaceType.IsAssignableFrom(settingsClassType))
        {
            throw new ArgumentException($"Passed class '{settingsClassType.Name}' does not implement '{nameof(ISettings)}'");
        }

        SettingsClassType = settingsClassType;
    }
}