using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("yeu_cau_xuat_du_lieu")]
    public class YeuCauXuatDuLieu
    {
        [Key]
        [Column("ma_yeu_cau")]
        public string MaYeuCau { get; set; } = string.Empty;

        [Required]
        [Column("loai_bao_cao")]
        public string LoaiBaoCao { get; set; } = string.Empty;

        [Column("ten_bao_cao")]
        public string TenBaoCao { get; set; } = string.Empty;

        [Column("hoc_ky")]
        public string? HocKy { get; set; }

        [Column("cap_don_vi")]
        public string? CapDonVi { get; set; }

        [Required]
        [Column("dinh_dang")]
        public string DinhDang { get; set; } = "excel";

        [Required]
        [Column("trang_thai")]
        public string TrangThai { get; set; } = "queued"; // queued, processing, completed, failed

        [Column("duong_dan_file")]
        public string? DuongDanFile { get; set; }

        [Column("nguoi_yeu_cau")]
        public string NguoiYeuCau { get; set; } = string.Empty;

        [Column("thoi_gian_yeu_cau")]
        public DateTime ThoiGianYeuCau { get; set; }

        [Column("thoi_gian_hoan_thanh")]
        public DateTime? ThoiGianHoanThanh { get; set; }
    }
}
