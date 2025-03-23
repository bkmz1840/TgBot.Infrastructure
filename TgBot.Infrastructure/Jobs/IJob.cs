using Telegram.Bot;

namespace TgBot.Infrastructure.Jobs;

public interface IJob
{
    public TimeSpan Interval { get; }
    public bool OneTime => false;
    
    Task DoWork(ITelegramBotClient botClient, CancellationToken cancellationToken);
}