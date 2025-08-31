# Environments and Settings

## About

*Settings* are some const values that use in whole project. 
It's normal situation store db connection data, tokens, API keys in separate file in one place. 
But often project have different settings for different **environments**. For example, *test database*.  

You can store settings values into settings class as default values of properties. This was shown into [README](https://github.com/bkmz1840/TgBot.Infrastructure/blob/master/README.md).
But sometimes it's not useful. Maybe you want to share settings for one project to others projects. 
Maybe you don't want to search and collect your settings into project tree. Thus, you can use **settings json file**.

**Settings json file** is a json file that can be deserialized to your implementation of `ISettings` interface.


## Usage

*Environment* value stored into `TgBotApplication` class as property `EnvironmentName`. 
By default, value comes from *System Environment* by key `BOT_ENVIRONMENT`. Of course, it can be overriden.  

For example:
```c#
protected override string EnvironmentName
{
    get
    {
#if DEBUG
        return "debug";
#else
        return "release";
#endif
    }
}
```
  
**Settings json file** is json file that **added to build (!!!)**

Example:  
*Settings class*
```c#
internal record Settings : ISettings
{
    public string BotToken { get; init; }
    public string CallbackDataPrefixDelimiter { get; init; } = ":";
    public bool FailUpdateOnContextUpdateFailed { get; init; } = true;
}
```
*settings-debug.json*
