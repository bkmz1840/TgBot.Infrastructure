namespace TgBot.Infrastructure.Common.Faults;

public class HandlerNotFoundFault(string? message = "Can't find handler with specified command") : Fault(message);