namespace Backend.DTOs.TeacherPersonnel;

public class TeacherPersonnelDetailDto
{
    public int MaNguoiDung { get; set; }
    public string MaGiangVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public int MaDonVi { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
    public string TrangThai { get; set; } = "hoat_dong";
    public DateTime NgayTao { get; set; }
    public DateTime? LanDangNhapCuoi { get; set; }

    // Chuyên môn & Chuyên ngành
    public List<TeacherMajorDto> ChuyenNganhList { get; set; } = [];
    public List<TeacherSubjectCapabilityDto> MonHocList { get; set; } = [];

    // Tóm tắt tải giảng dạy & Đánh giá
    public TeacherWorkloadSummaryDto TuanNayWorkload { get; set; } = new();
    public TeacherEvaluationSummaryDto EvaluationSummary { get; set; } = new();

    // Nguyện vọng giảng dạy gần nhất
    public TeacherPreferenceSummaryDto? NguyenVongGanNhat { get; set; }
}

public class TeacherMajorDto
{
    public int MaChuyenNganh { get; set; }
    public string TenChuyenNganh { get; set; } = string.Empty;
    public string MaCode { get; set; } = string.Empty;
    public bool LaChuyenMonChinh { get; set; }
    public int MucDoPhuHop { get; set; }
    public int? SoNamKinhNghiem { get; set; }
}

public class TeacherSubjectCapabilityDto
{
    public int MaMonHoc { get; set; }
    public string MaCodeMonHoc { get; set; } = string.Empty;
    public string TenMonHoc { get; set; } = string.Empty;
    public int SoTinChi { get; set; }
    public int MucDoPhuHop { get; set; }
    public int? SoNamKinhNghiem { get; set; }
    public int SoLanDaDay { get; set; }
    public bool LaMonChinh { get; set; }
    public bool ConHoatDong { get; set; }
}

public class TeacherPreferenceSummaryDto
{
    public int MaHocKy { get; set; }
    public string TenHocKy { get; set; } = string.Empty;
    public int? SoLopToiDaMongMuon { get; set; }
    public int? SoCaToiDaMoiTuan { get; set; }
    public string? GhiChu { get; set; }
    public string TrangThai { get; set; } = "draft";
    public List<string> CaUuTien { get; set; } = [];
}
