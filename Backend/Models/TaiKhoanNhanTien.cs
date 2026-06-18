namespace Backend.Models;

public class TaiKhoanNhanTien
{
    public int MaTaiKhoanNhanTien { get; set; }

    public int MaDonVi { get; set; }

    public string TenNganHang { get; set; } = string.Empty;
    public string MaNganHang { get; set; } = string.Empty;
    public string SoTaiKhoan { get; set; } = string.Empty;
    public string TenChuTaiKhoan { get; set; } = string.Empty;
    public string? ChiNhanh { get; set; }

    public string NhaCungCapThanhToan { get; set; } = "payos";
    public string TrangThaiDuyet { get; set; } = "nhap";
    public string? CauHinhProviderJson { get; set; }

    public bool LaMacDinh { get; set; }
    public bool ConHoatDong { get; set; }

    public int? NguoiTao { get; set; }
    public int? NguoiDuyet { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public string? LyDoTuChoi { get; set; }

    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public DonVi? DonVi { get; set; }
    public NguoiDung? NguoiTaoNavigation { get; set; }
    public NguoiDung? NguoiDuyetNavigation { get; set; }
}
