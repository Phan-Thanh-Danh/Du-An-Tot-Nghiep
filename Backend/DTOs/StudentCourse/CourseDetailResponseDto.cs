using System.Collections.Generic;

namespace Backend.DTOs.StudentCourse;

public class CourseDetailResponseDto
{
    public CourseDetailDto Course { get; set; } = new();
    public List<CourseStatDto> Stats { get; set; } = new();
    public List<CourseChapterDto> Lessons { get; set; } = new();
}

public class CourseDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Teacher { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public int Credits { get; set; }
    public string CoverGradient { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CourseStatDto
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Tone { get; set; } = string.Empty;
    public int Progress { get; set; }
    public string Hint { get; set; } = string.Empty;
}

public class CourseChapterDto
{
    public string Id { get; set; } = string.Empty;
    public string Chapter { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Badge { get; set; } = string.Empty;
    public string Tone { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public List<string> Meta { get; set; } = new();
    public int Progress { get; set; }
    public List<CourseLessonDto> Lessons { get; set; } = new();
}

public class CourseLessonDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
