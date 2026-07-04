using Backend.DTOs.Registrations;

namespace Backend.Services.Registrations;

public interface IRegistrationService
{
    Task<IReadOnlyList<RegistrationPeriodDto>> GetPeriodsAsync(CancellationToken cancellationToken);
    Task<RegistrationPeriodDto> GetPeriodAsync(int id, CancellationToken cancellationToken);
    Task<RegistrationPeriodDto> CreatePeriodAsync(UpsertRegistrationPeriodRequest request, CancellationToken cancellationToken);
    Task<RegistrationPeriodDto> UpdatePeriodAsync(int id, UpsertRegistrationPeriodRequest request, CancellationToken cancellationToken);
    Task<RegistrationPeriodDto> OpenPeriodAsync(int id, CancellationToken cancellationToken);
    Task<RegistrationPeriodDto> ClosePeriodAsync(int id, CancellationToken cancellationToken);
    Task DeleteDraftPeriodAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<CourseSectionRegistrationDto>> GetCourseSectionsAsync(string? status, CancellationToken cancellationToken);
    Task<IReadOnlyList<CourseSectionRegistrationDto>> GetPeriodRegistrationsAsync(int periodId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminRegistrationResultDto>> GetRegistrationResultsAsync(CancellationToken cancellationToken);
    Task<CourseSectionRegistrationDto> UpdateCapacityAsync(int sectionId, UpdateCourseSectionCapacityRequest request, CancellationToken cancellationToken);
    Task<CourseSectionRegistrationDto> CancelSectionAsync(int sectionId, CourseSectionStatusRequest request, CancellationToken cancellationToken);
    Task<CourseSectionRegistrationDto> ReopenSectionAsync(int sectionId, CourseSectionStatusRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<CourseSectionRegistrationDto>> GetAvailableSectionsForStudentAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<StudentRegistrationDto>> GetStudentRegistrationsAsync(CancellationToken cancellationToken);
    Task<StudentRegistrationDto> EnrollAsync(StudentEnrollmentRequest request, CancellationToken cancellationToken);
    Task<StudentRegistrationDto> WithdrawAsync(int registrationId, CancellationToken cancellationToken);
}
