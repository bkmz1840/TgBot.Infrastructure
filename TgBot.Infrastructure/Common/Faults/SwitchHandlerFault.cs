namespace TgBot.Infrastructure.Common.Faults;

public class SwitchHandlerFault(string? message = "Can't switch handler") : Fault(message);