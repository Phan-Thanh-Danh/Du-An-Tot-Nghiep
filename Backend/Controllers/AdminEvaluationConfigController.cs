using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.Evaluations;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Applications;
using Backend.Services.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/evaluations")]
[Authorize(Policy = "AdminOnly")]
public class AdminEvaluationConfigController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IApplicationTemplateValidator _templateValidator;

    public AdminEvaluationConfigController(
        ApplicationDbContext db,
        IAuditLogService auditLogService,
        IHttpContextAccessor httpContextAccessor,
        IApplicationTemplateValidator templateValidator)
    {
        _db = db;
        _auditLogService = auditLogService;
        _httpContextAccessor = httpContextAccessor;
        _templateValidator = templateValidator;
    }

    [HttpGet("config")]
    public async Task<ActionResult<ApiResponseDto<EvaluationConfigDto?>>> GetConfig(CancellationToken cancellationToken)
    {
        var entity = await _db.MauDanhGias
            .AsNoTracking()
            .OrderBy(x => x.MaMauDanhGia)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return Ok(ApiResponseDto<EvaluationConfigDto?>.Ok(null, "Chưa có biểu mẫu đánh giá."));
        }

        return Ok(ApiResponseDto<EvaluationConfigDto?>.Ok(ToConfigDto(entity), "Lấy cấu hình đánh giá thành công."));
    }

    [HttpPut("config")]
    public async Task<ActionResult<ApiResponseDto<EvaluationConfigDto>>> UpsertConfig(
        [FromBody] UpdateEvaluationConfigRequest request,
        CancellationToken cancellationToken)
    {
        var tenMau = request.TenMau?.Trim() ?? string.Empty;
        if (tenMau.Length == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tên biểu mẫu không được rỗng.");
        }

        if (tenMau.Length > 200)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tên biểu mẫu không được vượt quá 200 ký tự.");
        }

        var json = request.CauHinhJson?.Trim() ?? string.Empty;
        _templateValidator.Validate(json);

        var entity = await _db.MauDanhGias
            .OrderBy(x => x.MaMauDanhGia)
            .FirstOrDefaultAsync(cancellationToken);

        var oldSnapshot = entity is null ? null : ToConfigDto(entity);

        if (entity is null)
        {
            entity = new MauDanhGia
            {
                TenMau = tenMau,
                CauHinhJson = json,
                DangHoatDong = request.DangHoatDong,
                NgayTao = DateTime.UtcNow,
                NgayCapNhat = DateTime.UtcNow
            };
            _db.MauDanhGias.Add(entity);
        }
        else
        {
            entity.TenMau = tenMau;
            entity.CauHinhJson = json;
            entity.DangHoatDong = request.DangHoatDong;
            entity.NgayCapNhat = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        var dto = ToConfigDto(entity);
        await LogAuditAsync(
            "UPSERT_EVALUATION_CONFIG",
            oldSnapshot,
            dto,
            $"Lưu cấu hình biểu mẫu đánh giá giảng viên '{dto.TenMau}'.",
            cancellationToken);

        return Ok(ApiResponseDto<EvaluationConfigDto>.Ok(dto, "Lưu cấu hình đánh giá thành công."));
    }

    private static EvaluationConfigDto ToConfigDto(MauDanhGia entity)
    {
        return new EvaluationConfigDto
        {
            MaMauDanhGia = entity.MaMauDanhGia,
            TenMau = entity.TenMau,
            CauHinhJson = entity.CauHinhJson,
            DangHoatDong = entity.DangHoatDong,
            NgayCapNhat = entity.NgayCapNhat
        };
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponseDto<EvaluationConfigSummaryDto>>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = new EvaluationConfigSummaryDto
        {
            TongCauHoi = await _db.CauHoiDanhGias.CountAsync(cancellationToken),
            CauHoiHoatDong = await _db.CauHoiDanhGias.CountAsync(x => x.ConHoatDong, cancellationToken),
            TongLuotDanhGia = await _db.DanhGiaGiaoViens.CountAsync(cancellationToken),
            SoGiaoVienDuocDanhGia = await _db.DanhGiaGiaoViens.Select(x => x.MaGiaoVien).Distinct().CountAsync(cancellationToken),
            SoHocKyCoDanhGia = await _db.DanhGiaGiaoViens.Select(x => x.MaHocKy).Distinct().CountAsync(cancellationToken)
        };

        return Ok(ApiResponseDto<EvaluationConfigSummaryDto>.Ok(summary, "Lấy tổng quan cấu hình đánh giá thành công."));
    }

    [HttpGet("questions")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<EvaluationQuestionDto>>>> GetQuestions(CancellationToken cancellationToken)
    {
        var questions = await _db.CauHoiDanhGias
            .AsNoTracking()
            .Select(x => new EvaluationQuestionDto
            {
                MaCauHoiDg = x.MaCauHoiDg,
                NoiDungCauHoi = x.NoiDungCauHoi,
                ConHoatDong = x.ConHoatDong,
                LuotSuDung = _db.DanhGiaGiaoViens.Count(g => g.MaCauHoiDg == x.MaCauHoiDg)
            })
            .OrderByDescending(x => x.ConHoatDong)
            .ThenBy(x => x.MaCauHoiDg)
            .ToListAsync(cancellationToken);

        return Ok(ApiResponseDto<IReadOnlyList<EvaluationQuestionDto>>.Ok(questions, "Lấy danh sách câu hỏi đánh giá thành công."));
    }

    [HttpPost("questions")]
    public async Task<ActionResult<ApiResponseDto<EvaluationQuestionDto>>> CreateQuestion(
        [FromBody] CreateEvaluationQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var content = ValidateContent(request.NoiDungCauHoi);

        var entity = new CauHoiDanhGia
        {
            NoiDungCauHoi = content,
            ConHoatDong = true
        };

        _db.CauHoiDanhGias.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = ToDto(entity, 0);
        await LogAuditAsync("CREATE_EVALUATION_QUESTION", null, dto, $"Tạo câu hỏi đánh giá: {dto.NoiDungCauHoi}", cancellationToken);

        return Ok(ApiResponseDto<EvaluationQuestionDto>.Ok(dto, "Tạo câu hỏi đánh giá thành công."));
    }

    [HttpPut("questions/{id:int}")]
    public async Task<ActionResult<ApiResponseDto<EvaluationQuestionDto>>> UpdateQuestion(
        int id,
        [FromBody] UpdateEvaluationQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await GetQuestionAsync(id, cancellationToken);
        var content = ValidateContent(request.NoiDungCauHoi);

        var oldSnapshot = ToDto(entity, 0);
        entity.NoiDungCauHoi = content;
        await _db.SaveChangesAsync(cancellationToken);

        var luotSuDung = await CountUsageAsync(id, cancellationToken);
        var dto = ToDto(entity, luotSuDung);
        await LogAuditAsync("UPDATE_EVALUATION_QUESTION", oldSnapshot, dto, $"Cập nhật câu hỏi đánh giá #{id}.", cancellationToken);

        return Ok(ApiResponseDto<EvaluationQuestionDto>.Ok(dto, "Cập nhật câu hỏi đánh giá thành công."));
    }

    [HttpPost("questions/{id:int}/toggle-active")]
    public async Task<ActionResult<ApiResponseDto<EvaluationQuestionDto>>> ToggleActive(
        int id,
        CancellationToken cancellationToken)
    {
        var entity = await GetQuestionAsync(id, cancellationToken);

        var oldSnapshot = ToDto(entity, 0);
        entity.ConHoatDong = !entity.ConHoatDong;
        await _db.SaveChangesAsync(cancellationToken);

        var luotSuDung = await CountUsageAsync(id, cancellationToken);
        var dto = ToDto(entity, luotSuDung);
        await LogAuditAsync(
            entity.ConHoatDong ? "ACTIVATE_EVALUATION_QUESTION" : "DEACTIVATE_EVALUATION_QUESTION",
            oldSnapshot,
            dto,
            $"{(entity.ConHoatDong ? "Kích hoạt" : "Tạm ẩn")} câu hỏi đánh giá #{id}.",
            cancellationToken);

        return Ok(ApiResponseDto<EvaluationQuestionDto>.Ok(dto, "Cập nhật trạng thái câu hỏi thành công."));
    }

    [HttpDelete("questions/{id:int}")]
    public async Task<ActionResult<ApiResponseDto<EvaluationQuestionDto>>> DeleteQuestion(
        int id,
        CancellationToken cancellationToken)
    {
        var entity = await GetQuestionAsync(id, cancellationToken);

        var luotSuDung = await CountUsageAsync(id, cancellationToken);
        if (luotSuDung > 0)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "Câu hỏi này đã được dùng trong lượt đánh giá, không thể xóa. Hãy tạm ẩn câu hỏi thay vì xóa.");
        }

        var dto = ToDto(entity, 0);
        _db.CauHoiDanhGias.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await LogAuditAsync("DELETE_EVALUATION_QUESTION", dto, null, $"Xóa câu hỏi đánh giá #{id}.", cancellationToken);

        return Ok(ApiResponseDto<EvaluationQuestionDto>.Ok(dto, "Xóa câu hỏi đánh giá thành công."));
    }

    private async Task<CauHoiDanhGia> GetQuestionAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _db.CauHoiDanhGias.FirstOrDefaultAsync(x => x.MaCauHoiDg == id, cancellationToken);
        if (entity is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy câu hỏi đánh giá.");
        }

        return entity;
    }

    private async Task<int> CountUsageAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.DanhGiaGiaoViens.CountAsync(x => x.MaCauHoiDg == id, cancellationToken);
    }

    private static string ValidateContent(string? content)
    {
        var trimmed = content?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Nội dung câu hỏi không được rỗng.");
        }

        if (trimmed.Length > 500)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Nội dung câu hỏi không được vượt quá 500 ký tự.");
        }

        return trimmed;
    }

    private static EvaluationQuestionDto ToDto(CauHoiDanhGia entity, int luotSuDung)
    {
        return new EvaluationQuestionDto
        {
            MaCauHoiDg = entity.MaCauHoiDg,
            NoiDungCauHoi = entity.NoiDungCauHoi,
            ConHoatDong = entity.ConHoatDong,
            LuotSuDung = luotSuDung
        };
    }

    private async Task LogAuditAsync(
        string action,
        object? oldValue,
        object? newValue,
        string description,
        CancellationToken cancellationToken)
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser is null)
        {
            return;
        }

        await _auditLogService.LogAsync(
            "CauHoiDanhGia",
            newValue is EvaluationQuestionDto dto ? dto.MaCauHoiDg.ToString() : oldValue is EvaluationQuestionDto oldDto ? oldDto.MaCauHoiDg.ToString() : string.Empty,
            action,
            oldValue,
            newValue,
            currentUser.UserId,
            currentUser.CampusId,
            description,
            cancellationToken);
    }
}
