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
    
    public int DefaultTopN { get; set; } = 10;
    public int MaximumTopN { get; set; } = 50;
}
