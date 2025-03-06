using Telegram.Bot;
using TgBot.Infrastructure;
using TgBot.Infrastructure.Handlers;

namespace Example.Handlers;

internal class StartHandler : IHandler
{
    public string Command => "start";

    public async Task<HandleResult> HandleSetHandlerToActiveAsync(
        ITelegramBotClient botClient,
        BotUpdate update,
        object? context,
        CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(
            update.ChatId,
            "Hello! What's your name?",
            cancellationToken: cancellationToken);

        return new HandleResult
        {
            NewContext = context,
            NeedUpdateContext = false,
            StayHandlerAsActive = true
        };
    }

    public async Task<HandleResult> HandleNextMessageAsync(
        ITelegramBotClient botClient,
        BotUpdate update,
        object? context,
        CancellationToken cancellationToken)
    {
        var greetingsText = $"I'm glad to meet you, {update.Message.Text ?? "strange"}";

        await botClient.SendTextMessageAsync(
            update.ChatId,
            greetingsText,
            cancellationToken: cancellationToken);

        return new HandleResult
        {
            NewContext = context,
            NeedUpdateContext = false,
            StayHandlerAsActive = false
        };
    }

    public Task<HandleResult> ExecuteOnHandlerLeaveAsync(
        ITelegramBotClient botClient,
        object? context,
        CancellationToken cancellationToken)
        => Task.FromResult(new HandleResult());
}