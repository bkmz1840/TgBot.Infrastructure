using Telegram.Bot.Types;

namespace TgBot.Infrastructure;

public record BotUpdate(Message Message, long ChatId, User Sender);