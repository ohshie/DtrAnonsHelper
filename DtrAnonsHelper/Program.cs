using DtrAnonsHelper.BotClient;
using DtrAnonsHelper.Business;
using DtrAnonsHelper.DataLayer;
using DtrAnonsHelper.DataLayer.DbContext;
using DtrAnonsHelper.DataLayer.Repository;
using DtrAnonsHelper.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Telegram.Bot;

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
            var dbContext = host.Services.GetRequiredService<AnonsContext>();
            await dbContext.Database.EnsureCreatedAsync();
            
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

        collection.AddDbContext<AnonsContext>(c =>
        {
            c.UseSqlite(context.Configuration.GetConnectionString("DefaultConnection"));
        });
        
        collection.AddSerilog();

        collection.AddTransient<BotClient.BotClient>();
        
        collection.AddTransient<CsvToDbParser>();
        collection.AddTransient<AnnounceCreator>();
        collection.AddTransient<AnnounceCreator>();
        collection.AddTransient<BotHandleFile>();
        collection.AddTransient<VideoUrlFetcher>();
        
        collection.AddTransient<DateConverter>();
        
        collection.AddTransient<IRepository<Announce>, AnonsRepository>();
        collection.AddTransient<AnnounceOperator>();
    }
}