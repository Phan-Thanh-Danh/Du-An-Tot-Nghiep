using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("GiaoVienMonHoc")]
public class GiaoVienMonHoc
{
    [Required]
    public int MaGiaoVien { get; set; }

    [Required]
    public int MaMonHoc { get; set; }

    /// <summary>
    /// Mức độ phù hợp của giảng viên với môn học (0-100)
    /// </summary>
    public int MucDoPhuHop { get; set; } = 80;

    /// <summary>
    /// Số lần đã từng dạy môn này
    /// </summary>
    public int SoLanDaDay { get; set; } = 0;

    /// <summary>
    /// Số năm kinh nghiệm dạy hoặc làm việc thực tế với môn học này
    /// </summary>
    public int? SoNamKinhNghiem { get; set; }

    /// <summary>
    /// Đánh dấu đây là môn chính (thế mạnh) của giảng viên
    /// </summary>
    public bool LaMonChinh { get; set; } = false;

    public bool ConHoatDong { get; set; } = true;

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    public DateTime? NgayCapNhat { get; set; }

    [ForeignKey(nameof(MaGiaoVien))]
    public virtual NguoiDung GiaoVien { get; set; } = null!;

    [ForeignKey(nameof(MaMonHoc))]
    public virtual DanhMucMonHoc MonHoc { get; set; } = null!;
}
