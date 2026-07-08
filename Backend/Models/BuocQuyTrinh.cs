using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("BuocQuyTrinh")]
public class BuocQuyTrinh
{
    [Key]
    public int MaBuoc { get; set; }

    [Required]
    public int MaQuyTrinh { get; set; }

    public int ThuTu { get; set; }

    [Required]
    [MaxLength(100)]
    public string TenBuoc { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? VaiTroXuLy { get; set; }

    [MaxLength(20)]
    public string? KieuBuoc { get; set; }

    [MaxLength(50)]
    public string? SlaKhoangThoiGian { get; set; }

    [ForeignKey("MaQuyTrinh")]
    public virtual QuyTrinhDonTu QuyTrinh { get; set; } = null!;
}
