using Telegram.Bot;

namespace TgBot.Infrastructure.Handlers;

/// <summary>
/// Handler of command with typed context
/// </summary>
/// <typeparam name="TContext">
/// Class that have default constructor without parameters and store data for user in chat for one handler.
/// For different handlers use different TContext classes.
/// </typeparam>
public interface IHandler<in TContext> : IHandler
{
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
        TContext? context,
        CancellationToken cancellationToken);

    Task<HandleResult> IHandler.HandleSetHandlerToActiveAsync(
        ITelegramBotClient botClient,
        BotUpdate update,
        object? context,
        CancellationToken cancellationToken) 
        => HandleSetHandlerToActiveAsync(botClient, update, (TContext?)context, cancellationToken);
    

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
        TContext? context,
        CancellationToken cancellationToken);

    Task<HandleResult> IHandler.HandleNextMessageAsync(
        ITelegramBotClient botClient,
        BotUpdate update,
        object? context,
        CancellationToken cancellationToken)
        => HandleNextMessageAsync(botClient, update, (TContext?)context, cancellationToken);

    /// <summary>
    /// Call when user send other command
    /// </summary>
    /// <param name="botClient">Client TG bot</param>
    /// <param name="context">Current context of handler in chat from update</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Result of handle</returns>
    Task<HandleResult> ExecuteOnHandlerLeaveAsync(
        ITelegramBotClient botClient,
        TContext? context,
        CancellationToken cancellationToken);

    Task<HandleResult> IHandler.ExecuteOnHandlerLeaveAsync(
        ITelegramBotClient botClient,
        object? context,
        CancellationToken cancellationToken)
        => ExecuteOnHandlerLeaveAsync(botClient, (TContext?)context, cancellationToken);
}