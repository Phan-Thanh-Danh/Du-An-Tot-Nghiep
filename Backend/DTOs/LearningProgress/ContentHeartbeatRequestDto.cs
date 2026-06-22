using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.LearningProgress;

public class ContentHeartbeatRequestDto
{
    [Required]
    public Guid SessionToken { get; set; }

    [Required]
    public int SoThuTuNhipTim { get; set; }

    public int? ViTriVideoGiay { get; set; }
    public decimal? PhanTramCuon { get; set; }
    public int? ChiSoMuc { get; set; }
}
