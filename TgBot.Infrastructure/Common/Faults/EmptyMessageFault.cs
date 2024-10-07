namespace TgBot.Infrastructure.Common.Faults;

public class EmptyMessageFault(string? message = "Message text is null or empty") : Fault(message);