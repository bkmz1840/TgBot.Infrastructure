using TgBot.Infrastructure.Handlers;

namespace TgBot.Infrastructure.Common.Context;

/// <summary>
/// Repository for contexts of users in chats for handlers
/// </summary>
public interface IContextRepository
{
    /// <summary>
    /// Try get handler context of chat or return null if absent
    /// </summary>
    /// <param name="chatId">Id of chat</param>
    /// <param name="handlerCommand">Command that set specific handler as active</param>
    /// <returns>Result of search handler context by chat</returns>
    Task<object?> TryGetHandlerContextAsync(long chatId, string handlerCommand);

    /// <summary>
    /// Update handler context of chat
    /// </summary>
    /// <param name="chatId">Id of chat</param>
    /// <param name="handlerCommand">Command that set specific handler as active</param>
    /// <param name="context">Updated model of context</param>
    /// <returns>Result of update handler context by chat</returns>
    Task<HandleResult> UpdateHandlerContextAsync(long chatId, string handlerCommand, object context);
    
    /// <summary>
    /// Remove handler context of chat
    /// </summary>
    /// <param name="chatId">Id of chat</param>
    /// <param name="handlerCommand">Command that set specific handler as active</param>
    /// <returns>Result of remove handler context by chat</returns>
    Task<HandleResult> RemoveHandlerContextAsync(long chatId, string handlerCommand);
}