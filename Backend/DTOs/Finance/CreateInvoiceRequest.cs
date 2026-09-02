namespace Backend.DTOs.Finance;

public class CreateInvoiceRequest
{
    public int MaHocSinh { get; set; }
    public int MaHocKy { get; set; }
    public string LoaiHoaDon { get; set; } = "hoc_phi"; // hoc_phi, le_phi, tai_lieu, khac
    public decimal SoTien { get; set; }
    public decimal GiamTru { get; set; } = 0;
    public DateOnly HanThanhToan { get; set; }
    public string? GhiChu { get; set; }
}

public class UpdateInvoiceStatusRequest
{
    public string TrangThaiMoi { get; set; } = string.Empty; // chua_thanh_toan, thanh_toan_mot_phan, da_thanh_toan, qua_han, da_huy
    public decimal? SoTienThanhToan { get; set; }
    public string? LyDo { get; set; }
}

public class CreateRefundRequest
{
    public int MaHoaDon { get; set; }
    public decimal SoTienYeuCau { get; set; }
    public string LoaiHoanPhi { get; set; } = string.Empty; // hoan_toan, hoan_phan, dieu_chinh
    public string? LyDoYeuCau { get; set; }
}

public class ApproveRefundRequest
{
    public bool DuaVao { get; set; } // true = duyệt, false = từ chối
    public string? LyDo { get; set; }
}
