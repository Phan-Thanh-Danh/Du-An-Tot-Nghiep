using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.TrainingProgramTerms;

public class CreateTrainingProgramTermRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Chương trình đào tạo không hợp lệ.")]
    public int MaChuongTrinh { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Học kỳ không hợp lệ.")]
    public int MaHocKy { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Thứ tự học kỳ phải lớn hơn 0.")]
    public int ThuTuHocKy { get; set; }
}
