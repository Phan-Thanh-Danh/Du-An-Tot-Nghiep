using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Blocks;

public class UpdateBlockRequest
{
    [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc.")]
    public DateOnly NgayBatDau { get; set; }

    [Required(ErrorMessage = "Ngày kết thúc là bắt buộc.")]
    public DateOnly NgayKetThuc { get; set; }
}
