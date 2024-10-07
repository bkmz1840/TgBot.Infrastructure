using Telegram.Bot.Types;

namespace TgBot.Infrastructure;

public record BotCallbackData(long ChatId, string Data, User User);