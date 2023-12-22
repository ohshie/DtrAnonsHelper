using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DtrAnonsHelper.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DtrAnonsHelper.Business;

public class AnnouncePoster
{
    private readonly ILogger<AnnouncePoster> _logger;
    private readonly IConfiguration _configuration;

    public AnnouncePoster(ILogger<AnnouncePoster> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<Boolean> Create(string apiUrlDictKey, IEnumerable<Announce> announcesArray)
    {
        using (var client = new HttpClient())
        {
            var apiUrl = ApiUrls.ApiDictionary[apiUrlDictKey];
            
            var apiEndpoint = "promo";
            
            client.DefaultRequestHeaders.Add("Auth", $"Bearer {CreateHeader()}");

            var jsonData = JsonSerializer.Serialize(new { data = announcesArray.ToArray() });
            var payload = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{apiUrl}{apiEndpoint}", payload);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully created announces on {Site}", apiUrl);
                return true;
            }
            
            var errorString = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error! {StatusCode} {Error}", response.StatusCode, errorString);
            return false;
        }
    }
    
    string CreateHeader()
    {
        var auth = $"{DateTime.Now.ToString("dd")}{_configuration.GetSection("AnnounceKey").GetValue<string>("Secret")}";

        using (var md5 = MD5.Create())
        {
            var input = Encoding.UTF8.GetBytes(auth);
            var hash = md5.ComputeHash(input);

            var encodedHeader = Convert.ToHexString(hash);

            return encodedHeader.ToLowerInvariant();
        }
    }
}