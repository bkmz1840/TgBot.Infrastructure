namespace TgBot.Infrastructure.Common.Settings;

public interface ISettings
{
    string BotToken { get; init; }
    bool FailUpdateOnContextUpdateFailed { get; init; }
}