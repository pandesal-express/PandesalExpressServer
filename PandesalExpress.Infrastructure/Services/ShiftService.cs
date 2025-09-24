namespace PandesalExpress.Infrastructure.Services;

public enum ShiftType { AM, PM }

public interface IShiftService
{
    ShiftType GetCurrentShift(DateTime? forTime = null);
}

public class ShiftService : IShiftService
{
    private readonly TimeSpan _amShiftStart = new(5, 0, 0); // 5:00 AM
    private readonly TimeSpan _pmShiftStart = new(12, 0, 0); // 12:00 PM (14:00)

    public ShiftType GetCurrentShift(DateTime? forTime = null)
    {
        TimeSpan now = (forTime ?? DateTime.UtcNow).TimeOfDay;

        if (now >= _amShiftStart && now < _pmShiftStart) return ShiftType.AM;
        
        return ShiftType.PM;
    }
}
