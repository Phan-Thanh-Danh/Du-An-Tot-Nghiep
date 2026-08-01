using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = "Teacher")]
public class TeacherRequestsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TeacherRequestsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("requests")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetRequests()
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            // Đơn cần giáo viên xét duyệt: được gán NguoiDuyetHienTai = giáo viên này
            var pendingRequests = await _context.DonTus
                .Where(d => d.NguoiDuyetHienTai == userId
                    && d.TrangThai == "da_nop"
                    && d.TrangThaiXuLyNghiepVu == "cho_xu_ly")
                .Include(d => d.HocSinh)
                .OrderByDescending(d => d.NgayTao)
                .Select(d => new
                {
                    RequestId = d.MaDonTu,
                    Title = d.TieuDe,
                    Type = d.LoaiDon,
                    Status = d.TrangThai,
                    ProcessingStatus = d.TrangThaiXuLyNghiepVu,
                    CreatedAt = d.NgayTao,
                    UpdatedAt = d.NgayCapNhat,
                    SubmittedAt = d.NgayNop,
                    StudentId = d.MaHocSinh,
                    StudentName = d.HocSinh != null ? d.HocSinh.HoTen : "",
                    FormData = d.DuLieuBieuMau,
                    EvidenceUrl = d.UrlBangChung
                })
                .ToListAsync();

            return Ok(ApiResponseDto<object>.Ok(pendingRequests));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải đơn từ: " + ex.Message));
        }
    }

    [HttpPost("requests")]
    public async Task<ActionResult<ApiResponseDto<object>>> CreateRequest([FromBody] CreateRequestRequest request)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var donTu = new DonTu
            {
                MaDonVi = currentUser.CampusId,
                MaHocSinh = userId,
                TieuDe = request.Title,
                LoaiDon = request.LoaiDon,
                DuLieuBieuMau = request.NoiDung,
                TrangThai = "cho_duyet",
                TrangThaiXuLyNghiepVu = "cho_xu_ly",
                NgayTao = DateTime.UtcNow,
                NgayCapNhat = DateTime.UtcNow
            };

            _context.DonTus.Add(donTu);
            await _context.SaveChangesAsync();

            return Ok(ApiResponseDto<object>.Ok(new { RequestId = donTu.MaDonTu }, "Tạo đơn thành công."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tạo đơn: " + ex.Message));
        }
    }

    [HttpPost("requests/{id:int}/approve")]
    public async Task<ActionResult<ApiResponseDto<object>>> ApproveRequest(int id)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var don = await _context.DonTus.FirstOrDefaultAsync(d =>
                d.MaDonTu == id && d.NguoiDuyetHienTai == userId);

            if (don == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy đơn hoặc bạn không có quyền xử lý."));

            don.TrangThai = "da_duyet";
            don.TrangThaiXuLyNghiepVu = "xu_ly_thanh_cong";
            don.NguoiXuLyCuoi = userId;
            don.NgayDuyet = DateTime.UtcNow;
            don.NgayCapNhat = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(ApiResponseDto<object>.Ok(new { Message = "Đã phê duyệt đơn." }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi phê duyệt: " + ex.Message));
        }
    }

    [HttpPost("requests/{id:int}/reject")]
    public async Task<ActionResult<ApiResponseDto<object>>> RejectRequest(int id, [FromBody] RejectRequestDto body)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var don = await _context.DonTus.FirstOrDefaultAsync(d =>
                d.MaDonTu == id && d.NguoiDuyetHienTai == userId);

            if (don == null)
                return NotFound(ApiResponseDto.Fail("Không tìm thấy đơn hoặc bạn không có quyền xử lý."));

            don.TrangThai = "tu_choi";
            don.TrangThaiXuLyNghiepVu = "xu_ly_that_bai";
            don.NguoiXuLyCuoi = userId;
            don.LyDoTuChoi = body?.LyDo ?? "Không được chấp thuận.";
            don.NgayCapNhat = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(ApiResponseDto<object>.Ok(new { Message = "Đã từ chối đơn." }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi từ chối: " + ex.Message));
        }
    }

    [HttpGet("requests/history")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetRequestHistory()
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var history = await _context.DonTus
                .Where(d => d.MaHocSinh == userId
                    && (d.TrangThai == "da_duyet" || d.TrangThai == "da_huy" || d.TrangThai == "tu_choi"))
                .OrderByDescending(d => d.NgayCapNhat)
                .Select(d => new
                {
                    RequestId = d.MaDonTu,
                    Title = d.TieuDe,
                    Type = d.LoaiDon,
                    Status = d.TrangThai,
                    CreatedAt = d.NgayTao,
                    UpdatedAt = d.NgayCapNhat
                })
                .ToListAsync();

            return Ok(ApiResponseDto<object>.Ok(history));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải lịch sử đơn từ: " + ex.Message));
        }
    }
}

public class CreateRequestRequest
{
    public string Title { get; set; } = string.Empty;
    public string LoaiDon { get; set; } = string.Empty;
    public string NoiDung { get; set; } = string.Empty;
}

public class RejectRequestDto
{
    public string? LyDo { get; set; }
}
