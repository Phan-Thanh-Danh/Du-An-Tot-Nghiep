using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.QuestionBank;

public class QuestionChoiceDto
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;
}
