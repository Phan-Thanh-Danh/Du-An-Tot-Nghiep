using Backend.DTOs.TeachingPreferences;

namespace Backend.Services.TeachingPreferences;

public interface ITeachingPreferenceService
{
    Task<TeachingPreferenceContextDto> GetContextAsync(int teacherId);
    Task<TeachingPreferenceFormDto> GetTeacherFormAsync(int teacherId, int maHocKy);
    Task<TeachingPreferenceFormDto> SaveDraftAsync(int teacherId, int maHocKy, UpdateTeachingPreferenceDto dto);
    Task<TeachingPreferenceFormDto> SubmitAsync(int teacherId, int maHocKy, SubmitTeachingPreferenceDto dto);
    
    // Staff operations
    Task<StaffTeachingPreferenceSummaryDto> GetSummaryAsync(int staffDonViId, int maHocKy);
    Task<List<StaffTeacherPreferenceDetailDto>> GetTeachersSummaryAsync(int staffDonViId, int maHocKy);
}
