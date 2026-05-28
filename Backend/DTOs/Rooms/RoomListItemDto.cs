namespace Backend.DTOs.Rooms;

public class RoomListItemDto
{
    public int MaPhong { get; set; }
    public int MaDonVi { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
    public int MaToaNha { get; set; }
    public string MaCodeToaNha { get; set; } = string.Empty;
    public string TenToaNha { get; set; } = string.Empty;
    public int MaTang { get; set; }
    public string TenTang { get; set; } = string.Empty;
    public int ThuTuTang { get; set; }
    public string MaCodePhong { get; set; } = string.Empty;
    public string TenPhong { get; set; } = string.Empty;
    public int SucChua { get; set; }
    public string LoaiPhong { get; set; } = string.Empty;
    public string TrangThaiPhong { get; set; } = string.Empty;
}
