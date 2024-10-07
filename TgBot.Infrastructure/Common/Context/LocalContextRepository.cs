using System.Collections.Concurrent;
using TgBot.Infrastructure.Common.Faults;
using TgBot.Infrastructure.Handlers;
using TgBot.Infrastructure.Helpers;

namespace TgBot.Infrastructure.Common.Context;

/// <summary>
/// Simple implementation of IContextRepository.
/// </summary>
/// <remarks>Store contexts in RAM</remarks>
public class LocalContextRepository : IContextRepository
{
    private readonly ConcurrentDictionary<long, Dictionary<string, object>> handlerContextsByChatId = new();
    
    public Task<object?> TryGetHandlerContextAsync(long chatId, string handlerCommand)
    {
        var contextsByHandler = handlerContextsByChatId.GetValueOrDefault(chatId);
        var result = contextsByHandler?.GetValueOrDefault(handlerCommand);
        
        return Task.FromResult(result);
    }

    public Task<HandleResult> UpdateHandlerContextAsync(long chatId, string handlerCommand, object context)
    {
        if (!handlerContextsByChatId.ContainsKey(chatId) &&
            !handlerContextsByChatId.TryAdd(chatId, new Dictionary<string, object>()))
        {
            return Task.FromResult(HandleResultBuilder.BuildFailedResult(new ContextUpdateFailedFault()));
        }

        if (!handlerContextsByChatId.TryGetValue(chatId, out var contextsByHandler))
        {
            return Task.FromResult(HandleResultBuilder.BuildFailedResult(new ContextUpdateFailedFault()));
        }

        contextsByHandler[handlerCommand] = context;
        
        return Task.FromResult(new HandleResult
        {
            NewContext = context
        });
    }

    public Task<HandleResult> RemoveHandlerContextAsync(long chatId, string handlerCommand)
    {
        if (!handlerContextsByChatId.TryGetValue(chatId, out var contextsByHandler))
        {
            return Task.FromResult(new HandleResult());
        }

        contextsByHandler.Remove(handlerCommand);
        return Task.FromResult(new HandleResult());
    }
}