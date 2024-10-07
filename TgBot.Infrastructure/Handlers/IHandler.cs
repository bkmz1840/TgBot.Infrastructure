using Telegram.Bot;

namespace TgBot.Infrastructure.Handlers;

/// <summary>
/// Handler of command 
/// </summary>
public interface IHandler
{
    /// <summary>
    /// Command that set this handler as active
    /// </summary>
    string Command { get; }

    /// <summary>
    /// Call when user send command to chat 
    /// </summary>
    /// <param name="botClient">Client TG bot</param>
    /// <param name="update">Wrapped action call from user</param>
    /// <param name="context">Current context of handler in chat from update</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Result of handle</returns>
    Task<HandleResult> HandleSetHandlerToActiveAsync(
        ITelegramBotClient botClient,
        BotUpdate update,
        object? context,
        CancellationToken cancellationToken);

    /// <summary>
    /// Call when user send message for active handler
    /// </summary>
    /// <param name="botClient">Client TG bot</param>
    /// <param name="update">Wrapped text message from user</param>
    /// <param name="context">Current context of handler in chat from update</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Result of handle</returns>
    Task<HandleResult> HandleNextMessageAsync(
        ITelegramBotClient botClient,
        BotUpdate update,
        object? context,
        CancellationToken cancellationToken);

    /// <summary>
    /// Call when user send other command
    /// </summary>
    /// <param name="botClient">Client TG bot</param>
    /// <param name="context">Current context of handler in chat</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Result of handle</returns>
    Task<HandleResult> ExecuteOnHandlerLeaveAsync(
        ITelegramBotClient botClient,
        object? context,
        CancellationToken cancellationToken);
}