namespace TgBot.Infrastructure.Common.Faults;

public class Fault(string? message) : ApplicationException(message);