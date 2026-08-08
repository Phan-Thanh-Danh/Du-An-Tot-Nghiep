namespace Backend.DTOs.PassFailRules;

public class PassFailRuleDto
{
    public int MaCauHinhDiem { get; set; }
    public int MaMonHoc { get; set; }
    public string? MaCodeMonHoc { get; set; }
    public string? TenMonHoc { get; set; }
    public int MaHocKy { get; set; }
    public string? TenHocKy { get; set; }
    public decimal TrongSoQuaTrinh { get; set; }
    public decimal TrongSoGiuaKy { get; set; }
    public decimal TrongSoCuoiKy { get; set; }
    public decimal NguongDat { get; set; }
    public decimal TiLeChuyenCanToiThieu { get; set; }
    public int? NguoiCapNhat { get; set; }
    public string? TenNguoiCapNhat { get; set; }
    public DateTime? CapNhatLuc { get; set; }
}

public class PassFailRuleListResponse
{
    public int TongMonHoc { get; set; }
    public int DaCauHinh { get; set; }
    public int ChuaCauHinh { get; set; }
    public List<PassFailRuleDto> Items { get; set; } = new();
}

public class UpsertPassFailRuleRequest
{
    public int MaMonHoc { get; set; }
    public int MaHocKy { get; set; }
    public decimal TrongSoQuaTrinh { get; set; }
    public decimal TrongSoGiuaKy { get; set; }
    public decimal TrongSoCuoiKy { get; set; }
    public decimal NguongDat { get; set; }
    public decimal TiLeChuyenCanToiThieu { get; set; }
}
