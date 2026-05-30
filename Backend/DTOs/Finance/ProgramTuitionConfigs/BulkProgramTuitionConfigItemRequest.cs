using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Finance.ProgramTuitionConfigs;

public class BulkProgramTuitionConfigItemRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Năm học trong chương trình phải lớn hơn hoặc bằng 1.")]
    public int NamHocTrongChuongTrinh { get; set; }

    [Range(1, 3, ErrorMessage = "Học kỳ trong năm chỉ được là 1, 2 hoặc 3.")]
    public int HocKyTrongNam { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số thứ tự học kỳ phải lớn hơn hoặc bằng 1.")]
    public int SoThuTuHocKy { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Mã học kỳ không hợp lệ.")]
    public int MaHocKy { get; set; }

    [Range(typeof(decimal), "0", "999999999999.99", ErrorMessage = "Số tiền học phí phải lớn hơn hoặc bằng 0.")]
    public decimal SoTienHocPhi { get; set; }

    [Range(typeof(decimal), "0", "999999999999.99", ErrorMessage = "Tiền học liệu phải lớn hơn hoặc bằng 0.")]
    public decimal TienHocLieu { get; set; }

    public string? GhiChu { get; set; }
}
