using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Common;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/staff/rooms")]
[Authorize(Roles = AuthRoles.AcademicStaff)]
public class StaffRoomBookingsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public StaffRoomBookingsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("bookings")]
    public async Task<ActionResult<ApiResponseDto<List<RoomBookingDto>>>> GetBookings(
        [FromQuery] int? roomId,
        [FromQuery] DateTime? date,
        [FromQuery] string? status)
    {
        var query = _db.DatPhongs
            .Include(d => d.Phong)
            .Include(d => d.NguoiYeuCauNavigation)
            .AsQueryable();

        if (roomId.HasValue)
            query = query.Where(d => d.MaPhong == roomId.Value);

        if (date.HasValue)
        {
            var dayStart = date.Value.Date;
            var dayEnd = dayStart.AddDays(1);
            query = query.Where(d => d.BatDauLuc >= dayStart && d.BatDauLuc < dayEnd);
        }

        if (!string.IsNullOrEmpty(status))
            query = query.Where(d => d.TrangThai == status);

        var bookings = await query
            .OrderByDescending(d => d.NgayTao)
            .Select(d => new RoomBookingDto
            {
                Id = d.MaDatPhong,
                RoomId = d.MaPhong,
                RoomName = d.Phong != null ? d.Phong.TenPhong : null,
                RoomCode = d.Phong != null ? d.Phong.MaCodePhong : null,
                RequesterId = d.NguoiYeuCau,
                RequesterName = d.NguoiYeuCauNavigation != null ? d.NguoiYeuCauNavigation.HoTen : null,
                Purpose = d.MucDich,
                StartTime = d.BatDauLuc,
                EndTime = d.KetThucLuc,
                Attendees = d.SoNguoiThamDu,
                Status = d.TrangThai,
                ApproverId = d.NguoiDuyet,
                CreatedAt = d.NgayTao
            })
            .ToListAsync();

        return Ok(ApiResponseDto<List<RoomBookingDto>>.Ok(bookings));
    }

    [HttpPost("book")]
    public async Task<ActionResult<ApiResponseDto<RoomBookingDto>>> CreateBooking(
        [FromBody] CreateRoomBookingRequest request)
    {
        var entity = new DatPhong
        {
            MaPhong = request.RoomId,
            MaDonVi = request.MaDonVi,
            NguoiYeuCau = request.RequesterId,
            MucDich = request.Purpose,
            BatDauLuc = request.StartTime,
            KetThucLuc = request.EndTime,
            SoNguoiThamDu = request.Attendees,
            TrangThai = "cho_duyet",
            NgayTao = DateTime.UtcNow
        };

        _db.DatPhongs.Add(entity);
        await _db.SaveChangesAsync();

        await _db.Entry(entity).Reference(d => d.Phong).LoadAsync();
        await _db.Entry(entity).Reference(d => d.NguoiYeuCauNavigation).LoadAsync();

        var dto = new RoomBookingDto
        {
            Id = entity.MaDatPhong,
            RoomId = entity.MaPhong,
            RoomName = entity.Phong?.TenPhong,
            RoomCode = entity.Phong?.MaCodePhong,
            RequesterId = entity.NguoiYeuCau,
            RequesterName = entity.NguoiYeuCauNavigation?.HoTen,
            Purpose = entity.MucDich,
            StartTime = entity.BatDauLuc,
            EndTime = entity.KetThucLuc,
            Attendees = entity.SoNguoiThamDu,
            Status = entity.TrangThai,
            ApproverId = entity.NguoiDuyet,
            CreatedAt = entity.NgayTao
        };

        return Ok(ApiResponseDto<RoomBookingDto>.Ok(dto, "Đặt phòng thành công."));
    }

    [HttpGet("bookings/{id:int}")]
    public async Task<ActionResult<ApiResponseDto<RoomBookingDto>>> GetBooking(int id)
    {
        var booking = await _db.DatPhongs
            .Include(d => d.Phong)
            .Include(d => d.NguoiYeuCauNavigation)
            .Include(d => d.NguoiDuyetNavigation)
            .FirstOrDefaultAsync(d => d.MaDatPhong == id);

        if (booking is null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy đặt phòng."));

        var dto = new RoomBookingDto
        {
            Id = booking.MaDatPhong,
            RoomId = booking.MaPhong,
            RoomName = booking.Phong?.TenPhong,
            RoomCode = booking.Phong?.MaCodePhong,
            RequesterId = booking.NguoiYeuCau,
            RequesterName = booking.NguoiYeuCauNavigation?.HoTen,
            Purpose = booking.MucDich,
            StartTime = booking.BatDauLuc,
            EndTime = booking.KetThucLuc,
            Attendees = booking.SoNguoiThamDu,
            Status = booking.TrangThai,
            ApproverId = booking.NguoiDuyet,
            ApproverName = booking.NguoiDuyetNavigation?.HoTen,
            CreatedAt = booking.NgayTao
        };

        return Ok(ApiResponseDto<RoomBookingDto>.Ok(dto));
    }

    [HttpPut("bookings/{id:int}")]
    public async Task<ActionResult<ApiResponseDto<RoomBookingDto>>> UpdateBooking(
        int id,
        [FromBody] UpdateRoomBookingRequest request)
    {
        var booking = await _db.DatPhongs
            .Include(d => d.Phong)
            .Include(d => d.NguoiYeuCauNavigation)
            .FirstOrDefaultAsync(d => d.MaDatPhong == id);

        if (booking is null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy đặt phòng."));

        if (request.RoomId.HasValue)
            booking.MaPhong = request.RoomId.Value;
        if (request.Purpose is not null)
            booking.MucDich = request.Purpose;
        if (request.StartTime.HasValue)
            booking.BatDauLuc = request.StartTime.Value;
        if (request.EndTime.HasValue)
            booking.KetThucLuc = request.EndTime.Value;
        if (request.Attendees.HasValue)
            booking.SoNguoiThamDu = request.Attendees;
        if (request.Status is not null)
            booking.TrangThai = request.Status;

        await _db.SaveChangesAsync();

        var dto = new RoomBookingDto
        {
            Id = booking.MaDatPhong,
            RoomId = booking.MaPhong,
            RoomName = booking.Phong?.TenPhong,
            RoomCode = booking.Phong?.MaCodePhong,
            RequesterId = booking.NguoiYeuCau,
            RequesterName = booking.NguoiYeuCauNavigation?.HoTen,
            Purpose = booking.MucDich,
            StartTime = booking.BatDauLuc,
            EndTime = booking.KetThucLuc,
            Attendees = booking.SoNguoiThamDu,
            Status = booking.TrangThai,
            ApproverId = booking.NguoiDuyet,
            CreatedAt = booking.NgayTao
        };

        return Ok(ApiResponseDto<RoomBookingDto>.Ok(dto, "Cập nhật đặt phòng thành công."));
    }

    [HttpDelete("bookings/{id:int}")]
    public async Task<ActionResult<ApiResponseDto>> CancelBooking(int id)
    {
        var booking = await _db.DatPhongs.FindAsync(id);
        if (booking is null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy đặt phòng."));

        booking.TrangThai = "da_huy";
        await _db.SaveChangesAsync();

        return Ok(ApiResponseDto.Ok("Hủy đặt phòng thành công."));
    }
}

public class RoomBookingDto
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string? RoomName { get; set; }
    public string? RoomCode { get; set; }
    public int RequesterId { get; set; }
    public string? RequesterName { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int? Attendees { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateRoomBookingRequest
{
    public int RoomId { get; set; }
    public int MaDonVi { get; set; }
    public int RequesterId { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int? Attendees { get; set; }
}

public class UpdateRoomBookingRequest
{
    public int? RoomId { get; set; }
    public string? Purpose { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? Attendees { get; set; }
    public string? Status { get; set; }
}
