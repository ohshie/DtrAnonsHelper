using CsvHelper;
using CsvHelper.Configuration;
using DtrAnonsHelper.DataLayer;
using DtrAnonsHelper.Models;

namespace DtrAnonsHelper.Business;

public class CsvToDbParser
{
    private readonly DateConverter _dateConverter;
    private readonly AnnounceOperator _announceOperator;

    public CsvToDbParser(DateConverter dateConverter, AnnounceOperator announceOperator)
    {
        _dateConverter = dateConverter;
        _announceOperator = announceOperator;
    }

    public async Task Execute(string filePath)
    {
        var announceCSVs = ParseCsv(filePath);

        foreach (var site in ApiUrls.ApiDictionary)
        {
            if (announceCSVs.All(a => a.Channel != site.Key)) continue;
            
            var announcesByChannel = 
                announceCSVs.Where(a => a.Channel == site.Key).ToArray();

            List<Announce> mappedAnnounceList = new();
            
            foreach (var announce in announcesByChannel)
            {
                var mappedAnnounce = new Announce
                {
                    Title = announce.Name.Trim(),
                    StartDate = DateTime.Today.ToString("dd.MM.yyyy"),
                    EndDate = _dateConverter.Execute(announce.TextDate),
                    ScheduleDate = announce.TextDate,
                    Channel = announce.Channel
                };
                
                ChannelGroupSorter(mappedAnnounce);
                
                mappedAnnounceList.Add(mappedAnnounce);
            }

            await _announceOperator.SaveBatch(mappedAnnounceList);
        }
    }

    private void ChannelGroupSorter(Announce announce)
    {
        announce.ChannelGroup = announce.Channel switch
        {
            "Фан" => "Фан",
            "Мзк" => "Мзк",
            "Нст" => "Нст",
            _ => "Кино"
        };
    }
    
    private List<AnnounceCsv> ParseCsv(string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ","
        };
        
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            var announceCsvs = csv.GetRecords<AnnounceCsv>().ToList();

            return announceCsvs;
        }
    }
}