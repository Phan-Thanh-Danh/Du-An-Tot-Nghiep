using Backend.DTOs;
using Backend.DTOs.Common;
using Backend.DTOs.StudentSchedule;

namespace Backend.Services.StudentSchedule;

public interface IStudentScheduleService
{
    Task<StudentScheduleSummaryDto> GetScheduleSummaryAsync(int studentId, int classId);
    Task<List<StudentScheduleItemDto>> GetTodayScheduleAsync(int studentId, int classId);
    Task<PagedResultDto<StudentScheduleItemDto>> GetScheduleAsync(int studentId, int classId, StudentScheduleQueryDto query);
    Task<List<StudentScheduleTermDto>> GetScheduleTermsAsync(int studentId, int classId);
}
