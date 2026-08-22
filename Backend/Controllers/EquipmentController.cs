using Backend.Constants;
using Backend.DTOs.Facilities;
using Backend.Services.Facilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/master-data/equipment")]
[Authorize]
public class EquipmentController : ControllerBase
{
    private const string ReaderRoles =
        AuthRoles.Principal + "," +
        AuthRoles.SuperAdmin + "," +
        AuthRoles.Admin + "," +
        AuthRoles.CampusAdmin + "," +
        AuthRoles.SubCampusAdmin + "," +
        AuthRoles.AcademicStaff;

    private const string ManagerRoles =
        AuthRoles.Principal + "," +
        AuthRoles.SuperAdmin + "," +
        AuthRoles.Admin + "," +
        AuthRoles.CampusAdmin + "," +
        AuthRoles.SubCampusAdmin;

    private readonly IEquipmentService _equipmentService;
    private readonly Backend.Data.ApplicationDbContext _db;

    public EquipmentController(IEquipmentService equipmentService, Backend.Data.ApplicationDbContext db)
    {
        _equipmentService = equipmentService;
        _db = db;
    }

    [HttpGet("room/{roomId}")]
    [Authorize(Roles = ReaderRoles)]
    public async Task<IActionResult> GetByRoomId(int roomId)
    {
        var result = await _equipmentService.GetEquipmentByRoomIdAsync(roomId);
        return Ok(new { data = result });
    }

    [HttpGet("building/{buildingId}")]
    [Authorize(Roles = ReaderRoles)]
    public async Task<IActionResult> GetByBuildingId(int buildingId)
    {
        var rooms = await _db.PhongHocs.Where(r => r.Tang != null && r.Tang.MaToaNha == buildingId).Select(r => r.MaPhong).ToListAsync();
        var list = await _db.ThietBiPhongs
            .Where(e => rooms.Contains(e.MaPhong))
            .Select(e => new EquipmentDto
            {
                MaThietBi = e.MaThietBi,
                MaPhong = e.MaPhong,
                TenThietBi = e.TenThietBi,
                MaCodeThietBi = e.MaCodeThietBi,
                ChungLoai = e.ChungLoai,
                SoLuong = e.SoLuong,
                TinhTrang = e.TinhTrang,
                NgayKiemDinh = e.NgayKiemDinh,
                GhiChu = e.GhiChu
            })
            .ToListAsync();
        return Ok(new { data = list });
    }

    [HttpPost("room")]
    [Authorize(Roles = ManagerRoles)]
    public async Task<IActionResult> Create([FromBody] CreateEquipmentDto dto)
    {
        var result = await _equipmentService.CreateEquipmentAsync(dto);
        return Ok(new { data = result });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = ManagerRoles)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEquipmentDto dto)
    {
        var result = await _equipmentService.UpdateEquipmentAsync(id, dto);
        return Ok(new { data = result });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = ManagerRoles)]
    public async Task<IActionResult> Delete(int id)
    {
        await _equipmentService.DeleteEquipmentAsync(id);
        return Ok(new { success = true });
    }

    [HttpPost("import")]
    [Authorize(Roles = ManagerRoles)]
    public async Task<IActionResult> ImportExcel(IFormFile file, [FromQuery] int? maPhong)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Không tìm thấy file tải lên." });
        }

        try
        {
            var equipments = new List<CreateEquipmentDto>();

            if (file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var isHeader = true;
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (isHeader)
                    {
                        isHeader = false;
                        continue;
                    }
                    var cols = line.Split(',');
                    if (maPhong.HasValue)
                    {
                        if (cols.Length < 4) continue;
                        var dto = new CreateEquipmentDto
                        {
                            MaPhong = maPhong.Value,
                            TenThietBi = cols[0].Trim(),
                            MaCodeThietBi = cols[1].Trim(),
                            ChungLoai = cols[2].Trim(),
                            SoLuong = int.TryParse(cols[3].Trim(), out int sl) ? sl : 1,
                            TinhTrang = "Hoạt động tốt",
                            GhiChu = ""
                        };
                        if (!string.IsNullOrWhiteSpace(dto.TenThietBi))
                            equipments.Add(dto);
                    }
                    else
                    {
                        if (cols.Length < 8) continue;
                        var maCodePhong = cols[0].Trim();
                        var room = _db.PhongHocs.FirstOrDefault(r => r.MaCodePhong == maCodePhong);
                        if (room == null) continue;

                        var dto = new CreateEquipmentDto
                        {
                            MaPhong = room.MaPhong,
                            TenThietBi = cols[4].Trim(),
                            MaCodeThietBi = cols[5].Trim(),
                            ChungLoai = cols[6].Trim(),
                            SoLuong = int.TryParse(cols[7].Trim(), out int sl) ? sl : 1,
                            TinhTrang = "Hoạt động tốt",
                            GhiChu = ""
                        };
                        if (!string.IsNullOrWhiteSpace(dto.TenThietBi))
                            equipments.Add(dto);
                    }
                }
            }
            else
            {
                using (var stream = file.OpenReadStream())
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        if (maPhong.HasValue)
                        {
                            var dto = new CreateEquipmentDto
                            {
                                MaPhong = maPhong.Value,
                                TenThietBi = worksheet.Cell(row, 1).Value.ToString().Trim(),
                                MaCodeThietBi = worksheet.Cell(row, 2).Value.ToString().Trim(),
                                ChungLoai = worksheet.Cell(row, 3).Value.ToString().Trim(),
                                SoLuong = int.TryParse(worksheet.Cell(row, 4).Value.ToString(), out int sl) ? sl : 1,
                                TinhTrang = "Hoạt động tốt",
                                GhiChu = ""
                            };
                            if (!string.IsNullOrWhiteSpace(dto.TenThietBi))
                                equipments.Add(dto);
                        }
                        else
                        {
                            var maCodePhong = worksheet.Cell(row, 1).Value.ToString().Trim();
                            var room = _db.PhongHocs.FirstOrDefault(r => r.MaCodePhong == maCodePhong);
                            if (room == null) continue;

                            var dto = new CreateEquipmentDto
                            {
                                MaPhong = room.MaPhong,
                                TenThietBi = worksheet.Cell(row, 5).Value.ToString().Trim(),
                                MaCodeThietBi = worksheet.Cell(row, 6).Value.ToString().Trim(),
                                ChungLoai = worksheet.Cell(row, 7).Value.ToString().Trim(),
                                SoLuong = int.TryParse(worksheet.Cell(row, 8).Value.ToString(), out int sl) ? sl : 1,
                                TinhTrang = "Hoạt động tốt",
                                GhiChu = ""
                            };

                            if (!string.IsNullOrWhiteSpace(dto.TenThietBi))
                                equipments.Add(dto);
                        }
                    }
                }
            }

            foreach (var item in equipments)
            {
                await _equipmentService.CreateEquipmentAsync(item);
            }

            return Ok(new { success = true, importedCount = equipments.Count });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return StatusCode(500, new { message = "Lỗi khi xử lý file Excel", error = ex.Message });
        }
    }
}
