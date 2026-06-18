using Backend.DTOs.BuoiHoc;

namespace Backend.Services.BuoiHoc;

public class SessionConflictException : Exception
{
    public SessionConflictResultDto Result { get; }

    public SessionConflictException(SessionConflictResultDto result)
        : base("Buổi học bị xung đột lịch.")
    {
        Result = result;
    }
}
