namespace Backend.DTOs.TeachingPreferences;

public class TeachingPreferenceFormDto
{
    public int MaHocKy { get; set; }
    public int? SoLopToiDaMongMuon { get; set; }
    public int? SoCaToiDaMoiTuan { get; set; }
    public string? GhiChu { get; set; }
    public string TrangThai { get; set; } = "unstarted";
    public DateTime? NgayCapNhat { get; set; }
    public DateTime? NgayGui { get; set; }
    
    // Only slots that are not neutral/unknown are returned
    public List<TeachingPreferenceSlotDto> Slots { get; set; } = new List<TeachingPreferenceSlotDto>();
}
