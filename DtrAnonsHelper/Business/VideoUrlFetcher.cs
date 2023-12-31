using System.Text.Json;
using DtrAnonsHelper.DataLayer;
using DtrAnonsHelper.Models;

namespace DtrAnonsHelper.Business;

public class VideoUrlFetcher
{
    private readonly AnnounceOperator _announceOperator;

    public VideoUrlFetcher(AnnounceOperator announceOperator)
    {
        _announceOperator = announceOperator;
    }

    public async Task<(string, IEnumerable<IGrouping<string,Announce>>)> Execute(string tokenUrl)
    {
        var report = string.Empty;

        var apiToken = TokenExtractor(tokenUrl);
        
        var groupedAnnounces = (await _announceOperator.GetAll())
            .GroupBy(a => a.ChannelGroup);

        foreach (var channelGroup in groupedAnnounces)
        {
            var videoUrlAndNames = await GetUrls(channelGroup.Key, 
                channelGroup.Count(), apiToken);
            
            if (!videoUrlAndNames.Any())
            {
                report = string.Concat(report, "\n Something went wrong processing Url's from VK");
                continue;
            }
            
            foreach (var announce in channelGroup)
            {
                var videoUrlAndName = videoUrlAndNames.Find(a => a.VideoName == announce.Title);
                
                if (videoUrlAndName is null)
                {
                    report = string.Concat(report, "\n" +
                                                   $"{announce.Title} not found in VK");
                    continue;
                }
                
                announce.Url = videoUrlAndName.VideoUrl.Split("&hash")[0];
            }

            await _announceOperator.UpdateBatch(channelGroup);
        }

        return (report, groupedAnnounces);
    }

    private async Task<List<VideoUrlAndName>?> GetUrls(string channelGroup, 
        int amountOfAnnounceInGroup, string apiToken)
    {
        var ownerId = VkGroupIdDictionary.ChannelGroupIdMap[channelGroup];
        
        string requestUri = $"https://api.vk.com/method/video.get?" +
                            $"owner_id=-{ownerId}&" +
                            $"count={amountOfAnnounceInGroup}&" +
                            $"access_token={apiToken}&" +
                            $"v=5.131";

        using (HttpClient client = new())
        {
            var response = await client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            using (JsonDocument doc = JsonDocument.Parse(responseBody))
            {
                var root = doc.RootElement;
                var videoInfosJson = root.GetProperty("response").GetProperty("items");

                var videoInfos = JsonSerializer.Deserialize<List<VideoUrlAndName>>(videoInfosJson.GetRawText());

                return videoInfos;
            }
        }
    }

    private string TokenExtractor(string url)
    {
        var extractedToken = url.Split("token=")
            .Last()
            .Split("&expires")
            .First();

        return extractedToken;
    }
}