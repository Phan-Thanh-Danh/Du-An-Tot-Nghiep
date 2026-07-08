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

            var requests = await _context.DonTus
                .Where(d => d.MaHocSinh == userId)
                .OrderByDescending(d => d.NgayTao)
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

            return Ok(ApiResponseDto<object>.Ok(requests));
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
