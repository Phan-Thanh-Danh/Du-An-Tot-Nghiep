using Backend.Data;
using Backend.DTOs.Facilities;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Facilities;

public class EquipmentService : IEquipmentService
{
    private readonly ApplicationDbContext _db;

    public EquipmentService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<EquipmentDto>> GetEquipmentByRoomIdAsync(int roomId)
    {
        var list = await _db.ThietBiPhongs
            .Where(e => e.MaPhong == roomId)
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
        return list;
    }

    public async Task<EquipmentDto> CreateEquipmentAsync(CreateEquipmentDto dto)
    {
        var room = await _db.PhongHocs.FindAsync(dto.MaPhong);
        if (room == null) throw new Exception($"Không tìm thấy phòng {dto.MaPhong}");

        var equipment = new ThietBiPhong
        {
            MaPhong = dto.MaPhong,
            TenThietBi = dto.TenThietBi,
            MaCodeThietBi = dto.MaCodeThietBi,
            ChungLoai = dto.ChungLoai,
            SoLuong = dto.SoLuong,
            TinhTrang = dto.TinhTrang,
            NgayKiemDinh = dto.NgayKiemDinh,
            GhiChu = dto.GhiChu
        };

        _db.ThietBiPhongs.Add(equipment);
        await _db.SaveChangesAsync();

        return new EquipmentDto
        {
            MaThietBi = equipment.MaThietBi,
            MaPhong = equipment.MaPhong,
            TenThietBi = equipment.TenThietBi,
            MaCodeThietBi = equipment.MaCodeThietBi,
            ChungLoai = equipment.ChungLoai,
            SoLuong = equipment.SoLuong,
            TinhTrang = equipment.TinhTrang,
            NgayKiemDinh = equipment.NgayKiemDinh,
            GhiChu = equipment.GhiChu
        };
    }

    public async Task<EquipmentDto> UpdateEquipmentAsync(int id, UpdateEquipmentDto dto)
    {
        var equipment = await _db.ThietBiPhongs.FindAsync(id);
        if (equipment == null) throw new Exception($"Không tìm thấy thiết bị {id}");

        equipment.TenThietBi = dto.TenThietBi;
        equipment.MaCodeThietBi = dto.MaCodeThietBi;
        equipment.ChungLoai = dto.ChungLoai;
        equipment.SoLuong = dto.SoLuong;
        equipment.TinhTrang = dto.TinhTrang;
        equipment.NgayKiemDinh = dto.NgayKiemDinh;
        equipment.GhiChu = dto.GhiChu;

        await _db.SaveChangesAsync();

        return new EquipmentDto
        {
            MaThietBi = equipment.MaThietBi,
            MaPhong = equipment.MaPhong,
            TenThietBi = equipment.TenThietBi,
            MaCodeThietBi = equipment.MaCodeThietBi,
            ChungLoai = equipment.ChungLoai,
            SoLuong = equipment.SoLuong,
            TinhTrang = equipment.TinhTrang,
            NgayKiemDinh = equipment.NgayKiemDinh,
            GhiChu = equipment.GhiChu
        };
    }

    public async Task DeleteEquipmentAsync(int id)
    {
        var equipment = await _db.ThietBiPhongs.FindAsync(id);
        if (equipment != null)
        {
            _db.ThietBiPhongs.Remove(equipment);
            await _db.SaveChangesAsync();
        }
    }
}
