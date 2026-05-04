namespace Backend.Models;

public class LienKetPhuHuynh
{
    public int MaLienKetPh { get; set; }
    public int MaPhuHuynh { get; set; }
    public int MaHocSinh { get; set; }
    public string QuyenXem { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public DateTime? LienKetLuc { get; set; }

    public NguoiDung? PhuHuynh { get; set; }
    public NguoiDung? HocSinh { get; set; }
}
