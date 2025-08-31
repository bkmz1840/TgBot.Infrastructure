using Telegram.Bot;
using TgBot.Infrastructure.Jobs;

namespace Example.Jobs;

public class AliveJob : IJob
{
    // For example 
    private const long ChatId = 0;
    
    public TimeSpan Interval => TimeSpan.FromSeconds(30);
    
    public async Task DoWork(ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        if (ChatId == 0)
        {
            return;
        }
        
        await botClient.SendTextMessageAsync(
            ChatId,
            "I am alive \u2699\ufe0f\n" +
            $"Bot time: {DateTime.Now:HH:mm:ss}",
            cancellationToken: cancellationToken);
    }
}