using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DtrAnonsHelper.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DtrAnonsHelper.Business;

public class AnnounceCreator
{
    private readonly CsvParser _parser;
    private readonly List<Announce> _mappedAnnounceList = new();
    private readonly ILogger<AnnounceCreator> _logger;
    private readonly IConfiguration _configuration;

    public AnnounceCreator(CsvParser parser, 
        ILogger<AnnounceCreator> logger, 
        IConfiguration configuration)
    {
        _parser = parser;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task CreateAnnounces(string filePath)
    {
        var parsedAnnounces = _parser.ParseCsv(filePath);
        
        foreach (var site in ApiUrls.ApiDictionary)
        {
            if (parsedAnnounces.All(a => a.Channel != site.Key)) continue;
   
            MappedAnnounceList(parsedAnnounces, site);

            await Create(site.Value, _mappedAnnounceList.ToArray());
        }
    }

    private void MappedAnnounceList(List<AnnounceCsv> parsedAnnounces, KeyValuePair<string, string> site)
    {
        _mappedAnnounceList.Clear();
        
        var announcesByChannel = 
            parsedAnnounces.Where(a => a.Channel == site.Key).ToArray();
        
        foreach (var announce in announcesByChannel)
        {
            var mappedAnnounce = new Announce
            {
                Title = announce.Name,
                StartDate = DateTime.Today.ToString("dd.MM.yyyy"),
                EndDate = announce.EndDate,
                ScheduleDate = announce.TextDate,
                Url = !string.IsNullOrEmpty(announce.Url) ? announce.Url.Split("\"")[1] : ""
            };

            _mappedAnnounceList.Add(mappedAnnounce);
        }
    }
    
    async Task Create(string apiUrl, Announce[] announcesArray)
    {
        using (var client = new HttpClient())
        {
            var apiEndpoint = "promo";
            
            client.DefaultRequestHeaders.Add("Auth", $"Bearer {CreateHeader()}");

            var jsonData = JsonSerializer.Serialize(new { data = announcesArray });
            var payload = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{apiUrl}{apiEndpoint}", payload);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully created announces on {Site}", apiUrl);
                return;
            }
            
            var errorString = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error! {StatusCode} {Error}", response.StatusCode, errorString);
        }
    }
    string CreateHeader()
    { 
        var auth = $"{DateTime.Now.Day}{_configuration.GetSection("AnnounceKey").GetValue<string>("Secret")}";

        using (var md5 = MD5.Create())
        {
            var input = Encoding.UTF8.GetBytes(auth);
            var hash = md5.ComputeHash(input);

            var encodedHeader = Convert.ToHexString(hash);

            return encodedHeader.ToLowerInvariant();
        }
    }
}