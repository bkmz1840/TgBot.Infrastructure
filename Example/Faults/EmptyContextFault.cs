using TgBot.Infrastructure.Common.Faults;

namespace Example.Faults;

internal class EmptyContextFault(string? message = "Where is my context?") : Fault(message);