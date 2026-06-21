namespace Backend.DTOs.Curriculum;

// ─── Chương (Chapter) ────────────────────────────────────────

public class ChuongDto
{
    public int MaChuong { get; set; }
    public int MaMonHoc { get; set; }
    public string TenMonHoc { get; set; } = string.Empty;
    public string TieuDe { get; set; } = string.Empty;
    public int ThuTu { get; set; }
    public bool DaAn { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public int SoBaiHoc { get; set; }
    public List<BaiHocDto> BaiHocs { get; set; } = [];
}

public class ChuongCreateDto
{
    public int MaMonHoc { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public int? ThuTu { get; set; }
}

public class ChuongUpdateDto
{
    public string TieuDe { get; set; } = string.Empty;
    public bool? DaAn { get; set; }
}

// ─── Bài học (Lesson) ────────────────────────────────────────

public class BaiHocDto
{
    public int MaBaiHoc { get; set; }
    public int MaChuong { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string LoaiBaiHoc { get; set; } = string.Empty;
    public string? UrlTapTin { get; set; }
    public int? ThoiLuongGiay { get; set; }
    public string? NoiDungVanBan { get; set; }
    public string? DieuKienMoKhoa { get; set; }
    public string? TomTatAi { get; set; }
    public int ThuTu { get; set; }
    public bool DaAn { get; set; }
    public string? TrangThai { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public int SoNoiDung { get; set; }
    public List<BaiHocNoiDungDto> NoiDungs { get; set; } = [];
}

public class BaiHocCreateDto
{
    public int MaChuong { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string LoaiBaiHoc { get; set; } = "van_ban";
    public int? ThuTu { get; set; }
}

public class BaiHocUpdateDto
{
    public string? TieuDe { get; set; }
    public string? LoaiBaiHoc { get; set; }
    public string? NoiDungVanBan { get; set; }
    public string? DieuKienMoKhoa { get; set; }
    public bool? DaAn { get; set; }
    public string? TrangThai { get; set; }
}

// ─── Nội dung bài học (Lesson Content Block) ─────────────────

public class BaiHocNoiDungDto
{
    public int MaNoiDung { get; set; }
    public int MaBaiHoc { get; set; }
    public string LoaiNoiDung { get; set; } = string.Empty;
    public string? NoiDungHtml { get; set; }
    public string? NoiDungJson { get; set; }
    public string? UrlTapTin { get; set; }
    public string? StorageKey { get; set; }
    public long? KichThuocByte { get; set; }
    public int? ThoiLuongGiay { get; set; }
    public int? MaDeKiemTra { get; set; }
    public string? TrangThai { get; set; }
    public int ThuTu { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}

public class BaiHocNoiDungCreateDto
{
    public int MaBaiHoc { get; set; }
    public string LoaiNoiDung { get; set; } = string.Empty;
    public string? NoiDungHtml { get; set; }
    public string? NoiDungJson { get; set; }
    public string? UrlTapTin { get; set; }
    public string? StorageKey { get; set; }
    public long? KichThuocByte { get; set; }
    public int? ThoiLuongGiay { get; set; }
    public int? MaDeKiemTra { get; set; }
    public int? ThuTu { get; set; }
}

public class BaiHocNoiDungUpdateDto
{
    public string? NoiDungHtml { get; set; }
    public string? NoiDungJson { get; set; }
    public string? UrlTapTin { get; set; }
    public string? StorageKey { get; set; }
    public long? KichThuocByte { get; set; }
    public int? ThoiLuongGiay { get; set; }
    public int? MaDeKiemTra { get; set; }
    public string? TrangThai { get; set; }
    public DateTime? NgayCapNhat { get; set; } // for concurrency check
}

// ─── Reorder ─────────────────────────────────────────────────

public class ReorderItemDto
{
    public int Id { get; set; }
    public int ThuTu { get; set; }
}

public class ReorderRequestDto
{
    public List<ReorderItemDto> Items { get; set; } = [];
}

// ─── Upload Result ───────────────────────────────────────────

public class UploadResultDto
{
    public string Url { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public long KichThuocByte { get; set; }
    public string ContentType { get; set; } = string.Empty;
}
