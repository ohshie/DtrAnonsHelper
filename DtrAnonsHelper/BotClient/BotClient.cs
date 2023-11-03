using DtrAnonsHelper.Business;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DtrAnonsHelper.BotClient;

public class BotClient
{
    private readonly ITelegramBotClient _botClient;
    private readonly BotHandleFile _handleFile;
    private readonly AnnounceCreator _announceCreator;
    private readonly ILogger<BotClient> _logger;
    
    private static readonly ManualResetEvent ShutdownEvent = new ManualResetEvent(false);
    
    public BotClient(ITelegramBotClient botClient, 
        BotHandleFile handleFile,
        AnnounceCreator announceCreator,
        ILogger<BotClient> logger)
    {
        _botClient = botClient;
        _handleFile = handleFile;
        _announceCreator = announceCreator;
        _logger = logger;
    }

    public async Task BotOperations()
    {
        using CancellationTokenSource cts = new CancellationTokenSource();

        ReceiverOptions receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token);
        
        var me = await _botClient.GetMeAsync(cancellationToken: cts.Token);
        
        _logger.LogWarning("bot started @{Me}", me);
        
        ShutdownEvent.WaitOne();
        _logger.LogCritical("bot stopped");
        
        cts.Cancel();
    }
    
    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Handling update {Update} type {Type}", update.Id, update.Type);
        
            if (update.Message is not {} message) return;

            if (message.Document is null) return;

            if (!message.Document.FileName!.Contains(".csv")) return;

            var csvPath = await _handleFile.Download(message);
            
            if (string.IsNullOrEmpty(csvPath)) return;
            
            var creationStatus = await _announceCreator.CreateAnnounces(csvPath);
            if (!string.IsNullOrEmpty(creationStatus))
            {
                await _botClient.SendTextMessageAsync(chatId: message.From!.Id, 
                    text: creationStatus, 
                    cancellationToken: cancellationToken);
            }
            else
            {
                await _botClient.SendTextMessageAsync(chatId: message.From!.Id, 
                    text: "Failed to create announces", 
                    cancellationToken: cancellationToken);
            }
            
            _handleFile.Delete(csvPath); 
    }
    
    Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        try
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while processing update: {ex}");
            return Task.CompletedTask;
        }
    }
}