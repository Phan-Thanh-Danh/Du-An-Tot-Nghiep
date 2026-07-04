using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("NangLucGiangVien")]
public class NangLucGiangVien
{
    [Key]
    public int MaNangLuc { get; set; }

    [Required]
    public int MaGiaoVien { get; set; }

    [Required]
    public int MaMonHoc { get; set; }

    /// <summary>
    /// Mức độ phù hợp (1-5), 5 là rất phù hợp
    /// </summary>
    public int MucDoPhuHop { get; set; } = 3;

    /// <summary>
    /// Số lần đã từng dạy môn này
    /// </summary>
    public int SoLanDaDay { get; set; } = 0;

    /// <summary>
    /// Độ ưu tiên phân công (ví dụ: 1 là ưu tiên cao nhất, 0 là bình thường, -1 là ít ưu tiên)
    /// </summary>
    public int UuTien { get; set; } = 0;

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }

    [ForeignKey(nameof(MaGiaoVien))]
    public virtual NguoiDung GiaoVien { get; set; } = null!;

    [ForeignKey(nameof(MaMonHoc))]
    public virtual DanhMucMonHoc MonHoc { get; set; } = null!;
}
