using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.BuoiHoc;

public class ChangeSessionRoomRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Mã phòng phải lớn hơn 0.")]
    public int MaPhong { get; set; }

    [Required(ErrorMessage = "Lý do thay đổi không được để trống.")]
    public string LyDoThayDoi { get; set; } = string.Empty;
}
