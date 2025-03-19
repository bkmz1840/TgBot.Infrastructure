using TgBot.Infrastructure.Common.Settings;

namespace Example;

internal class Settings : ISettings
{
    public string BotToken { get; init; } = "";
    public string CallbackDataPrefixDelimiter { get; init; } = ":";
    public bool FailUpdateOnContextUpdateFailed { get; init; } = true;
}