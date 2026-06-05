namespace Backend.DTOs.Courses;

public class KhoaHocDetailDto : KhoaHocDto
{
    public IReadOnlyList<KhoaHocChuongDto> Chuongs { get; set; } = [];
}
