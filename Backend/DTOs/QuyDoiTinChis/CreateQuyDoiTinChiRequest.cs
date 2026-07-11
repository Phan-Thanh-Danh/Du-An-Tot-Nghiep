using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.QuyDoiTinChis;

public class CreateQuyDoiTinChiRequest
{
    [Required(ErrorMessage = "Số tín chỉ là bắt buộc.")]
    [Range(1, 20, ErrorMessage = "Số tín chỉ phải từ 1 đến 20.")]
    public int SoTinChi { get; set; }

    [Required(ErrorMessage = "Số block học là bắt buộc.")]
    [Range(1, 5, ErrorMessage = "Số block học phải từ 1 đến 5.")]
    public int SoBlockHoc { get; set; }

    [Required(ErrorMessage = "Số buổi mỗi tuần là bắt buộc.")]
    [Range(1, 10, ErrorMessage = "Số buổi mỗi tuần phải từ 1 đến 10.")]
    public int SoBuoiMoiTuan { get; set; }

    [Required(ErrorMessage = "Số ca mỗi buổi là bắt buộc.")]
    [Range(1, 5, ErrorMessage = "Số ca mỗi buổi phải từ 1 đến 5.")]
    public int SoCaMoiBuoi { get; set; }
}
