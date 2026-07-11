namespace Backend.DTOs.TeachingPreferences;

public class TeachingPreferenceSlotDto
{
    public int ThuTrongTuan { get; set; }
    public int MaCaHoc { get; set; }
    public string MucDo { get; set; } = string.Empty; // preferred, available, unavailable
}
