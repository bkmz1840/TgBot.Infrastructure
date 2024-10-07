using TgBot.Infrastructure.Common.Faults;

namespace TgBot.Infrastructure.Handlers;

public record HandleResult
{
    public Fault? Error { get; init; }
    public bool StayHandlerAsActive { get; init; }
    public bool NeedUpdateContext { get; init; }
    public object? NewContext { get; init; }
    
    public bool IsSuccess => Error is null;

    public void EnsureSuccess()
    {
        if (!IsSuccess)
        {
            throw Error!;
        }
    } 
}