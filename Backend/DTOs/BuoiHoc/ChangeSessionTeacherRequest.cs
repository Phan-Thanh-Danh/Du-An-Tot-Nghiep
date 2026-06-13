using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.BuoiHoc;

public class ChangeSessionTeacherRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Mã giáo viên dạy thay phải lớn hơn 0.")]
    public int MaGiaoVienDayThay { get; set; }

    [Required(ErrorMessage = "Lý do thay đổi không được để trống.")]
    public string LyDoThayDoi { get; set; } = string.Empty;
}
