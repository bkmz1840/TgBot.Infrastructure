using System.Collections.Concurrent;
using Telegram.Bot;
using TgBot.Infrastructure.Common.Context;
using TgBot.Infrastructure.Common.Faults;
using TgBot.Infrastructure.Common.Settings;
using TgBot.Infrastructure.Helpers;

namespace TgBot.Infrastructure.Handlers;

internal class HandlerExecutor(
    IEnumerable<IHandler> handlers,
    ISettings settings,
    IContextRepository contextRepository) : IHandlerExecutor
{
    private readonly ConcurrentDictionary<long, IHandler> activeHandlerByChatId = new();

    public async Task<HandleResult> ExecuteHandlerAsync(
        ITelegramBotClient botClient,
        BotUpdate update,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(update.Message.Text) && update.Message.Text.StartsWith('/'))
        {
            return await SetNewActiveHandlerAsync(botClient, update, cancellationToken);
        }

        return await HandleNextMessageAsync(botClient, update, cancellationToken);
    }

    public async Task<HandleResult> ExecuteCallbackDataHandlerAsync(
        ITelegramBotClient botClient,
        BotCallbackData callbackData,
        CancellationToken cancellationToken)
    {
        if (!activeHandlerByChatId.TryGetValue(callbackData.ChatId, out var activeHandler))
        {
            return new HandlerNotFoundFault().AsFailedResult();
        }

        return await CallHandlerAsync(
            callbackData.ChatId,
            activeHandler.Command,
            async context => await activeHandler.HandleCallBackDataAsync(
                botClient,
                callbackData,
                context,
                cancellationToken));
    }

    private async Task<HandleResult> SetNewActiveHandlerAsync(
        ITelegramBotClient botClient,
        BotUpdate update,
        CancellationToken cancellationToken)
    {
        var command = update.Message.Text![1..];
        var newHandler = handlers.FirstOrDefault(x => x.Command == command);

        if (newHandler is null)
        {
            return new HandlerNotFoundFault().AsFailedResult();
        }

        if (activeHandlerByChatId.TryRemove(update.ChatId, out var oldHandler))
        {
            var oldHandlerResult = await CallHandlerAsync(
                update.ChatId,
                oldHandler.Command,
                async context => await oldHandler.ExecuteOnHandlerLeaveAsync(botClient, context, cancellationToken));

            if (!oldHandlerResult.IsSuccess)
            {
                return oldHandlerResult;
            }
        }

        var newHandlerResult = await CallHandlerAsync(
            update.ChatId,
            newHandler.Command,
            async context => await newHandler.HandleSetHandlerToActiveAsync(
                botClient,
                update,
                context,
                cancellationToken));

        if (newHandlerResult.StayHandlerAsActive && !activeHandlerByChatId.TryAdd(update.ChatId, newHandler))
        {
            return new SwitchHandlerFault().AsFailedResult();
        }

        return newHandlerResult;
    }

    private async Task<HandleResult> HandleNextMessageAsync(
        ITelegramBotClient botClient,
        BotUpdate update,
        CancellationToken cancellationToken)
    {
        if (!activeHandlerByChatId.TryGetValue(update.ChatId, out var activeHandler))
        {
            return new HandlerNotFoundFault().AsFailedResult();
        }

        var handleResult = await CallHandlerAsync(
            update.ChatId,
            activeHandler.Command,
            async context => await activeHandler.HandleNextMessageAsync(
                botClient,
                update,
                context,
                cancellationToken));

        if (!handleResult.StayHandlerAsActive && 
            !activeHandlerByChatId.TryRemove(update.ChatId, out _))
        {
            return new SwitchHandlerFault().AsFailedResult();
        }

        return handleResult;
    }

    private async Task<HandleResult> CallHandlerAsync(
        long chatId,
        string handlerCommand,
        Func<object?, Task<HandleResult>> handlerCall)
    {
        var context = await contextRepository.TryGetHandlerContextAsync(chatId, handlerCommand);
        var handleResult = await handlerCall(context);

        var updateResult = await UpdateHandlerContextIfNeededAsync(chatId, handlerCommand, handleResult);
        
        return !updateResult.IsSuccess ? updateResult : handleResult;
    }

    private async Task<HandleResult> UpdateHandlerContextIfNeededAsync(
        long chatId,
        string handlerCommand,
        HandleResult handleResult)
    {
        if (!handleResult.NeedUpdateContext)
        {
            return new HandleResult();
        }
        
        var updateResult = handleResult.NewContext is null
            ? await contextRepository.RemoveHandlerContextAsync(chatId, handlerCommand)
            : await contextRepository.UpdateHandlerContextAsync(chatId, handlerCommand, handleResult.NewContext);

        if (!updateResult.IsSuccess && settings.FailUpdateOnContextUpdateFailed)
        {
            return updateResult;
        }

        return new HandleResult();
    }
}