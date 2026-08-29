using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("TienDoBaiHoc", Schema = "dbo")]
public class TienDoBaiHoc
{
    [Key]
    [Column("ma_tien_do")]
    public int MaTienDo { get; set; }

    [Column("ma_hoc_sinh")]
    public int MaHocSinh { get; set; }

    [Column("ma_bai_hoc")]
    public int MaBaiHoc { get; set; }

    [Column("phan_tram_tien_do")]
    public decimal PhanTramTienDo { get; set; }

    [Column("lan_gui_nhip_tim_cuoi")]
    public DateTime? LanGuiNhipTimCuoi { get; set; }

    [Column("hoan_thanh_luc")]
    public DateTime? HoanThanhLuc { get; set; }

    [Column("ghi_chu")]
    public string? GhiChu { get; set; }

    [ForeignKey(nameof(MaHocSinh))]
    public NguoiDung? HocSinh { get; set; }

    [ForeignKey(nameof(MaBaiHoc))]
    public BaiHoc? BaiHoc { get; set; }
}
