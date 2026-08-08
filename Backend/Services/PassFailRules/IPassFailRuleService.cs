using Backend.DTOs.PassFailRules;

namespace Backend.Services.PassFailRules;

public interface IPassFailRuleService
{
    Task<PassFailRuleListResponse> GetListAsync(
        int? maHocKy,
        string? search,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    Task<PassFailRuleDto?> GetAsync(int maCauHinhDiem, CancellationToken ct = default);

    Task<PassFailRuleDto> CreateAsync(UpsertPassFailRuleRequest request, int? currentUserId, CancellationToken ct = default);

    Task<PassFailRuleDto> UpdateAsync(int maCauHinhDiem, UpsertPassFailRuleRequest request, int? currentUserId, CancellationToken ct = default);
}
