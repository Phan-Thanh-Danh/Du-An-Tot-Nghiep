using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.BuoiHoc;

public class ChangeSessionShiftRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Mã ca học phải lớn hơn 0.")]
    public int MaCaHoc { get; set; }

    [Required(ErrorMessage = "Lý do thay đổi không được để trống.")]
    public string LyDoThayDoi { get; set; } = string.Empty;
}
