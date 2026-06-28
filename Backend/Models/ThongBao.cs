namespace Backend.Models;

public class ThongBao
{
    public int MaThongBao { get; set; }

    // Legacy group/recipient columns are kept for data compatibility with P0-8 rows.
    public Guid MaNhomThongBao { get; set; }
    public int MaNguoiNhan { get; set; }
    public int MaDonVi { get; set; }
    public string LoaiSuKien { get; set; } = string.Empty;

    public string LoaiThongBao { get; set; } = string.Empty;
    public string? TieuDe { get; set; }
    public string? TomTat { get; set; }
    public string? TomTatNoiDung { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public string? NoiDungJson { get; set; }
    public string? NoiDungText { get; set; }
    public string MucDo { get; set; } = "info";
    public string? DoiTuongLienKet { get; set; }
    public string? LoaiDoiTuongLienKet { get; set; }
    public int? MaDoiTuongLienKet { get; set; }
    public string PhamViGui { get; set; } = string.Empty;
    public string? DuongDan { get; set; }
    public int? NguoiTao { get; set; }
    public string TrangThai { get; set; } = "da_gui";
    public bool DaDoc { get; set; }
    public DateTime? DocLuc { get; set; }
    public DateTime? GuiLuc { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public NguoiDung? NguoiNhan { get; set; }
    public NguoiDung? NguoiTaoNavigation { get; set; }
    public DonVi? DonVi { get; set; }
    public ICollection<ThongBaoNguoiNhan> NguoiNhans { get; set; } = new List<ThongBaoNguoiNhan>();
}
