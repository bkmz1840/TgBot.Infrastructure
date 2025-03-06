namespace Example;

internal static class Program
{
    public static async Task Main()
    {
        await using var tgBotApplication = new ExampleTgBotApplication();
        await tgBotApplication.StartAsync();
    }
}
