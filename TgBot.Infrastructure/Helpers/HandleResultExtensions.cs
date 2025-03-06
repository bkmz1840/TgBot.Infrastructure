using TgBot.Infrastructure.Common.Faults;
using TgBot.Infrastructure.Handlers;

namespace TgBot.Infrastructure.Helpers;

public static class HandleResultExtensions
{
    public static HandleResult AsFailedResult(
        this Fault fault,
        bool stayHandlerAsActive = false,
        bool needUpdateContext = false)
        => new()
        {
            Error = fault,
            StayHandlerAsActive = stayHandlerAsActive,
            NeedUpdateContext = needUpdateContext
        };
}