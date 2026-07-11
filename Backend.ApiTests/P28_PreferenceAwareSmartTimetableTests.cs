using Backend.Configuration;
using Backend.DTOs.SmartTimetable.Suggestions;
using Backend.Models;
using Backend.Services.ThoiKhoaBieu.Scoring;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
public class P28_PreferenceAwareSmartTimetableTests
{
    private IScheduleCandidateScoringService _scoringService;
    private SmartTimetableScoringOptions _options;

    [SetUp]
    public void Setup()
    {
        _options = new SmartTimetableScoringOptions();
        var mockOptions = Options.Create(_options);
        _scoringService = new ScheduleCandidateScoringService(mockOptions);
    }

    [Test]
    public void ScoreCandidate_ShouldPrioritizePreferredShift()
    {
        // Arrange
        var context = new ScheduleCandidateContext
        {
            DayOfWeek = 2,
            Shift = new CaHoc { MaCaHoc = 1, TenCa = "Ca 1", GioBatDau = new TimeOnly(7, 0), GioKetThuc = new TimeOnly(9, 15) },
            Room = new PhongHoc { MaPhong = 1, TenPhong = "A101", SucChua = 40 },
            Course = new KhoaHoc { MaKhoaHoc = 1 },
            ExpectedStudentCount = 30,
            PreferenceLevel = "preferred"
        };

        // Act
        var suggestion = _scoringService.ScoreCandidate(context);

        // Assert
        Assert.That(suggestion.HardConstraintPassed, Is.True);
        Assert.That(suggestion.Score, Is.EqualTo(_options.BaseScore + _options.PreferredShiftBonus + _options.GoodRoomFitBonus));
        Assert.That(suggestion.Reasons.Any(r => r.Contains("nguyện vọng ưu tiên")), Is.True);
    }

    [Test]
    public void ScoreCandidate_ShouldRejectUnavailableShift()
    {
        // Arrange
        var context = new ScheduleCandidateContext
        {
            DayOfWeek = 3,
            Shift = new CaHoc { MaCaHoc = 2, TenCa = "Ca 2", GioBatDau = new TimeOnly(9, 30), GioKetThuc = new TimeOnly(11, 45) },
            Room = new PhongHoc { MaPhong = 1, TenPhong = "A101", SucChua = 40 },
            Course = new KhoaHoc { MaKhoaHoc = 1 },
            ExpectedStudentCount = 30,
            PreferenceLevel = "unavailable"
        };

        // Act
        var suggestion = _scoringService.ScoreCandidate(context);

        // Assert
        Assert.That(suggestion.HardConstraintPassed, Is.False);
        Assert.That(suggestion.Warnings.Any(w => w.Contains("báo bận")), Is.True);
    }

    [Test]
    public void ScoreCandidate_ShouldApplyWorkloadPenalties()
    {
        // Arrange
        var context = new ScheduleCandidateContext
        {
            DayOfWeek = 4,
            Shift = new CaHoc { MaCaHoc = 3, TenCa = "Ca 3", GioBatDau = new TimeOnly(13, 0), GioKetThuc = new TimeOnly(15, 15) },
            Room = new PhongHoc { MaPhong = 1, TenPhong = "A101", SucChua = 40 },
            Course = new KhoaHoc { MaKhoaHoc = 1 },
            ExpectedStudentCount = 30,
            TeacherDailyLoad = 3, // Meets threshold
            ClassDailyLoad = 3    // Meets threshold
        };

        // Act
        var suggestion = _scoringService.ScoreCandidate(context);

        // Assert
        Assert.That(suggestion.HardConstraintPassed, Is.True);
        Assert.That(suggestion.Components.TeacherDayLoadPenalty, Is.EqualTo(-_options.TeacherDailyLoadPenalty));
        Assert.That(suggestion.Components.ClassDayLoadPenalty, Is.EqualTo(-_options.ClassDailyLoadPenalty));
    }
    
    [Test]
    public void ScoreCandidate_ShouldRejectUnderSizedRoom()
    {
        // Arrange
        var context = new ScheduleCandidateContext
        {
            DayOfWeek = 2,
            Shift = new CaHoc { MaCaHoc = 1, TenCa = "Ca 1", GioBatDau = new TimeOnly(7, 0), GioKetThuc = new TimeOnly(9, 15) },
            Room = new PhongHoc { MaPhong = 1, TenPhong = "A101", SucChua = 20 },
            Course = new KhoaHoc { MaKhoaHoc = 1 },
            ExpectedStudentCount = 30
        };

        // Act
        var suggestion = _scoringService.ScoreCandidate(context);

        // Assert
        Assert.That(suggestion.HardConstraintPassed, Is.False);
        Assert.That(suggestion.Warnings.Any(w => w.Contains("Sức chứa")), Is.True);
    }
}
