using TgBot.Infrastructure.Common.Context;
using TgBot.Infrastructure.Common.Faults;
using TgBot.Infrastructure.Handlers;

namespace TgBot.Infrastructure.Helpers;

public static class HandleResultBuilder
{
    public static HandleResult BuildFailedResult(
        Fault fault,
        bool stayHandlerAsActive = false,
        bool needUpdateContext = false)
        => new()
        {
            Error = fault,
            StayHandlerAsActive = stayHandlerAsActive,
            NeedUpdateContext = needUpdateContext
        };
}