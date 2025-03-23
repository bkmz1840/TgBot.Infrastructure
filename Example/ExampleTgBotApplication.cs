using Example.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot.Infrastructure;
using TgBot.Infrastructure.Common.Faults;
using TgBot.Infrastructure.Common.Settings;
using TgBot.Infrastructure.Handlers;

namespace Example;

[ApplicationSettings(typeof(Settings))]
internal class ExampleTgBotApplication : TgBotApplication
{
    private const string DefaultErrorText = "Oh... Something gone wrong. Please, try later";
    private const string HandlerNotFoundMessage = "Please, select a command";

    protected override string EnvironmentName
    {
        get
        {
// For example
#if DEBUG
            return "debug";
#else
            return "release";
#endif
        }
    }

    protected override void RegisterServices(IServiceCollection serviceCollection)
        => serviceCollection.AddSingleton<ExampleMessageBuilder>();

    protected override async Task OnHandleResultAsync(
        ITelegramBotClient botClient,
        Update update,
        HandleResult handleResult,
        CancellationToken cancellationToken)
    {
        if (handleResult.IsSuccess || update.Message?.Chat.Id is null)
        {
            return;
        }

        var message = handleResult.Error is HandlerNotFoundFault
            ? HandlerNotFoundMessage
            : DefaultErrorText;
        
        await botClient.SendTextMessageAsync(
            update.Message!.Chat.Id,
            message,
            cancellationToken: cancellationToken);
    }

    protected override async Task OnUnexpectedExceptionAsync(
        ITelegramBotClient botClient,
        Update update,
        Exception exception,
        CancellationToken cancellationToken)
    {
        Console.WriteLine("Unexpected exception occured:");
        LogException(exception);

        if (update.Message?.Chat.Id is null) 
        {
            return;
        }
        
        await botClient.SendTextMessageAsync(
            update.Message!.Chat.Id,
            DefaultErrorText,
            cancellationToken: cancellationToken);
    }

    protected override async Task OnPollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken)
    {
        Console.WriteLine("Polling of new updates is failed:");
        LogException(exception);

        try
        {
            var botUser = await botClient.GetMeAsync(cancellationToken);
            Console.WriteLine($"Bot ({botUser.Username}) is alive");
        }
        catch (Exception getMeException)
        {
            Console.WriteLine("Oh no.. Bot is dead.\nGetMe exception:");
            LogException(getMeException);
            
            Environment.Exit(1);
        }
    }

    private static void LogException(Exception exception)
    {
        Console.WriteLine($"{exception.Message}\n{exception.StackTrace}");
    }
}