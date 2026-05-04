namespace Backend.Models;

public class TuyChonThongBao
{
    public int MaNguoiDung { get; set; }
    public bool NhanEmail { get; set; }
    public bool NhanPush { get; set; }
    public bool NhanSms { get; set; }
    public DateTime CapNhatLuc { get; set; }

    public NguoiDung? NguoiDung { get; set; }
}
