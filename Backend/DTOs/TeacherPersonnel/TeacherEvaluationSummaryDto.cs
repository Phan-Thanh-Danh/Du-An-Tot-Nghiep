namespace Backend.DTOs.TeacherPersonnel;

public class TeacherEvaluationSummaryDto
{
    public decimal DiemTrungBinhChung { get; set; }
    public int TongSoLuotDanhGia { get; set; }
    public int TongSoHocSinhDanhGia { get; set; }
    public List<TeacherEvaluationTermDto> TheoHocKy { get; set; } = [];
    public List<TeacherEvaluationFeedbackDto> NhanXetGanNhat { get; set; } = [];
}

public class TeacherEvaluationTermDto
{
    public int MaHocKy { get; set; }
    public string TenHocKy { get; set; } = string.Empty;
    public decimal DiemTrungBinh { get; set; }
    public int SoLuotDanhGia { get; set; }
    public int SoKhoaHoc { get; set; }
}

public class TeacherEvaluationFeedbackDto
{
    public int MaDanhGia { get; set; }
    public string TenKhoaHoc { get; set; } = string.Empty;
    public decimal DiemSo { get; set; }
    public string? NhanXet { get; set; }
    public DateTime? NgayDanhGia { get; set; }
}
