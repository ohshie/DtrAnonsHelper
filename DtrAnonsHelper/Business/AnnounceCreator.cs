using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DtrAnonsHelper.DataLayer;
using DtrAnonsHelper.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DtrAnonsHelper.Business;

public class AnnounceCreator
{
    private readonly CsvToDbParser _csvToDbParser;
    private readonly VideoUrlFetcher _videoUrlFetcher;
    private readonly ILogger<AnnounceCreator> _logger;
    private readonly AnnounceOperator _announceOperator;
    private readonly AnnouncePoster _announcePoster;

    public AnnounceCreator(CsvToDbParser csvToDbParser, 
        ILogger<AnnounceCreator> logger, VideoUrlFetcher videoUrlFetcher, AnnounceOperator announceOperator, AnnouncePoster announcePoster)
    {
        _csvToDbParser = csvToDbParser;
        _logger = logger;
        _videoUrlFetcher = videoUrlFetcher;
        _announceOperator = announceOperator;
        _announcePoster = announcePoster;
    }

    public async Task<string> CreateAnnounces(string filePath)
    {
        _logger.LogInformation("Attempting to parse provided CSV");
        await _csvToDbParser.Execute(filePath);
        
        _logger.LogInformation("Attempting to get Urls for Announces from corresponding VK groups");
        var (status, announces) = await _videoUrlFetcher.Execute();
        if (!string.IsNullOrEmpty(status)) return status;
        
        foreach (var channelAnnounces in announces)
        {
            _logger.LogInformation("Attempting to create announces on {Channel}", channelAnnounces.Key);
            
            var success = await _announcePoster.Create(channelAnnounces.Key, channelAnnounces);
            if (success)
            {
                status = string.Concat(status, $"\n Created announces on {channelAnnounces.Key}");
                
                _logger.LogInformation("Successfully created announces on {Channel}", channelAnnounces.Key);
            }
        }

        _logger.LogWarning("Removing all existing entities from db");
        await _announceOperator.RemoveAll();
        
        return status;
    }
}