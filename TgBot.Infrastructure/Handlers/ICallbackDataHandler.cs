using Telegram.Bot;

namespace TgBot.Infrastructure.Handlers;

/// <summary>
/// Handler for callback query
/// </summary>
/// <remarks>
/// For call handler add InlineKeyboardButton with specific data format:
/// [CallbackDataPrefix][ISettings.CallbackDataPrefixDelimiter][DataForHandler]
/// </remarks>
public interface ICallbackDataHandler
{
    /// <summary>
    /// Prefix in CallbackData for search ICallbackDataHandler 
    /// </summary>
    public string CallbackDataPrefix { get; }

    /// <summary>
    /// Handle callback query send from user in chat
    /// </summary>
    /// <param name="botClient">Tg bot client</param>
    /// <param name="callbackData">Wrapped callback query</param>
    /// <param name="context">Current context of user in chat (by active handler command)</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Result of handle</returns>
    Task<HandleResult> HandleCallbackDataAsync(
        ITelegramBotClient botClient,
        BotCallbackData callbackData,
        object? context,
        CancellationToken cancellationToken);
}