using Backend.DTOs.Common;
using System.Text.Json;

namespace Backend.DTOs.QuizManagement;

public class QuizDto
{
    public int MaDeKiemTra { get; set; }
    public int? MaMonHoc { get; set; }
    public string? MaCodeMonHoc { get; set; }
    public string? TenMonHoc { get; set; }
    public int? MaHocKy { get; set; }
    public string? TenHocKy { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public int ThoiGianPhut { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? LoaiDeThi { get; set; }
    public string? HinhThucThi { get; set; }
    public decimal? TyLeTracNghiem { get; set; }
    public decimal? TyLeTuLuan { get; set; }
    public int? MaNguoiSoan { get; set; }
    public string? TenNguoiSoan { get; set; }
    public string? TrangThaiDuyet { get; set; }
    public int SoCauHoi { get; set; }
    public decimal TongDiem { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}

public class QuizDetailDto
{
    public int MaDeKiemTra { get; set; }
    public int? MaMonHoc { get; set; }
    public string? TenMonHoc { get; set; }
    public int? MaHocKy { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public int ThoiGianPhut { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? LoaiDeThi { get; set; }
    public string? HinhThucThi { get; set; }
    public decimal? TyLeTracNghiem { get; set; }
    public decimal? TyLeTuLuan { get; set; }
    public int? MaNguoiSoan { get; set; }
    public string? TrangThaiDuyet { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public QuizConfigurationDto CauHinh { get; set; } = new();
    
    public decimal TongDiemCauHoi { get; set; }
    public int TongSoCauHoi { get; set; }
    public int SoCauTracNghiem { get; set; }
    public int SoCauTuLuan { get; set; }

    public List<QuizQuestionDto> DanhSachCauHoi { get; set; } = new();
}

public class QuizQuestionDto
{
    public int MaCauHoi { get; set; }
    public int? MaMonHoc { get; set; }
    public string LoaiCauHoi { get; set; } = string.Empty;
    public string NoiDung { get; set; } = string.Empty;
    public string? KieuLuaChon { get; set; }
    public object? LuaChon { get; set; }
    public object? DapAnDung { get; set; }
    public string? GiaiThichDapAn { get; set; }
    public string? DoKho { get; set; }
    public bool ConHoatDong { get; set; }
    
    public decimal DiemSo { get; set; }
    public int? ThuTu { get; set; }

    public static object? ParseJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<object>(json);
        }
        catch
        {
            return json; // Fallback to raw string if not json
        }
    }
}

public class QuizFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Keyword { get; set; }
    public int? MaMonHoc { get; set; }
    public int? MaHocKy { get; set; }
    public string? TrangThai { get; set; }
    public string? LoaiDeThi { get; set; }
    public string? HinhThucThi { get; set; }
}
