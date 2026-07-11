namespace Backend.DTOs.TeachingPreferences;

public class TeachingPreferenceShiftDto
{
    public int MaCaHoc { get; set; }
    public string TenCa { get; set; } = string.Empty;
    public string Buoi { get; set; } = string.Empty;
    public string GioBatDau { get; set; } = string.Empty;
    public string GioKetThuc { get; set; } = string.Empty;
    public int ThuTu { get; set; }
}
