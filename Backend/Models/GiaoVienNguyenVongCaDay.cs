namespace Backend.Models;

public class GiaoVienNguyenVongCaDay
{
    public int Id { get; set; }
    public int NguyenVongId { get; set; }
    public int ThuTrongTuan { get; set; }
    public int MaCaHoc { get; set; }
    public string MucDo { get; set; } = string.Empty; // preferred, available, unavailable
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public virtual GiaoVienNguyenVongHocKy NguyenVongHocKy { get; set; } = null!;
    public virtual CaHoc CaHoc { get; set; } = null!;
}
