using Example.Contexts;
using Example.Faults;
using Example.Helpers;
using Example.Requests;
using Telegram.Bot;
using TgBot.Infrastructure;
using TgBot.Infrastructure.Common.Faults;
using TgBot.Infrastructure.Handlers;

namespace Example.Handlers;

internal class LikeMessageCallbackDataHandler(ExampleMessageBuilder exampleMessageBuilder) : ICallbackDataHandler<ExampleHandlerContext>
{
    public string CallbackDataPrefix => "LikeMessage";

    public async Task<HandleResult> HandleCallbackDataAsync(
        ITelegramBotClient botClient,
        BotCallbackData callbackData,
        ExampleHandlerContext? context,
        CancellationToken cancellationToken)
    {
        if (callbackData.Data != "true")
        {
            return new HandleResult
            {
                Error = new CallbackDataInvalidFault()
            };
        }

        if (context?.LastMessageId is null)
        {
            return new HandleResult
            {
                Error = new EmptyContextFault()
            };
        }

        if (context.LastMessageLiked)
        {
            return new HandleResult();
        }

        return await LikeMessageAsync(botClient, callbackData.ChatId, context, cancellationToken);
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
            ? new HandleResult
            {
                Error = new HandleFailedFault()
            }
            : new HandleResult
            {
                NewContext = newContext,
                NeedUpdateContext = true
            };
    }
}