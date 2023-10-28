using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace DtrAnonsHelper.BotClient;

public class BotHandleFile
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<BotHandleFile> _logger;

    public BotHandleFile(ITelegramBotClient botClient, ILogger<BotHandleFile> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task<string> Download(Message message)
    {
        var telegramFilePath = await GetTelegramFilePath(message);
        var localFilePath = LocalFilePath(message,telegramFilePath!);
        if (string.IsNullOrEmpty(localFilePath))
        {
            _logger.LogError("Something went wrong when saving local file from {ChatId}", message.From!.Id);
            return "";
        }

        using (Stream fileStream = File.Create(localFilePath))
        {
            await _botClient.DownloadFileAsync(telegramFilePath!, fileStream);
            _logger.LogInformation("Successfully downloaded file {FilePath} from {UserId}", 
                localFilePath, message.From!.Id);
        }

        return localFilePath;
    }

    public void Delete(string filePath)
    {
        if (!File.Exists(filePath)) return;

        try
        {
            File.Delete(filePath);
        }
        catch (Exception e)
        {
            _logger.LogError("Something went wrong deleting file {Path}. Info: {Exception}", filePath, e.Message);
            throw;
        }
       
    }

    private async Task<string?> GetTelegramFilePath(Message message)
    {
        if (!Directory.Exists($"./{message.From!.Id}"))
        {
            Directory.CreateDirectory($"./{message.From.Id}");
        }
        
        var fileId = message.Document!.FileId;
        var fileInfo = await _botClient.GetFileAsync(fileId);
        
        return fileInfo.FilePath;
    }

    private string LocalFilePath(Message message,string telegramFilePath)
    {
        int lastDot = telegramFilePath.LastIndexOf(".", StringComparison.Ordinal);
        if (lastDot != -1)
            return $"./{message.From!.Id}/" + telegramFilePath.Substring(lastDot - 1);

        return "";
    }
}