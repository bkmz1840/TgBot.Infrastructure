using Example.Contexts;
using Example.Faults;
using Example.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot.Infrastructure;
using TgBot.Infrastructure.Handlers;

namespace Example.Handlers;

internal class ExampleHandler(ISettings settings, ExampleMessageBuilder exampleMessageBuilder) : IHandler<ExampleHandlerContext>
{
    public string Command => "example";

    public async Task<HandleResult> HandleSetHandlerToActiveAsync(
        ITelegramBotClient botClient,
        BotUpdate update,
        ExampleHandlerContext? context,
        CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(
            update.ChatId,
            "Send me what you want to say",
            cancellationToken: cancellationToken);

        return new HandleResult
        {
            NewContext = new ExampleHandlerContext(),
            NeedUpdateContext = true,
            StayHandlerAsActive = true
        };
    }

    public async Task<HandleResult> HandleNextMessageAsync(
        ITelegramBotClient botClient,
        BotUpdate update,
        ExampleHandlerContext? context,
        CancellationToken cancellationToken)
    {
        if (context is null)
        {
            return new HandleResult
            {
                Error = new EmptyContextFault(),
                StayHandlerAsActive = true,
                NewContext = new ExampleHandlerContext(),
                NeedUpdateContext = true
            };
        }

        if (context.LastMessageId.HasValue && !context.LastMessageLiked)
        {
            await botClient.EditMessageReplyMarkupAsync(
                update.ChatId,
                context.LastMessageId.Value,
                cancellationToken: cancellationToken);
        }

        var newContext = new ExampleHandlerContext
        {
            CurrentText = update.Message.Text,
            LastText = context.CurrentText,
            LastMessageLiked = false
        };

        newContext.LastMessageId = await SendNewExampleMessageAsync(botClient, update.ChatId, newContext, cancellationToken);

        return new HandleResult
        {
            NewContext = newContext,
            NeedUpdateContext = true,
            StayHandlerAsActive = true
        };
    }

    public Task<HandleResult> ExecuteOnHandlerLeaveAsync(
        ITelegramBotClient botClient,
        ExampleHandlerContext? context,
        CancellationToken cancellationToken)
        => Task.FromResult(new HandleResult
        {
            NeedUpdateContext = true
        });

    private async Task<int> SendNewExampleMessageAsync(
        ITelegramBotClient botClient,
        long chatId,
        ExampleHandlerContext context,
        CancellationToken cancellationToken)
    {
        var markup = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                new InlineKeyboardButton("Like message?")
                {
                    CallbackData = $"LikeMessage{settings.CallbackDataPrefixDelimiter}true"
                }
            }
        });

        var messageText = exampleMessageBuilder.Build(context);

        var lastMessage = await botClient.SendTextMessageAsync(
            chatId,
            messageText,
            replyMarkup: markup,
            cancellationToken: cancellationToken);

        return lastMessage.MessageId;
    }
}