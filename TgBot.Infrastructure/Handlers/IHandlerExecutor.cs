using Telegram.Bot;

namespace TgBot.Infrastructure.Handlers;

internal interface IHandlerExecutor
{
    Task<HandleResult> ExecuteHandlerAsync(
        ITelegramBotClient botClient,
        BotUpdate update,
        CancellationToken cancellationToken);
    
    Task<HandleResult> ExecuteCallbackDataHandlerAsync(
        ITelegramBotClient botClient,
        BotCallbackData callbackData,
        CancellationToken cancellationToken);
}