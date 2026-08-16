namespace Backend.DTOs.AdminUsers;

public class UserImportResultDto
{
    public int TongSoDong { get; set; }
    public int SoDongHopLe { get; set; }
    public int SoDongLoi { get; set; }
    public int SoDongDaNhap { get; set; }
    public int SoDongTaoMoi { get; set; }
    public int SoDongCapNhat { get; set; }
    public bool DryRun { get; set; }
    public bool DaLuu { get; set; }
    public IReadOnlyList<UserImportErrorDto> ChiTietLoi { get; set; } = [];
}

public class UserImportErrorDto
{
    public int Dong { get; set; }
    public string? Email { get; set; }
    public string LyDo { get; set; } = string.Empty;
}
