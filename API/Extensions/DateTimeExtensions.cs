namespace API;

public static class DateTimeExtensions
{
    public static int CalculateAge(this DateOnly dateOnly) 
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var age = today.Year - dateOnly.Year;
        if (dateOnly.AddYears(age) > today) age--;
        return age;
    }
}
