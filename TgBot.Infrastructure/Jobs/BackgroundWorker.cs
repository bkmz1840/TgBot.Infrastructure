using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace TgBot.Infrastructure.Jobs;

internal class BackgroundWorker(IServiceProvider serviceProvider) : IDisposable, IAsyncDisposable
{
    private List<Timer> jobs = null!;
    
    public void Start(ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        jobs = serviceProvider
            .GetServices<IJob>()
            .Select(x => new Timer(async _ =>
                {
                    await x.DoWork(botClient, cancellationToken);
                },
                null,
                x.Interval,
                x.OneTime ? TimeSpan.FromMilliseconds(-1) : x.Interval))
            .ToList();
    }

    public void Dispose()
    {
        jobs.ForEach(x => x.Dispose());
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var job in jobs)
        {
            await job.DisposeAsync();
        }
        
        GC.SuppressFinalize(this);
    }
}