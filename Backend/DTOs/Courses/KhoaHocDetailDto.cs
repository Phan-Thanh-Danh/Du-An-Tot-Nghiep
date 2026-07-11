namespace Backend.DTOs.Courses;

public class KhoaHocDetailDto : KhoaHocDto
{
    public IReadOnlyList<KhoaHocChuongDto> Chuongs { get; set; } = [];

    public List<KhoaHocBaiHocDto> Lessons { get; set; } = new();

    // Gợi ý thông số từ QuyDoiTinChi
    public int? SoBlockHoc { get; set; }
    public int? GoiYSoBuoiMoiTuan { get; set; }
    public int? GoiYSoCaMoiBuoi { get; set; }
}
