namespace Backend.DTOs.TrainingPrograms;

public class CurriculumSubjectDto
{
    public int MaChuongTrinhMonHoc { get; set; }
    public int MaChuongTrinh { get; set; }
    public int MaMonHoc { get; set; }
    public string MaCodeMonHoc { get; set; } = string.Empty;
    public string TenMonHoc { get; set; } = string.Empty;
    public int HocKyDuKien { get; set; }
    public int SoTinChi { get; set; }
    public int SoTietLyThuyet { get; set; }
    public int SoTietThucHanh { get; set; }
    public string LoaiMonHoc { get; set; } = string.Empty;
    public bool BatBuoc { get; set; }
    public int ThuTu { get; set; }
    public string? GhiChu { get; set; }
    public bool ConHoatDong { get; set; }
    public List<PrerequisiteSubjectDto> MonTienQuyets { get; set; } = [];
}

public class PrerequisiteSubjectDto
{
    public int MaMonTienQuyet { get; set; }
    public string MaCodeMonTienQuyet { get; set; } = string.Empty;
    public string TenMonTienQuyet { get; set; } = string.Empty;
    public decimal? DiemToiThieu { get; set; }
}

public class AddCurriculumSubjectRequest
{
    public int MaMonHoc { get; set; }
    public int HocKyDuKien { get; set; }
    public int SoTinChi { get; set; }
    public string LoaiMonHoc { get; set; } = "Bắt buộc";
    public bool BatBuoc { get; set; } = true;
    public int ThuTu { get; set; } = 1;
    public string? GhiChu { get; set; }
    public List<int>? MaMonTienQuyetIds { get; set; }
}

public class UpdateCurriculumSubjectRequest
{
    public int HocKyDuKien { get; set; }
    public int SoTinChi { get; set; }
    public string LoaiMonHoc { get; set; } = "Bắt buộc";
    public bool BatBuoc { get; set; } = true;
    public int ThuTu { get; set; } = 1;
    public string? GhiChu { get; set; }
    public List<int>? MaMonTienQuyetIds { get; set; }
}
