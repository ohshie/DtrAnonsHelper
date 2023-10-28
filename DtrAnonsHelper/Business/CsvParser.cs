using CsvHelper;
using CsvHelper.Configuration;
using DtrAnonsHelper.Models;

namespace DtrAnonsHelper.Business;

public class CsvParser
{
    public List<AnnounceCsv> ParseCsv(string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        };
        
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            var announceCsvs = csv.GetRecords<AnnounceCsv>().ToList();

            return announceCsvs;
        }
    }
}