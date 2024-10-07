using Telegram.Bot;

namespace TgBot.Infrastructure.Jobs;

public interface IJob
{
    Task Execute(ITelegramBotClient botClient);
}