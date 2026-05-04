namespace Backend.Models;

public class DanhGiaGiaoVien
{
    public int MaDanhGia { get; set; }
    public int MaGiaoVien { get; set; }
    public int MaHocKy { get; set; }
    public int MaCauHoiDg { get; set; }
    public int DiemSo { get; set; }
    public string? NhanXetTuDo { get; set; }
    public string? AiCamXuc { get; set; }
    public string? AiChuDe { get; set; }
    public DateTime NgayTao { get; set; }
    public string? CohortHash { get; set; }

    public NguoiDung? GiaoVien { get; set; }
    public HocKy? HocKy { get; set; }
    public CauHoiDanhGia? CauHoiDg { get; set; }
}
