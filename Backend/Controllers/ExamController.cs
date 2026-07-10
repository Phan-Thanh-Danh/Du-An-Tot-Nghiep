using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.Exam;
using Backend.DTOs.QuizAttempts;
using Backend.Services.Exam;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/exam")]
[Authorize]
public class ExamController : ControllerBase
{
    private readonly IExamService _examService;

    public ExamController(IExamService examService)
    {
        _examService = examService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = HttpContext.Items["CurrentUser"];
        if (userIdClaim is Backend.Models.NguoiDung user)
            return user.MaNguoiDung;

        var claim = User.FindFirst(CustomClaimTypes.UserId);
        return claim != null ? int.Parse(claim.Value) : 0;
    }

    // ===== KyThi =====

    [HttpGet("ky-thi")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<KyThiDto>>>> GetKyThis(
        [FromQuery] ExamQueryParameters parameters, CancellationToken ct)
    {
        var result = await _examService.GetKyThisAsync(parameters, ct);
        return Ok(ApiResponseDto<PagedResultDto<KyThiDto>>.Ok(result));
    }

    [HttpGet("ky-thi/{id:int}")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<ActionResult<ApiResponseDto<KyThiDto>>> GetKyThiById(int id, CancellationToken ct)
    {
        var result = await _examService.GetKyThiByIdAsync(id, ct);
        return Ok(ApiResponseDto<KyThiDto>.Ok(result));
    }

    [HttpPost("ky-thi")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<ActionResult<ApiResponseDto<KyThiDto>>> CreateKyThi(
        CreateKyThiRequest request, CancellationToken ct)
    {
        var result = await _examService.CreateKyThiAsync(request, ct);
        return CreatedAtAction(nameof(GetKyThiById), new { id = result.MaKyThi },
            ApiResponseDto<KyThiDto>.Ok(result, "Tạo kỳ thi thành công"));
    }

    [HttpPut("ky-thi/{id:int}")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<ActionResult<ApiResponseDto<KyThiDto>>> UpdateKyThi(
        int id, UpdateKyThiRequest request, CancellationToken ct)
    {
        var result = await _examService.UpdateKyThiAsync(id, request, ct);
        return Ok(ApiResponseDto<KyThiDto>.Ok(result, "Cập nhật kỳ thi thành công"));
    }

    // ===== LichThiTong =====

    [HttpGet("lich-thi-tong")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<LichThiTongDto>>>> GetLichThiTongs(
        [FromQuery] ExamQueryParameters parameters, CancellationToken ct)
    {
        var result = await _examService.GetLichThiTongsAsync(parameters, ct);
        return Ok(ApiResponseDto<PagedResultDto<LichThiTongDto>>.Ok(result));
    }

    [HttpGet("lich-thi-tong/{id:int}")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<ActionResult<ApiResponseDto<LichThiTongDto>>> GetLichThiTongById(int id, CancellationToken ct)
    {
        var result = await _examService.GetLichThiTongByIdAsync(id, ct);
        return Ok(ApiResponseDto<LichThiTongDto>.Ok(result));
    }

    [HttpPost("lich-thi-tong")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<ActionResult<ApiResponseDto<LichThiTongDto>>> CreateLichThiTong(
        CreateLichThiTongRequest request, CancellationToken ct)
    {
        var result = await _examService.CreateLichThiTongAsync(request, ct);
        return CreatedAtAction(nameof(GetLichThiTongById), new { id = result.MaLichThiTong },
            ApiResponseDto<LichThiTongDto>.Ok(result, "Tạo lịch thi tổng thành công"));
    }

    [HttpPut("lich-thi-tong/{id:int}")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<ActionResult<ApiResponseDto<LichThiTongDto>>> UpdateLichThiTong(
        int id, UpdateLichThiTongRequest request, CancellationToken ct)
    {
        var result = await _examService.UpdateLichThiTongAsync(id, request, ct);
        return Ok(ApiResponseDto<LichThiTongDto>.Ok(result, "Cập nhật lịch thi tổng thành công"));
    }

    [HttpPost("lich-thi-tong/{id:int}/send-to-co-so")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<ActionResult<ApiResponseDto>> SendToCoSo(int id, CancellationToken ct)
    {
        await _examService.SendToCoSoAsync(id, ct);
        return Ok(ApiResponseDto.Ok("Gửi lịch thi về cơ sở thành công"));
    }

    // ===== CaThi =====

    [HttpGet("ca-thi")]
    [Authorize(Roles = $"{AuthRoles.Teacher},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Admin},{AuthRoles.SuperAdmin}")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<CaThiDto>>>> GetCaThis(
        [FromQuery] CaThiQueryParameters parameters, CancellationToken ct)
    {
        var result = await _examService.GetCaThisAsync(parameters, ct);
        return Ok(ApiResponseDto<PagedResultDto<CaThiDto>>.Ok(result));
    }

    [HttpGet("ca-thi/{id:int}")]
    [Authorize(Roles = $"{AuthRoles.Teacher},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Admin},{AuthRoles.SuperAdmin}")]
    public async Task<ActionResult<ApiResponseDto<CaThiDto>>> GetCaThiById(int id, CancellationToken ct)
    {
        var result = await _examService.GetCaThiByIdAsync(id, ct);
        return Ok(ApiResponseDto<CaThiDto>.Ok(result));
    }

    [HttpPost("ca-thi")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<ActionResult<ApiResponseDto<CaThiDto>>> CreateCaThi(
        CreateCaThiRequest request, CancellationToken ct)
    {
        var result = await _examService.CreateCaThiAsync(request, ct);
        return CreatedAtAction(nameof(GetCaThiById), new { id = result.MaCaThi },
            ApiResponseDto<CaThiDto>.Ok(result, "Tạo ca thi thành công"));
    }

    [HttpPut("ca-thi/{id:int}")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<ActionResult<ApiResponseDto<CaThiDto>>> UpdateCaThi(
        int id, UpdateCaThiRequest request, CancellationToken ct)
    {
        var result = await _examService.UpdateCaThiAsync(id, request, ct);
        return Ok(ApiResponseDto<CaThiDto>.Ok(result, "Cập nhật ca thi thành công"));
    }

    // ===== PhanCongGiamThi =====

    [HttpGet("ca-thi/{maCaThi:int}/giam-thi")]
    [Authorize(Roles = $"{AuthRoles.Teacher},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Admin},{AuthRoles.SuperAdmin}")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<PhanCongGiamThiDto>>>> GetGiamThis(
        int maCaThi, CancellationToken ct)
    {
        var result = await _examService.GetGiamThisByCaThiAsync(maCaThi, ct);
        return Ok(ApiResponseDto<IReadOnlyList<PhanCongGiamThiDto>>.Ok(result));
    }

    [HttpPost("ca-thi/giam-thi")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<ActionResult<ApiResponseDto<PhanCongGiamThiDto>>> AssignGiamThi(
        CreatePhanCongGiamThiRequest request, CancellationToken ct)
    {
        var result = await _examService.AssignGiamThiAsync(request, ct);
        return Ok(ApiResponseDto<PhanCongGiamThiDto>.Ok(result, "Phân công giám thị thành công"));
    }

    [HttpDelete("ca-thi/giam-thi/{maPhanCong:int}")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<ActionResult<ApiResponseDto>> RemoveGiamThi(int maPhanCong, CancellationToken ct)
    {
        await _examService.RemoveGiamThiAsync(maPhanCong, ct);
        return Ok(ApiResponseDto.Ok("Hủy phân công giám thị thành công"));
    }

    // ===== ThiSinhCaThi =====

    [HttpGet("ca-thi/{maCaThi:int}/thi-sinh")]
    [Authorize(Roles = $"{AuthRoles.Teacher},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Admin},{AuthRoles.SuperAdmin}")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<ThiSinhCaThiDto>>>> GetThiSinhs(
        int maCaThi, CancellationToken ct)
    {
        var result = await _examService.GetThiSinhsByCaThiAsync(maCaThi, ct);
        return Ok(ApiResponseDto<IReadOnlyList<ThiSinhCaThiDto>>.Ok(result));
    }

    [HttpPost("ca-thi/thi-sinh")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<ThiSinhCaThiDto>>>> AddThiSinhs(
        AddThiSinhCaThiRequest request, CancellationToken ct)
    {
        var result = await _examService.AddThiSinhsToCaThiAsync(request, ct);
        return Ok(ApiResponseDto<IReadOnlyList<ThiSinhCaThiDto>>.Ok(result, "Thêm thí sinh thành công"));
    }

    // ===== DiemDanhThi =====

    [HttpGet("ca-thi/{maCaThi:int}/diem-danh")]
    [Authorize(Roles = $"{AuthRoles.Teacher},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Admin},{AuthRoles.SuperAdmin}")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<DiemDanhThiDto>>>> GetDiemDanh(
        int maCaThi, CancellationToken ct)
    {
        var result = await _examService.GetDiemDanhByCaThiAsync(maCaThi, ct);
        return Ok(ApiResponseDto<IReadOnlyList<DiemDanhThiDto>>.Ok(result));
    }

    [HttpPost("ca-thi/diem-danh")]
    [Authorize(Roles = $"{AuthRoles.Teacher},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Admin},{AuthRoles.SuperAdmin}")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<DiemDanhThiDto>>>> BatchDiemDanh(
        BatchDiemDanhRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _examService.BatchDiemDanhAsync(request, userId, ct);
        return Ok(ApiResponseDto<IReadOnlyList<DiemDanhThiDto>>.Ok(result, "Điểm danh thành công"));
    }

    [HttpPost("ca-thi/{id:int}/start")]
    [Authorize(Roles = $"{AuthRoles.Teacher},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Admin},{AuthRoles.SuperAdmin}")]
    public async Task<ActionResult<ApiResponseDto>> StartCaThi(int id, CancellationToken ct)
    {
        await _examService.StartCaThiAsync(id, ct);
        return Ok(ApiResponseDto.Ok("Bắt đầu ca thi thành công"));
    }

    // ===== NhatKyViPhamThi =====

    [HttpGet("ca-thi/{maCaThi:int}/vi-pham")]
    [Authorize(Roles = $"{AuthRoles.Teacher},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Admin},{AuthRoles.SuperAdmin}")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<NhatKyViPhamThiDto>>>> GetViPhams(
        int maCaThi, CancellationToken ct)
    {
        var result = await _examService.GetViPhamsByCaThiAsync(maCaThi, ct);
        return Ok(ApiResponseDto<IReadOnlyList<NhatKyViPhamThiDto>>.Ok(result));
    }

    [HttpPost("vi-pham")]
    [Authorize(Roles = $"{AuthRoles.Teacher},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Admin},{AuthRoles.SuperAdmin}")]
    public async Task<ActionResult<ApiResponseDto<NhatKyViPhamThiDto>>> CreateViPham(
        CreateNhatKyViPhamRequest request, CancellationToken ct)
    {
        var result = await _examService.CreateViPhamAsync(request, ct);
        return Ok(ApiResponseDto<NhatKyViPhamThiDto>.Ok(result, "Ghi nhận vi phạm thành công"));
    }

    // ===== XuLyViPhamThi =====

    [HttpPost("vi-pham/xu-ly")]
    [Authorize(Roles = $"{AuthRoles.Teacher},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Admin},{AuthRoles.SuperAdmin}")]
    public async Task<ActionResult<ApiResponseDto<XuLyViPhamThiDto>>> XuLyViPham(
        CreateXuLyViPhamRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _examService.XuLyViPhamAsync(request, userId, ct);
        return Ok(ApiResponseDto<XuLyViPhamThiDto>.Ok(result, "Xử lý vi phạm thành công"));
    }

    // ===== BienBanThi =====

    [HttpGet("ca-thi/{maCaThi:int}/bien-ban")]
    [Authorize(Roles = $"{AuthRoles.Teacher},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Admin},{AuthRoles.SuperAdmin}")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<BienBanThiDto>>>> GetBienBans(
        int maCaThi, CancellationToken ct)
    {
        var result = await _examService.GetBienBansByCaThiAsync(maCaThi, ct);
        return Ok(ApiResponseDto<IReadOnlyList<BienBanThiDto>>.Ok(result));
    }

    [HttpPost("bien-ban")]
    [Authorize(Roles = $"{AuthRoles.Teacher},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Admin},{AuthRoles.SuperAdmin}")]
    public async Task<ActionResult<ApiResponseDto<BienBanThiDto>>> CreateBienBan(
        CreateBienBanThiRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _examService.CreateBienBanAsync(request, userId, ct);
        return Ok(ApiResponseDto<BienBanThiDto>.Ok(result, "Lập biên bản thành công"));
    }

    // ===== Signature =====

    [HttpPost("signature/confirm")]
    [Authorize(Roles = $"{AuthRoles.Teacher},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Admin},{AuthRoles.SuperAdmin}")]
    public async Task<ActionResult<ApiResponseDto<PhienThiDto>>> ConfirmSignature(
        ConfirmSignatureRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _examService.ConfirmSignatureAsync(request, userId, ct);
        return Ok(ApiResponseDto<PhienThiDto>.Ok(result, "Xác nhận ký tên thành công"));
    }

    [HttpPost("signature/report-missing")]
    [Authorize(Roles = $"{AuthRoles.Teacher},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Admin},{AuthRoles.SuperAdmin}")]
    public async Task<ActionResult<ApiResponseDto<PhienThiDto>>> ReportMissingSignature(
        ReportMissingSignatureRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _examService.ReportMissingSignatureAsync(request, userId, ct);
        return Ok(ApiResponseDto<PhienThiDto>.Ok(result, "Báo cáo quên ký tên thành công"));
    }

    [HttpGet("signature/reports/missing")]
    [Authorize(Roles = $"{AuthRoles.Teacher},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Admin},{AuthRoles.SuperAdmin}")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<PhienThiDto>>>> GetMissingSignatureReports(
        [FromQuery] int? maCaThi, CancellationToken ct)
    {
        var result = await _examService.GetMissingSignatureSessionsAsync(maCaThi, ct);
        return Ok(ApiResponseDto<IReadOnlyList<PhienThiDto>>.Ok(result));
    }

    [HttpGet("signature/reports/signed")]
    [Authorize(Roles = $"{AuthRoles.Teacher},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Admin},{AuthRoles.SuperAdmin}")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<PhienThiDto>>>> GetSignedReports(
        [FromQuery] int? maCaThi, CancellationToken ct)
    {
        var result = await _examService.GetSignedSessionsAsync(maCaThi, ct);
        return Ok(ApiResponseDto<IReadOnlyList<PhienThiDto>>.Ok(result));
    }

    // ===== Exam Taking (Student) =====

    [HttpGet("student/list")]
    [Authorize(Roles = AuthRoles.Student)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<StudentExamListItemDto>>>> GetStudentExams(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _examService.GetStudentExamsAsync(userId, ct);
        return Ok(ApiResponseDto<IReadOnlyList<StudentExamListItemDto>>.Ok(result));
    }

    [HttpGet("taking/session/{maPhienThi:int}")]
    [Authorize(Roles = AuthRoles.Student)]
    public async Task<ActionResult<ApiResponseDto<PhienThiDto>>> GetExamSession(
        int maPhienThi, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _examService.GetExamSessionAsync(maPhienThi, userId, ct);
        return Ok(ApiResponseDto<PhienThiDto>.Ok(result));
    }

    [HttpGet("taking/session/{maPhienThi:int}/questions")]
    [Authorize(Roles = AuthRoles.Student)]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<QuizAttemptQuestionDto>>>> GetExamQuestions(
        int maPhienThi, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _examService.GetExamQuestionsAsync(maPhienThi, userId, ct);
        return Ok(ApiResponseDto<IReadOnlyList<QuizAttemptQuestionDto>>.Ok(result));
    }

    [HttpPost("taking/start")]
    [Authorize(Roles = AuthRoles.Student)]
    public async Task<ActionResult<ApiResponseDto<PhienThiDto>>> StartExam(
        StartExamRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _examService.StartExamAsync(request, userId, ct);
        return Ok(ApiResponseDto<PhienThiDto>.Ok(result, "Bắt đầu thi thành công"));
    }

    [HttpPost("taking/autosave")]
    [Authorize(Roles = AuthRoles.Student)]
    public async Task<ActionResult<ApiResponseDto>> AutoSave(
        AutoSaveAnswerRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _examService.AutoSaveAnswerAsync(request, userId, ct);
        return Ok(ApiResponseDto.Ok("Lưu tạm thành công"));
    }

    [HttpPost("taking/submit")]
    [Authorize(Roles = AuthRoles.Student)]
    public async Task<ActionResult<ApiResponseDto<PhienThiDto>>> SubmitExam(
        SubmitExamRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _examService.SubmitExamAsync(request, userId, ct);
        return Ok(ApiResponseDto<PhienThiDto>.Ok(result, "Nộp bài thi thành công"));
    }

    [HttpGet("student/result/{sessionId:int}")]
    [Authorize(Roles = AuthRoles.Student)]
    public async Task<ActionResult<ApiResponseDto<object>>> GetStudentExamResult(int sessionId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _examService.GetStudentExamResultAsync(sessionId, userId, ct);
        return Ok(ApiResponseDto<object>.Ok(result));
    }

    // ===== Grading =====

    [HttpPost("grading/auto/{maCaThi:int}")]
    [Authorize(Roles = $"{AuthRoles.Teacher},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Admin},{AuthRoles.SuperAdmin}")]
    public async Task<ActionResult<ApiResponseDto>> FinalizeAutoGrade(int maCaThi, CancellationToken ct)
    {
        await _examService.FinalizeAutoGradeAsync(maCaThi, ct);
        return Ok(ApiResponseDto.Ok("Chấm tự động hoàn tất"));
    }

    [HttpPost("grading/essay")]
    [Authorize(Roles = $"{AuthRoles.Teacher},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Admin},{AuthRoles.SuperAdmin}")]
    public async Task<ActionResult<ApiResponseDto<PhienThiDto>>> GradeEssay(
        GradeEssayRequest request, CancellationToken ct)
    {
        var result = await _examService.GradeEssayAsync(request, ct);
        return Ok(ApiResponseDto<PhienThiDto>.Ok(result, "Chấm bài tự luận thành công"));
    }

    [HttpPost("grading/publish")]
    [Authorize(Policy = "AcademicOperations")]
    public async Task<ActionResult<ApiResponseDto>> PublishScores(
        PublishScoresRequest request, CancellationToken ct)
    {
        await _examService.PublishScoresAsync(request, ct);
        return Ok(ApiResponseDto.Ok("Công bố điểm thành công"));
    }

    // ===== Reports =====

    [HttpGet("reports/summary")]
    [Authorize(Policy = "Reports")]
    public async Task<ActionResult<ApiResponseDto<ExamReportSummaryDto>>> GetReportSummary(
        [FromQuery] int? maKyThi, [FromQuery] int? maDonVi, CancellationToken ct)
    {
        var result = await _examService.GetReportSummaryAsync(maKyThi, maDonVi, ct);
        return Ok(ApiResponseDto<ExamReportSummaryDto>.Ok(result));
    }

    [HttpGet("de-kiem-tra")]
    [Authorize(Roles = $"{AuthRoles.Teacher},{AuthRoles.CampusAdmin},{AuthRoles.AcademicStaff},{AuthRoles.Admin},{AuthRoles.SuperAdmin},{AuthRoles.HoiDongQuanLyNoiDung}")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<DeKiemTraDto>>>> GetDeKiemTras(CancellationToken ct)
    {
        var result = await _examService.GetDeKiemTrasAsync(ct);
        return Ok(ApiResponseDto<IReadOnlyList<DeKiemTraDto>>.Ok(result));
    }
}
