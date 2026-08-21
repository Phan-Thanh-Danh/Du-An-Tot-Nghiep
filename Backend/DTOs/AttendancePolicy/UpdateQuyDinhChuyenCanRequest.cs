using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.AttendancePolicy;

public class UpdateQuyDinhChuyenCanRequest
{
    [Range(0, 1000, ErrorMessage = "Quỹ vắng tối đa phải từ 0 đến 1000 buổi (0 = không giới hạn).")]
    public int QuyVangToiDa { get; set; }

    [Range(0, 100, ErrorMessage = "Tỷ lệ cảnh báo phải từ 0 đến 100%.")]
    public decimal TiLeCanhBao { get; set; }

    [Range(0, 10, ErrorMessage = "Hệ số vắng không phép phải từ 0 đến 10.")]
    public decimal HeSoVangKhongPhep { get; set; }

    [Range(0, 10, ErrorMessage = "Hệ số vắng có phép phải từ 0 đến 10.")]
    public decimal HeSoVangCoPhep { get; set; }

    [Range(0, 10, ErrorMessage = "Hệ số đi muộn phải từ 0 đến 10.")]
    public decimal HeSoDiMuon { get; set; }

    [Range(1, 1440, ErrorMessage = "Hạn gửi điểm danh phải từ 1 đến 1440 phút.")]
    public int HanGuiPhut { get; set; }

    [Range(0, 1440, ErrorMessage = "Hạn chỉnh sửa phải từ 0 đến 1440 phút.")]
    public int HanChinhSuaPhut { get; set; }

    [MaxLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự.")]
    public string? GhiChu { get; set; }
}
