using TgBot.Infrastructure;

namespace Example;

internal class Settings : ISettings
{
    public string BotToken => "";
    public string CallbackDataPrefixDelimiter => ":";
    public bool FailUpdateOnContextUpdateFailed => true;
}