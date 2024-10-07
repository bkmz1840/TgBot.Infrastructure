namespace TgBot.Infrastructure.Common.Faults;

public class CallbackDataInvalidFault(
    string? message = "Callback data is invalid (null or empty or has not delimiter)") : Fault(message);