using System.Collections.Generic;

namespace Backend.DTOs.StudentCurriculum
{
    public class StudentCurriculumResponseDto
    {
        public string StudentName { get; set; } = string.Empty;
        public string MajorName { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
        public string ProgramCode { get; set; } = string.Empty;
        public string ProgramVersion { get; set; } = string.Empty;
        public string ProgramName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string CohortName { get; set; } = string.Empty;
        public int TrainingMonths { get; set; }
        public int TotalSemesters { get; set; }
        public int CurrentSemesterIndex { get; set; }
        public int CurrentBlockIndex { get; set; }
        public int TotalCredits { get; set; }
        public int CompletedCredits { get; set; }
        public int TotalSubjects { get; set; }
        public int CompletedSubjects { get; set; }
        public List<SemesterDto> Semesters { get; set; } = new List<SemesterDto>();
    }

    public class SemesterDto
    {
        public int SemesterIndex { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public List<BlockDto> Blocks { get; set; } = new List<BlockDto>();
    }

    public class BlockDto
    {
        public int BlockIndex { get; set; }
        public string BlockName { get; set; } = string.Empty;
        public List<CurriculumSubjectDto> Subjects { get; set; } = new List<CurriculumSubjectDto>();
    }

    public class CurriculumSubjectDto
    {
        public string Id { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ProgressPercent { get; set; }
        public double? QuizScore { get; set; }
        public double? Score { get; set; }
        public bool IsEarlyLearning { get; set; }
        public string SubjectType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string Note { get; set; } = string.Empty;
        
        public string VersionStatus { get; set; } = string.Empty;
        public string ReplacedSubjectCode { get; set; } = string.Empty;
        public string PreviousVersionName { get; set; } = string.Empty;
        public double? EarlyScoreFromOldVersion { get; set; }
        public bool RequiresSupplement { get; set; }
        public int? SupplementPercent { get; set; }
        public string VersionNote { get; set; } = string.Empty;
    }
}
