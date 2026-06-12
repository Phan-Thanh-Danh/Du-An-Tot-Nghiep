using Backend.DTOs.ThoiKhoaBieu;

namespace Backend.Services.ThoiKhoaBieu;

public class ScheduleConflictException : Exception
{
    public ScheduleConflictResultDto Result { get; }

    public ScheduleConflictException(ScheduleConflictResultDto result)
        : base("Thời khóa biểu bị xung đột.")
    {
        Result = result;
    }
}
