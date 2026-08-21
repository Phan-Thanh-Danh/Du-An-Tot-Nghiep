using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("VaiTroQuyenHan")]
public class VaiTroQuyenHan
{
    [Column("ma_vai_tro")]
    public int MaVaiTro { get; set; }

    [Column("ma_quyen_han")]
    public int MaQuyenHan { get; set; }

    [Column("ngay_cap")]
    public DateTime NgayCap { get; set; } = DateTime.UtcNow;

    [Column("nguoi_cap")]
    public int? NguoiCap { get; set; }

    public virtual VaiTro? VaiTro { get; set; }
    public virtual QuyenHan? QuyenHan { get; set; }
    public virtual NguoiDung? NguoiCapNavigation { get; set; }
}
