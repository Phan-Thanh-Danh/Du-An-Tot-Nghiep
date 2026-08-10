using Backend.Configuration;
using Backend.Models;
using Backend.Services.ThoiKhoaBieu;
using Backend.Services.ThoiKhoaBieu.Scoring;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
public class P26_GeneticTimetableHardConstraintTests
{
    private static GeneticTimetableSolver CreateSolver(int weeklyCap = 6)
    {
        var options = Options.Create(new SmartTimetableScoringOptions
        {
            MinTeacherSkill = 70,
            WeeklyCapCa = weeklyCap
        });
        return new GeneticTimetableSolver(new ScheduleCandidateScoringService(options), options);
    }

    [Test]
    public void Solve_WhenNoTeacherMeetsMinimumSkill_LeavesCourseUnassigned()
    {
        var result = CreateSolver().Solve(
            new[] { Course(1, 101, 1001, 501) },
            Shifts(), Rooms(), new Dictionary<int, int> { [1] = 1 },
            new Dictionary<int, IReadOnlyList<TeacherSkillCandidate>>
            {
                [101] = new[] { new TeacherSkillCandidate { MaGiaoVien = 99, MucDoPhuHop = 69 } }
            },
            new Dictionary<int, int> { [501] = 30 },
            new Dictionary<int, IReadOnlySet<(int Day, int Shift)>>(), 30, 10, 0.5, 5);

        Assert.Multiple(() =>
        {
            Assert.That(result.XepDuoc, Is.EqualTo(0));
            Assert.That(result.KhongXepDuoc, Is.EqualTo(1));
            Assert.That(result.Assignments, Is.Empty);
        });
    }

    [Test]
    public void Solve_WhenTeacherWouldExceedWeeklyCap_LeavesExcessCourseUnassigned()
    {
        var courses = Enumerable.Range(1, 7).Select(i => Course(i, 101, 1001, 500 + i)).ToList();
        var required = courses.ToDictionary(x => x.MaKhoaHoc, _ => 1);
        var result = CreateSolver(6).Solve(
            courses, Shifts(), Rooms(), required,
            new Dictionary<int, IReadOnlyList<TeacherSkillCandidate>>
            {
                [101] = new[] { new TeacherSkillCandidate { MaGiaoVien = 1001, MucDoPhuHop = 100 } }
            }, courses.ToDictionary(x => x.MaLop, _ => 30),
            new Dictionary<int, IReadOnlySet<(int Day, int Shift)>>(), 50, 20, 0.5, 5);

        Assert.Multiple(() =>
        {
            Assert.That(result.XepDuoc, Is.EqualTo(6));
            Assert.That(result.KhongXepDuoc, Is.EqualTo(1));
            Assert.That(result.Assignments.GroupBy(x => x.MaGiaoVien).Single().Count(), Is.EqualTo(6));
        });
    }

    [Test]
    public void Solve_WhenTeacherHasConfirmedAvailability_OnlyUsesConfirmedSlots()
    {
        var result = CreateSolver().Solve(
            new[] { Course(1, 101, 1001, 501) },
            Shifts(), Rooms(), new Dictionary<int, int> { [1] = 1 },
            new Dictionary<int, IReadOnlyList<TeacherSkillCandidate>>
            {
                [101] = new[] { new TeacherSkillCandidate { MaGiaoVien = 1001, MucDoPhuHop = 100 } }
            }, new Dictionary<int, int> { [501] = 30 },
            new Dictionary<int, IReadOnlySet<(int Day, int Shift)>>
            {
                [1001] = new HashSet<(int Day, int Shift)> { (3, 1) }
            }, 30, 10, 0.5, 5);

        Assert.That(result.Assignments, Has.Count.EqualTo(1));
        Assert.That(result.Assignments.Single().ThuTrongTuan, Is.EqualTo(3));
    }

    [Test]
    public void Solve_WhenPreferredTeacherReachesCap_ReassignsCourseToAnotherQualifiedTeacher()
    {
        var courses = Enumerable.Range(1, 3).Select(i => Course(i, 101, 1001, 500 + i)).ToList();
        var result = CreateSolver(6).Solve(
            courses, Shifts(), Rooms(2), courses.ToDictionary(x => x.MaKhoaHoc, _ => 3),
            new Dictionary<int, IReadOnlyList<TeacherSkillCandidate>>
            {
                [101] = new[]
                {
                    new TeacherSkillCandidate { MaGiaoVien = 1001, MucDoPhuHop = 100 },
                    new TeacherSkillCandidate { MaGiaoVien = 1002, MucDoPhuHop = 90 }
                }
            }, courses.ToDictionary(x => x.MaLop, _ => 30),
            new Dictionary<int, IReadOnlySet<(int Day, int Shift)>>(), 50, 20, 0.5, 5);

        Assert.Multiple(() =>
        {
            Assert.That(result.XepDuoc, Is.EqualTo(3));
            Assert.That(result.KhongXepDuoc, Is.EqualTo(0));
            Assert.That(result.Assignments.GroupBy(x => x.MaGiaoVien).Select(x => x.Count()), Is.All.LessThanOrEqualTo(6));
            Assert.That(result.Assignments.Select(x => x.MaGiaoVien), Does.Contain(1002));
        });
    }

    [Test]
    public void Solve_WhenOnlyRoomIsUndersized_LeavesCourseUnassigned()
    {
        var result = CreateSolver().Solve(
            new[] { Course(1, 101, 1001, 501) },
            Shifts(), Rooms(), new Dictionary<int, int> { [1] = 1 },
            new Dictionary<int, IReadOnlyList<TeacherSkillCandidate>>
            {
                [101] = new[] { new TeacherSkillCandidate { MaGiaoVien = 1001, MucDoPhuHop = 100 } }
            }, new Dictionary<int, int> { [501] = 41 },
            new Dictionary<int, IReadOnlySet<(int Day, int Shift)>>(), 30, 10, 0.5, 5);

        Assert.That(result.XepDuoc, Is.EqualTo(0));
        Assert.That(result.KhongXepDuoc, Is.EqualTo(1));
    }

    [Test]
    public void OccupationMap_TracksTeacherWeeklyLoad()
    {
        var map = new OccupationMap();
        for (var shift = 1; shift <= 6; shift++)
            map.OccupyTeacher(3, 2, shift, 1001);

        Assert.That(map.GetTeacherWeeklyLoad(3, 1001), Is.EqualTo(6));
    }

    private static KhoaHoc Course(int id, int subjectId, int teacherId, int classId) => new()
    {
        MaKhoaHoc = id,
        MaMonHoc = subjectId,
        MaGiaoVien = teacherId,
        MaLop = classId
    };

    private static List<CaHoc> Shifts() => new()
    {
        new CaHoc { MaCaHoc = 1, TenCa = "Ca 1", ConHoatDong = true }
    };

    private static List<PhongHoc> Rooms(int count = 1) => Enumerable.Range(1, count)
        .Select(id => new PhongHoc { MaPhong = id, SucChua = 40, TrangThaiPhong = "hoat_dong" })
        .ToList();
}
