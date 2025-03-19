namespace TgBot.Infrastructure.Common.Settings;

public interface ISettings
{
    string BotToken { get; init; }
    string CallbackDataPrefixDelimiter { get; init; }
    bool FailUpdateOnContextUpdateFailed { get; init; }
}