using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.BuoiHoc;

public class CancelSessionRequest
{
    [Required(ErrorMessage = "Lý do thay đổi không được để trống.")]
    public string LyDoThayDoi { get; set; } = string.Empty;
}
