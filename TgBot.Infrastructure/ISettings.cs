namespace TgBot.Infrastructure;

public interface ISettings
{
    string BotToken { get; }
    string CallbackDataPrefixDelimiter { get; }
    bool FailUpdateOnContextUpdateFailed { get; }
}