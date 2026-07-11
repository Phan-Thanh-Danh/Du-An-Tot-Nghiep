using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.TeachingPreferences;

public class UpdateTeachingPreferenceDto
{
    [Range(0, 100)]
    public int? SoLopToiDaMongMuon { get; set; }
    
    [Range(0, 100)]
    public int? SoCaToiDaMoiTuan { get; set; }
    
    [MaxLength(1000)]
    public string? GhiChu { get; set; }
    
    public List<TeachingPreferenceSlotDto> Slots { get; set; } = new List<TeachingPreferenceSlotDto>();
}
