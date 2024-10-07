namespace TgBot.Infrastructure.Common.Faults;

public class MessageNotFoundFault(string? message = "Message in update is null") : Fault(message);