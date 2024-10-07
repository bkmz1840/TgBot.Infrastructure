namespace TgBot.Infrastructure.Common.Faults;

public class UserNotFoundFault(string? message = "Sender of message is null") : Fault(message);