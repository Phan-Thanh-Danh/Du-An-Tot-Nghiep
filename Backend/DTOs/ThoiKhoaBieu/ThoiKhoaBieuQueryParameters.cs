using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.ThoiKhoaBieu;

public class ThoiKhoaBieuQueryParameters
{
    public int? MaKhoaHoc { get; set; }
    public int? MaHocKy { get; set; }
    public int? MaLop { get; set; }
    public int? MaGiaoVien { get; set; }
    public int? MaPhong { get; set; }
    public int? MaCaHoc { get; set; }

    [Range(1, 7, ErrorMessage = "Thứ trong tuần phải từ 1 đến 7.")]
    public int? ThuTrongTuan { get; set; }

    public string? TrangThai { get; set; }
    public DateOnly? NgayBatDau { get; set; }
    public DateOnly? NgayKetThuc { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "pageIndex phải lớn hơn 0.")]
    public int PageIndex { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "pageSize phải từ 1 đến 100.")]
    public int PageSize { get; set; } = 20;
}
