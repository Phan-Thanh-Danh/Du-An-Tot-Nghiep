using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("GiaoVienChuyenNganh")]
public class GiaoVienChuyenNganh
{
    [Required]
    public int MaGiaoVien { get; set; }

    [Required]
    public int MaChuyenNganh { get; set; }

    /// <summary>
    /// Giảng viên có thể có nhiều chuyên ngành, nhưng chỉ 1 chuyên môn chính
    /// </summary>
    public bool LaChuyenMonChinh { get; set; } = false;

    /// <summary>
    /// Mức độ phù hợp của giảng viên với chuyên ngành (0-100)
    /// </summary>
    public int MucDoPhuHop { get; set; } = 80;

    /// <summary>
    /// Số năm kinh nghiệm thực tế hoặc giảng dạy trong chuyên ngành này
    /// </summary>
    public int? SoNamKinhNghiem { get; set; }

    public bool ConHoatDong { get; set; } = true;

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    public DateTime? NgayCapNhat { get; set; }

    [ForeignKey(nameof(MaGiaoVien))]
    public virtual NguoiDung GiaoVien { get; set; } = null!;

    [ForeignKey(nameof(MaChuyenNganh))]
    public virtual ChuyenNganh ChuyenNganh { get; set; } = null!;
}
