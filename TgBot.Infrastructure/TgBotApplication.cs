using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot.Infrastructure.Common;
using TgBot.Infrastructure.Common.Context;
using TgBot.Infrastructure.Common.Faults;
using TgBot.Infrastructure.Common.Settings;
using TgBot.Infrastructure.Handlers;
using TgBot.Infrastructure.Helpers;
using TgBot.Infrastructure.Jobs;

namespace TgBot.Infrastructure;

public abstract class TgBotApplication : IDisposable, IAsyncDisposable
{
    private const string ApplicationEnvironmentSystemVariableName = "BOT_ENVIRONMENT"; 
    
    private readonly CancellationTokenRegistration cancellationTokenRegistration;
    private readonly ServiceProvider serviceProvider;
    
    private ITelegramBotClient bot = null!;
    private IHandlerExecutor executor = null!;
    private BackgroundWorker backgroundWorker = null!;

    protected virtual string EnvironmentName
        => Environment.GetEnvironmentVariable(ApplicationEnvironmentSystemVariableName) ?? string.Empty;

    protected TgBotApplication()
    {
        cancellationTokenRegistration = new CancellationTokenRegistration();
        serviceProvider = SetupServices();
    }

    public async Task StartAsync()
    {
        await InitializeServices();
        executor = serviceProvider.GetRequiredService<IHandlerExecutor>();
        
        var settings = serviceProvider.GetRequiredService<ISettings>();
        
        bot = new TelegramBotClient(settings.BotToken);
        backgroundWorker = serviceProvider.GetRequiredService<BackgroundWorker>();

        var cancellationToken = cancellationTokenRegistration.Token;
        cancellationToken.Register(async () => await DisposeAsync());
        
        backgroundWorker.Start(bot, cancellationToken);
        
        await bot.ReceiveAsync(
            OnUpdateAsync,
            OnPollingErrorAsync,
            cancellationToken: cancellationToken);
    }

    protected abstract void RegisterServices(IServiceCollection serviceCollection);
    
    private ServiceProvider SetupServices()
    {
        var services = new ServiceCollection()
            .AddSettings(this, EnvironmentName)
            .AddSingleton<IContextRepository, LocalContextRepository>();

        RegisterServices(services);

        return services
            .AddServicesOf<IHandler>()
            .AddServicesOf<ICallbackDataHandler>()
            .AddServicesOf<IJob>()
            .AddServicesOf<IInitializable>()
            .AddSingleton<IHandlerExecutor, HandlerExecutor>()
            .AddSingleton<BackgroundWorker>()
            .BuildServiceProvider();
    }

    private async Task InitializeServices()
    {
        foreach (var initializable in serviceProvider.GetServices<IInitializable>())
        {
            await initializable.InitializeAsync();
        }
    }

    private async Task OnUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery is not null)
        {
            await OnCallbackDataGetAsync(botClient, update, cancellationToken);
            return;
        }

        if (update.Message is null)
        {
            await OnHandleResultAsync(
                botClient,
                update,
                new EmptyMessageFault().AsFailedResult(),
                cancellationToken);
            return;
        }

        if (update.Message.From is null)
        {
            await OnHandleResultAsync(
                botClient,
                update,
                new UserNotFoundFault().AsFailedResult(),
                cancellationToken);
            return;
        }

        var wrappedUpdate = new BotUpdate(update.Message, update.Message.Chat.Id, update.Message.From);

        try
        {
            var handleData = await executor.ExecuteHandlerAsync(botClient, wrappedUpdate, cancellationToken);
            await OnHandleResultAsync(botClient, update, handleData, cancellationToken);
        }
        catch (Exception exception)
        {
            await OnUnexpectedExceptionAsync(botClient, update, exception, cancellationToken);
        }
    }

    private async Task OnCallbackDataGetAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(update.CallbackQuery!.Data))
        {
            await OnHandleResultAsync(
                botClient,
                update,
                new CallbackDataInvalidFault().AsFailedResult(),
                cancellationToken);
            return;
        }

        if (update.CallbackQuery.Message?.Chat.Id is null)
        {
            await OnHandleResultAsync(
                botClient,
                update,
                new ChatNotFoundFault().AsFailedResult(),
                cancellationToken);
            return;
        }

        var wrappedUpdate = new BotCallbackData(
            update.CallbackQuery.Message!.Chat.Id,
            update.CallbackQuery.Data,
            update.CallbackQuery.From);

        try
        {
            var handleData = await executor.ExecuteCallbackDataHandlerAsync(
                botClient,
                wrappedUpdate,
                cancellationToken);
            
            await OnHandleResultAsync(botClient, update, handleData, cancellationToken);
        }
        catch (Exception exception)
        {
            await OnUnexpectedExceptionAsync(botClient, update, exception, cancellationToken);
        }
    }

    protected abstract Task OnHandleResultAsync(
        ITelegramBotClient botClient,
        Update update,
        HandleResult handleResult,
        CancellationToken cancellationToken);

    protected abstract Task OnUnexpectedExceptionAsync(
        ITelegramBotClient botClient,
        Update update,
        Exception exception,
        CancellationToken cancellationToken);
    
    protected abstract Task OnPollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken);

    public void Dispose()
    {
        cancellationTokenRegistration.Dispose();
        serviceProvider.Dispose();
        backgroundWorker.Dispose();

        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await cancellationTokenRegistration.DisposeAsync();
        await serviceProvider.DisposeAsync();
        await backgroundWorker.DisposeAsync();

        GC.SuppressFinalize(this);
    }
}