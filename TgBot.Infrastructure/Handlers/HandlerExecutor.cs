using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TgBot.Infrastructure.Common.Context;
using TgBot.Infrastructure.Common.Faults;
using TgBot.Infrastructure.Helpers;

namespace TgBot.Infrastructure.Handlers;

internal class HandlerExecutor(
    IServiceProvider serviceProvider,
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
        var splitCallbackData = callbackData.Data.Split(settings.CallbackDataPrefixDelimiter);

        if (splitCallbackData.Length < 2)
        {
            return HandleResultBuilder.BuildFailedResult(new CallbackDataInvalidFault());
        }
        
        var callbackDataHandler = serviceProvider
            .GetServices<ICallbackDataHandler>()
            .FirstOrDefault(x => x.CallbackDataPrefix == splitCallbackData.First());
        
        if (!activeHandlerByChatId.TryGetValue(callbackData.ChatId, out var activeHandler) || callbackDataHandler is null)
        {
            return HandleResultBuilder.BuildFailedResult(new HandlerNotFoundFault());
        }

        var updatedCallbackData = callbackData with
        {
            Data = string.Join(settings.CallbackDataPrefixDelimiter, splitCallbackData.Skip(1))
        };

        return await CallHandlerAsync(
            callbackData.ChatId,
            activeHandler.Command,
            async context => await callbackDataHandler.HandleCallbackDataAsync(
                botClient,
                updatedCallbackData,
                context,
                cancellationToken));
    }

    private async Task<HandleResult> SetNewActiveHandlerAsync(
        ITelegramBotClient botClient,
        BotUpdate update,
        CancellationToken cancellationToken)
    {
        var command = update.Message.Text![1..];
        var newHandler = serviceProvider
            .GetServices<IHandler>()
            .FirstOrDefault(x => x.Command == command);

        if (newHandler is null)
        {
            return HandleResultBuilder.BuildFailedResult(new HandlerNotFoundFault());
        }

        if (activeHandlerByChatId.TryRemove(update.ChatId, out var oldHandler) && oldHandler is not null)
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
            return HandleResultBuilder.BuildFailedResult(new SwitchHandlerFault());
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
            return HandleResultBuilder.BuildFailedResult(new HandlerNotFoundFault());
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
            return HandleResultBuilder.BuildFailedResult(new SwitchHandlerFault());
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