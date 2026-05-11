namespace Backend.DTOs;

public class AccountProfileResponse
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public string VaiTroChinh { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
}
