namespace TgBot.Infrastructure.Common.Faults;

public class ContextUpdateFailedFault(string? message = "Can't update handler context") : Fault(message);