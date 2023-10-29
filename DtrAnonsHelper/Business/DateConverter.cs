using NodaTime;

namespace DtrAnonsHelper.Business;

public class DateConverter
{
    private enum RuDates
    {
        Пн = 1,
        Вт = 2,
        Ср = 3,
        Чт = 4,
        Пт = 5,
        Сб = 6,
        Вс = 7,
        Понедельник  = 1,
        Вторник = 2,
        Среда = 3,
        Четверг = 4,
        Пятница = 5,
        Суббота = 6,
        Воскресенье = 7
    };
    
    public string Execute(string unformattedDate)
    {
        DateTime announceDate;
        
        if (unformattedDate.Contains(','))
        {
            var splittedOnComma = unformattedDate.Split(',');

            announceDate = splittedOnComma[0].Contains('-') ? WhenContainsHyphen(splittedOnComma[0]) : DayOfWeekToDate(splittedOnComma[0]);
        }
        else
        {
            announceDate = unformattedDate.Contains('-') ? WhenContainsHyphen(unformattedDate) : DayOfWeekToDate(unformattedDate);
        }
        
        return announceDate.ToString("dd.MM.yyyy");
    }

    private DateTime WhenContainsHyphen(string splittedOnComma)
    {
        var splittedOnHypen = splittedOnComma.Split('-');

        var announceDate = DayOfWeekToDate(splittedOnHypen[1]);
        
        return announceDate;
    }

    private DateTime DayOfWeekToDate(string dayOfTheWeek)
    {
        if (DateTime.TryParse(dayOfTheWeek, new CultureInfo("ru-RU"),
                DateTimeStyles.AdjustToUniversal, out var announceDate)) return announceDate;
        
        if (!Enum.TryParse(dayOfTheWeek, out RuDates parsedRuDate)) return GetDefaultAnnounceDate();
        
        announceDate =  GetMonday() + TimeSpan.FromDays(Convert.ToInt32(parsedRuDate) - 1);

        return announceDate;
    }

    /// <summary>
    /// Gets default announce date.
    /// Defaults to next sunday from the date of parsing.
    /// </summary>
    /// <returns>DateTime in form of dd.MM.yyyy containing next sunday date</returns>
    private DateTime GetDefaultAnnounceDate()
    {
        var nextMonday = GetMonday();

        return (nextMonday + TimeSpan.FromDays(6)).Date;
    }

    private DateTime GetMonday()
    {
        DateTime.TryParse(LocalDate.FromDateTime(DateTime.Today)
                .Next(IsoDayOfWeek.Monday)
                .ToDateOnly()
                .ToString(),
            new CultureInfo("ru-Ru"), 
            DateTimeStyles.AdjustToUniversal, 
            out var nextMonday);

        return nextMonday;
    }
}