namespace Backend.DTOs.TrainingPrograms;

public class TrainingProgramSetupDto
{
    public int MaKhoaTuyenSinh { get; set; }
    public string MaCodeKhoa { get; set; } = string.Empty;
    public string TenKhoa { get; set; } = string.Empty;
    public int NamBatDau { get; set; }
    public int? NamKetThucDuKien { get; set; }
    public int TongSoChuyenNganh { get; set; }
    public int SoChuyenNganhDaCoChuongTrinh { get; set; }
    public int SoChuyenNganhChuaCoChuongTrinh { get; set; }
    public List<TrainingProgramSetupItemDto> Items { get; set; } = [];
}

public class TrainingProgramSetupItemDto
{
    public int MaNganh { get; set; }
    public string MaCodeNganh { get; set; } = string.Empty;
    public string TenNganh { get; set; } = string.Empty;
    public int MaChuyenNganh { get; set; }
    public string TenChuyenNganh { get; set; } = string.Empty;
    public bool DaCoChuongTrinh { get; set; }
    public TrainingProgramSetupProgramDto? ChuongTrinhHienTai { get; set; }
    public TrainingProgramSetupProgramDto? ChuongTrinhNguonDeXuat { get; set; }
    public bool CoTheClone { get; set; }
    public string? GhiChu { get; set; }
}

public class TrainingProgramSetupProgramDto
{
    public int MaChuongTrinh { get; set; }
    public string MaCodeChuongTrinh { get; set; } = string.Empty;
    public string TenChuongTrinh { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public int MaKhoaTuyenSinh { get; set; }
    public string MaCodeKhoa { get; set; } = string.Empty;
    public string TenKhoa { get; set; } = string.Empty;
    public int SoHocKy { get; set; }
    public int ThoiGianDaoTaoThang { get; set; }
    public int? TongTinChiYeuCau { get; set; }
    public DateOnly? NgayHieuLuc { get; set; }
    public DateOnly? NgayHetHieuLuc { get; set; }
}
