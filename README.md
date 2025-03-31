# TgBot Infrastructure framework

[![TgBot.Infrastructure](https://img.shields.io/nuget/vpre/TgBot.Infrastructure.svg)](https://www.nuget.org/packages/TgBot.Infrastructure)

## About

*TgBot.Infrastructure* is a **.NET Core** (net8.0) framework that help easier launch a telegram bots.  
Features:
* **Command handlers** with auto registration;
* **Callback handlers** from inline buttons with auto registration;
* **Shared context** of a handler with a simple management system; 
* **DI** that support registration of application specific services;
* **Environments**;
* **Application settings** that stored in json by environments;
* **Background worker** that allow run tasks by time interval.

## Quick start

Create `Settings` class/record that implements `ISettings` interface:
```c#
internal record MySettings : ISettings
{
    public string BotToken { get; init; } = "";  // Token from @BotFather
    public string CallbackDataPrefixDelimiter { get; init; } = ":";  // Using for callback data handlers: [handler name][CallbackDataPrefixDelimiter from settings][data]
    public bool FailUpdateOnContextUpdateFailed { get; init; } = true;  // Stop bot if handler fail update context
}
```

Create `Application` class that extend `TgBotApplication` abstract class and realize everything necessary.
Also, mark your class with `ApplicationSettings` attribute with type of created settings implementation:
```c#
[ApplicationSettings(typeof(MySettings))]
internal class MyTgBotApplication : TgBotApplication
{
    protected override void RegisterServices(IServiceCollection serviceCollection) 
    {
        // Register your services
    }

    protected override async Task OnHandleResultAsync(
        ITelegramBotClient botClient,
        Update update,
        HandleResult handleResult,
        CancellationToken cancellationToken)
    {
        // Handler return result
        
        if (handleResult.IsSuccess || update.Message?.Chat.Id is null)
        {
            return;
        }

        // Handle result is error
    }

    protected override async Task OnUnexpectedExceptionAsync(
        ITelegramBotClient botClient,
        Update update,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Exception was thrown
    }

    protected override async Task OnPollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Telegram API can't refresh updates 
    }
}
```
Add your *command handlers* that user can execute by sending your bot messages like `/command`.  
For example:
```c#
internal class MyHandler : IHandler
{
    public string Command => "my_command";  // name of command from message: /my_command

    public async Task<HandleResult> HandleSetHandlerToActiveAsync(
        ITelegramBotClient botClient,
        BotUpdate update,
        object? context,
        CancellationToken cancellationToken)
    {
        // Executes when it's first time user has sent command of this handler
    }

    public async Task<HandleResult> HandleNextMessageAsync(
        ITelegramBotClient botClient,
        BotUpdate update,
        object? context,
        CancellationToken cancellationToken)
    {
        // Executes on every update from user if handler is active 
        // (must be set in HandleResult by StayHandlerAsActive prop)
    }

    public Task<HandleResult> ExecuteOnHandlerLeaveAsync(
        ITelegramBotClient botClient,
        object? context,
        CancellationToken cancellationToken)
    {
        // Executes when user has sent an other command 
    }
}
```

## What's next?

You can check the runnable [Example application](https://github.com/bkmz1840/TgBot.Infrastructure/tree/master/Example) of telegram bot. *Just add your bot token to settings*