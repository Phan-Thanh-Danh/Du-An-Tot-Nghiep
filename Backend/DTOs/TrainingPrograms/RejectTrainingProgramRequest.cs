using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.TrainingPrograms;

public class RejectTrainingProgramRequest
{
    [Required(ErrorMessage = "Lý do từ chối không được để trống.")]
    public string LyDoTuChoi { get; set; } = string.Empty;
}
