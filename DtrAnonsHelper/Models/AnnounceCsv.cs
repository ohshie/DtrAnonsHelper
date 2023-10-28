using CsvHelper.Configuration.Attributes;

namespace DtrAnonsHelper.Models;

public class AnnounceCsv
{
    [Name("Канал")]
    public string Channel { get; set; } = string.Empty;
    [Name("Название")]
    public string Name { get; set; } = string.Empty;
    [Name("Сокращенная дата")]
    public string TextDate { get; set; } = string.Empty;
    [Name("Дата в числах (не заполняй)")] 
    public string EndDate { get; set; } = string.Empty;
    [Name("vkUrl (не заполняй)")]
    public string Url { get; set; } = string.Empty;
}