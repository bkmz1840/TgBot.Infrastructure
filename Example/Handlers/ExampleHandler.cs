using Example.Contexts;
using Example.Faults;
using Example.Helpers;
using Example.Requests;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot.Infrastructure;
using TgBot.Infrastructure.Common.Faults;
using TgBot.Infrastructure.Handlers;
using TgBot.Infrastructure.Helpers;

namespace Example.Handlers;

internal class ExampleHandler(ExampleMessageBuilder exampleMessageBuilder) : IHandler<ExampleHandlerContext>
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
            "Send me what you want to say.\n" +
            "After you can continue sending messages that check what happened",
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

        if (context is { LastMessageId: not null, LastMessageLiked: false })
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

    public async Task<HandleResult> HandleCallBackDataAsync(
        ITelegramBotClient botClient, 
        BotCallbackData callbackData, 
        ExampleHandlerContext? context,
        CancellationToken cancellationToken)
    {
        if (callbackData.Data != "true")
        {
            return new CallbackDataInvalidFault().AsFailedResult(true);
        }

        if (context?.LastMessageId is null)
        {
            return new EmptyContextFault().AsFailedResult(true);
        }

        if (context.LastMessageLiked)
        {
            return HandleResult.Success(true);
        }

        return await LikeMessageAsync(botClient, callbackData.ChatId, context, cancellationToken);
    }

    private async Task<int> SendNewExampleMessageAsync(
        ITelegramBotClient botClient,
        long chatId,
        ExampleHandlerContext context,
        CancellationToken cancellationToken)
    {
        var markup = new InlineKeyboardMarkupBuilder()
            .AddButtonRow()
            .AddButton(new InlineKeyboardButton("Like message?") { CallbackData = "true" })
            .Build();

        var lastMessage = await botClient.SendTextMessageAsync(
            chatId,
            exampleMessageBuilder.Build(context),
            replyMarkup: markup,
            cancellationToken: cancellationToken);

        return lastMessage.MessageId;
    }
    
    private async Task<HandleResult> LikeMessageAsync(
        ITelegramBotClient botClient,
        long chatId,
        ExampleHandlerContext context,
        CancellationToken cancellationToken)
    {
        var request = new SetReactionRequest(chatId, context.LastMessageId!.Value);
        var response = await botClient.MakeRequestAsync(request, cancellationToken: cancellationToken);

        var newContext = context with { LastMessageLiked = true };
        var newText = exampleMessageBuilder.Build(newContext);

        await botClient.EditMessageTextAsync(
            chatId,
            context.LastMessageId.Value,
            newText,
            cancellationToken: cancellationToken);

        return !response
            ? new HandleFailedFault().AsFailedResult(true)
            : new HandleResult
            {
                NewContext = newContext,
                NeedUpdateContext = true,
                StayHandlerAsActive = true
            };
    }
}