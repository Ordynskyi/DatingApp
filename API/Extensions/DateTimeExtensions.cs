namespace API;

public static class DateTimeExtensions
{
    public static int CalculateAge(this DateOnly dateOnly) 
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var age  = today.AddDays(-dateOnly.Day);
        age = age.AddMonths(-dateOnly.Month);
        age = age.AddYears(-dateOnly.Year);
        return age.Year;
    }
}
