namespace TgBot.Infrastructure.Common.Faults;

public class ChatNotFoundFault(string? message = "Can't determine chat id") : Fault(message);