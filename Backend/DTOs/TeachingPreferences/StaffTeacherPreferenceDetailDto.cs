namespace Backend.DTOs.TeachingPreferences;

public class StaffTeacherPreferenceDetailDto
{
    public int MaGiaoVien { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TrangThai { get; set; } = "unstarted";
    public DateTime? NgayGui { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
    public TeachingPreferenceFormDto? Form { get; set; }
}
