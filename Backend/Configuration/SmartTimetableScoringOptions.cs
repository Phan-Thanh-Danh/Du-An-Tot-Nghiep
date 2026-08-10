namespace Backend.Configuration;

public class SmartTimetableScoringOptions
{
    public const string SectionName = "SmartTimetableScoring";

    public double BaseScore { get; set; } = 100;
    public double PreferredShiftBonus { get; set; } = 15;
    public double AvailableShiftBonus { get; set; } = 5;
    
    public int TeacherDailyLoadThreshold { get; set; } = 3;
    public double TeacherDailyLoadPenalty { get; set; } = 15;
    
    public int ClassDailyLoadThreshold { get; set; } = 3;
    public double ClassDailyLoadPenalty { get; set; } = 15;
    
    public double SaturdayPenalty { get; set; } = 5;
    public double EveningPenalty { get; set; } = 8;
    
    public double GoodRoomFitBonus { get; set; } = 5;
    public double OversizedRoomPenalty { get; set; } = 5;
    public double OversizedRoomRatio { get; set; } = 2.0;

    // Genetic Algorithm soft penalties (per event)
    public double SameDayDuplicatePenalty { get; set; } = 60;
    public double ConsecutiveShiftPenalty { get; set; } = 30;
    public double UnassignedSlotPenalty { get; set; } = 500;
    public double HardConflictPenalty { get; set; } = 1000;

    // Teacher skill matrix scoring (GA chọn giảng viên theo kỹ năng)
    public double SkillScoreWeight { get; set; } = 150;
    public bool PreferMainSubjectTeacher { get; set; } = true;

    // Ngưỡng chuyên môn tối thiểu: chỉ xếp giảng viên có mức độ phù hợp >= ngưỡng
    public int MinTeacherSkill { get; set; } = 70;

    // Cân bằng định mức giảng dạy (ca/tuần/GV). WeeklyCapCa là hard constraint.
    public int WeeklyTargetCa { get; set; } = 5;
    public double WeeklyLoadPenalty { get; set; } = 15;
    public int WeeklyCapCa { get; set; } = 6;

    public int DefaultTopN { get; set; } = 10;
    public int MaximumTopN { get; set; } = 50;
}
