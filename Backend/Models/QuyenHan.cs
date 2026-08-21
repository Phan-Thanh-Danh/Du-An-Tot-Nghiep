using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("QuyenHan")]
public class QuyenHan
{
    [Key]
    [Column("ma_quyen_han")]
    public int MaQuyenHan { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("ma_code")]
    public string MaCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("ten_quyen_han")]
    public string TenQuyenHan { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("module")]
    public string Module { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("action")]
    public string Action { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("mo_ta")]
    public string? MoTa { get; set; }

    public virtual ICollection<VaiTroQuyenHan> VaiTroQuyenHans { get; set; } = new List<VaiTroQuyenHan>();
}
