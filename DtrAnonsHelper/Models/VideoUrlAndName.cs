using System.Text.Json.Serialization;

namespace DtrAnonsHelper.Models;

public class VideoUrlAndName
{
    [JsonPropertyName("title")]
    public string VideoName { get; set; }
    [JsonPropertyName("player")]
    public string VideoUrl { get; set; }
}