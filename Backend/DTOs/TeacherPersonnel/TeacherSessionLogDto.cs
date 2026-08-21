namespace Backend.DTOs.TeacherPersonnel;

public class TeacherSessionLogDto
{
    public int MaBuoiHoc { get; set; }
    public int MaKhoaHoc { get; set; }
    public string TenMonHoc { get; set; } = string.Empty;
    public string MaCodeMonHoc { get; set; } = string.Empty;
    public string TenLopHanhChinh { get; set; } = string.Empty;
    public DateOnly NgayHoc { get; set; }
    public string TenCaHoc { get; set; } = string.Empty;
    public string GioBatDau { get; set; } = string.Empty;
    public string GioKetThuc { get; set; } = string.Empty;
    public string TenPhong { get; set; } = string.Empty;
    public string TrangThaiBuoi { get; set; } = "chua_dien_ra"; // chua_dien_ra, dang_dien_ra, da_dien_ra, da_huy, doi_lich
    public bool LaDayThay { get; set; }
    public string? TenGiangVienChinh { get; set; }
    public string TrangThaiDiemDanh { get; set; } = "chua_gui"; // chua_gui, da_gui, bi_khoa, het_han
    public DateTime? ThoiDiemGuiDiemDanh { get; set; }
    public DateTime? HanDiemDanh { get; set; }
    public bool DungHanDiemDanh { get; set; }
    public int SoLuongSinhVien { get; set; }
    public int SoCoMat { get; set; }
    public int SoVang { get; set; }
    public int SoDiMuon { get; set; }
}

public class TeacherSessionLogsSummaryDto
{
    public int TongSoCa { get; set; }
    public int SoCaDaDienRa { get; set; }
    public int SoCaDayThay { get; set; }
    public int SoCaBiHuy { get; set; }
    public int SoCaDiemDanhDungHan { get; set; }
    public int SoCaDiemDanhTreHan { get; set; }
    public int SoCaChuaDiemDanh { get; set; }
    public decimal TyLeDiemDanhDungHan { get; set; }
    public List<TeacherSessionLogDto> Items { get; set; } = [];
}
