using TgBot.Infrastructure.Common.Faults;

namespace Example.Faults;

internal class HandleFailedFault(string? message = "Can't handle action") : Fault(message);