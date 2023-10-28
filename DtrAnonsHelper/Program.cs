using DtrAnonsHelper.BotClient;
using DtrAnonsHelper.Business;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Telegram.Bot;
using CsvParser = DtrAnonsHelper.Business.CsvParser;

namespace DtrAnonsHelper;

class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Warning()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
        
        var host = new HostBuilder()
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddJsonFile($"./appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices(ConfigureServices)
            .UseConsoleLifetime()
            .Build();

        using (host)
        {
            var botClient = host.Services.GetRequiredService<BotClient.BotClient>();
            await botClient.BotOperations();

            await host.WaitForShutdownAsync();
        }
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection collection)
    {
        collection.AddSingleton<ITelegramBotClient>(_ =>
        {
            var token = context.Configuration.GetSection("BotToken").GetValue<string>("BotToken");
            return new TelegramBotClient(token!);
        });
        
        collection.AddSerilog();

        collection.AddTransient<BotClient.BotClient>();
        
        collection.AddTransient<CsvParser>();
        collection.AddTransient<AnnounceCreator>();
        collection.AddTransient<BotHandleFile>();
    }
}