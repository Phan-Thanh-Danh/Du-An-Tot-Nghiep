using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/parent-links")]
[Authorize(Roles = AuthRoles.Student)]
public class StudentParentLinksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StudentParentLinksController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<StudentParentLinkDto>>>> GetParentLinks(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var links = await _context.LienKetPhuHuynhs
            .AsNoTracking()
            .Include(l => l.PhuHuynh)
            .Where(l => l.MaHocSinh == userId && l.TrangThai != "da_thu_hoi")
            .ToListAsync(ct);

        var result = links.Select(l =>
        {
            Dictionary<string, bool> permissions = new()
            {
                ["grades"] = true,
                ["attendance"] = true,
                ["finance"] = true,
                ["schedule"] = true
            };

            if (!string.IsNullOrWhiteSpace(l.QuyenXem))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<Dictionary<string, bool>>(l.QuyenXem);
                    if (parsed != null)
                    {
                        permissions = parsed;
                    }
                }
                catch { }
            }

            return new StudentParentLinkDto
            {
                Id = l.MaLienKetPh.ToString(),
                Name = l.PhuHuynh?.HoTen ?? "Phụ huynh",
                Email = l.PhuHuynh?.Email ?? "",
                Phone = l.PhuHuynh?.SoDienThoai ?? "",
                Status = l.TrangThai == "hoat_dong" ? "Connected" : "Pending",
                Permissions = permissions
            };
        }).ToList();

        return Ok(ApiResponseDto<List<StudentParentLinkDto>>.Ok(result));
    }

    [HttpPost("invite")]
    public async Task<ActionResult<ApiResponseDto<StudentParentLinkDto>>> InviteParent(
        [FromBody] InviteParentRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(ApiResponseDto.Fail("Email phụ huynh không được để trống."));

        var parent = await _context.NguoiDungs
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower(), ct);

        if (parent == null)
        {
            // Tự động tạo tài khoản phụ huynh nếu chưa có
            parent = new NguoiDung
            {
                Email = request.Email.Trim().ToLowerInvariant(),
                HoTen = string.IsNullOrWhiteSpace(request.Name) ? "Phụ Huynh" : request.Name.Trim(),
                VaiTroChinh = AuthRoles.Parent,
                TrangThai = UserStatuses.DbActive,
                MaDonVi = 3,
                NgayTao = DateTime.UtcNow,
                DangNhapLanDau = true
            };
            _context.NguoiDungs.Add(parent);
            await _context.SaveChangesAsync(ct);
        }

        var existingLink = await _context.LienKetPhuHuynhs
            .FirstOrDefaultAsync(l => l.MaHocSinh == userId && l.MaPhuHuynh == parent.MaNguoiDung, ct);

        if (existingLink != null)
        {
            existingLink.TrangThai = "hoat_dong";
            existingLink.LienKetLuc = DateTime.UtcNow;
        }
        else
        {
            existingLink = new LienKetPhuHuynh
            {
                MaHocSinh = userId,
                MaPhuHuynh = parent.MaNguoiDung,
                QuyenXem = "{\"grades\":true,\"attendance\":true,\"finance\":true,\"schedule\":true}",
                TrangThai = "hoat_dong",
                LienKetLuc = DateTime.UtcNow
            };
            _context.LienKetPhuHuynhs.Add(existingLink);
        }

        await _context.SaveChangesAsync(ct);

        return Ok(ApiResponseDto<StudentParentLinkDto>.Ok(new StudentParentLinkDto
        {
            Id = existingLink.MaLienKetPh.ToString(),
            Name = parent.HoTen,
            Email = parent.Email,
            Phone = parent.SoDienThoai ?? "",
            Status = "Connected",
            Permissions = new Dictionary<string, bool>
            {
                ["grades"] = true,
                ["attendance"] = true,
                ["finance"] = true,
                ["schedule"] = true
            }
        }));
    }

    [HttpPut("{linkId}/permissions")]
    public async Task<ActionResult<ApiResponseDto<object>>> UpdatePermission(
        string linkId, [FromBody] UpdatePermissionRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (!int.TryParse(linkId, out var id))
            return BadRequest(ApiResponseDto.Fail("ID liên kết không hợp lệ."));

        var link = await _context.LienKetPhuHuynhs
            .FirstOrDefaultAsync(l => l.MaLienKetPh == id && l.MaHocSinh == userId, ct);

        if (link == null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy liên kết phụ huynh."));

        Dictionary<string, bool> permissions = new()
        {
            ["grades"] = true,
            ["attendance"] = true,
            ["finance"] = true,
            ["schedule"] = true
        };

        if (!string.IsNullOrWhiteSpace(link.QuyenXem))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<Dictionary<string, bool>>(link.QuyenXem);
                if (parsed != null) permissions = parsed;
            }
            catch { }
        }

        if (!string.IsNullOrWhiteSpace(request.Key))
        {
            permissions[request.Key] = request.Value;
        }

        link.QuyenXem = JsonSerializer.Serialize(permissions);
        await _context.SaveChangesAsync(ct);

        return Ok(ApiResponseDto<object>.Ok(new { Success = true, Permissions = permissions }));
    }

    [HttpDelete("{linkId}")]
    public async Task<ActionResult<ApiResponseDto<object>>> RemoveLink(string linkId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (!int.TryParse(linkId, out var id))
            return BadRequest(ApiResponseDto.Fail("ID liên kết không hợp lệ."));

        var link = await _context.LienKetPhuHuynhs
            .FirstOrDefaultAsync(l => l.MaLienKetPh == id && l.MaHocSinh == userId, ct);

        if (link != null)
        {
            _context.LienKetPhuHuynhs.Remove(link);
            await _context.SaveChangesAsync(ct);
        }

        return Ok(ApiResponseDto<object>.Ok(new { Success = true }));
    }

    private int GetCurrentUserId()
    {
        if (HttpContext.Items["CurrentUser"] is CurrentUserContext currentUser)
            return currentUser.UserId;
        throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
    }
}

public class StudentParentLinkDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public Dictionary<string, bool> Permissions { get; set; } = new();
}

public class InviteParentRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
}

public class UpdatePermissionRequest
{
    public string? Key { get; set; }
    public bool Value { get; set; }
}
