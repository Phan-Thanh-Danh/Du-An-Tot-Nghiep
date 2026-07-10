using Backend.DTOs.Common;
using Backend.DTOs.TeacherSchedule;

namespace Backend.Services.TeacherSchedule;

public interface ITeacherScheduleService
{
    Task<TeacherScheduleSummaryDto> GetSummaryAsync(int teacherId);
    Task<List<TeacherScheduleItemDto>> GetTodayScheduleAsync(int teacherId);
    Task<PagedResultDto<TeacherScheduleItemDto>> GetScheduleAsync(int teacherId, TeacherScheduleQueryDto query);
    Task<List<TeacherScheduleTermDto>> GetTermsAsync(int teacherId);
}
