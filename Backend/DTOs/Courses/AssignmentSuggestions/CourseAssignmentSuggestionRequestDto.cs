using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Courses.AssignmentSuggestions;

public class CourseAssignmentSuggestionRequestDto
{
    [Required]
    public int MaHocKy { get; set; }

    [Required]
    public int MaMonHoc { get; set; }

    [Required]
    public List<int> MaLopIds { get; set; } = new();

    public List<PlannedTeachingSlotDto> PlannedSlots { get; set; } = new();

    [Range(1, 100)]
    public int CandidateLimit { get; set; } = 20;
}
