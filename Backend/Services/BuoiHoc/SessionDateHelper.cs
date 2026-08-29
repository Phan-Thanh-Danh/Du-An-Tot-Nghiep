namespace Backend.Services.BuoiHoc;

public static class SessionDateHelper
{
    public static List<DateOnly> ExpandSessionDates(
        DateOnly startDate,
        DateOnly endDate,
        int vietnameseDayOfWeek)
    {
        var dates = new List<DateOnly>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            if (ToVietnameseDayOfWeek(date.DayOfWeek) == vietnameseDayOfWeek)
            {
                dates.Add(date);
            }
        }

        return dates;
    }

    public static int ToVietnameseDayOfWeek(DayOfWeek dayOfWeek)
    {
        return dayOfWeek == DayOfWeek.Sunday ? 1 : (int)dayOfWeek + 1;
    }
}