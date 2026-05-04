namespace Backend.Models;

public class DiemSo
{
    public int MaDiemSo { get; set; }
    public int MaDonVi { get; set; }
    public int MaHocSinh { get; set; }
    public int MaMonHoc { get; set; }
    public int MaHocKy { get; set; }
    public decimal? DiemQuaTrinh { get; set; }
    public decimal? DiemGiuaKy { get; set; }
    public decimal? DiemCuoiKy { get; set; }
    public decimal GpaMonHoc { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public bool DaKhoa { get; set; }
    public string? LyDoRot { get; set; }
    public int NamNhapHoc { get; set; }

    public DonVi? DonVi { get; set; }
    public NguoiDung? HocSinh { get; set; }
    public DanhMucMonHoc? MonHoc { get; set; }
    public HocKy? HocKy { get; set; }
}
