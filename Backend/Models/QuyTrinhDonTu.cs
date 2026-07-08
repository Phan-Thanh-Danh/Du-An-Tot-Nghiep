using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("QuyTrinhDonTu")]
public class QuyTrinhDonTu
{
    [Key]
    public int MaQuyTrinh { get; set; }

    [Required]
    [MaxLength(50)]
    public string LoaiDon { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string TenQuyTrinh { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    [MaxLength(50)]
    public string? SlaKhoangThoiGian { get; set; }

    public virtual ICollection<BuocQuyTrinh> BuocQuyTrinhs { get; set; } = new List<BuocQuyTrinh>();
}
