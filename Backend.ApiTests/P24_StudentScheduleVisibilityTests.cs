using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.StudentSchedule;
using Backend.Models;
using Backend.Services.StudentSchedule;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Backend.ApiTests;

public class P24_StudentScheduleVisibilityTests
{
    private DbContextOptions<ApplicationDbContext> GetInMemoryOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private StudentScheduleService CreateService(ApplicationDbContext context, DateTime utcNow)
    {
        var timeProviderMock = new Mock<TimeProvider>();
        timeProviderMock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(utcNow));
        return new StudentScheduleService(context, timeProviderMock.Object);
    }

    [Test]
    public async Task GetScheduleSummaryAsync_ShouldOnlyReturnVisibleTermsAndOwnClassSessions()
    {
        var options = GetInMemoryOptions();
        using var context = new ApplicationDbContext(options);
        
        var todayUtc = new DateTime(2024, 5, 20, 0, 0, 0, DateTimeKind.Utc); // May 20, 2024
        var todayVn = TimeZoneInfo.ConvertTimeFromUtc(todayUtc, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).Date;
        var todayVnDateOnly = DateOnly.FromDateTime(todayVn);

        var studentId = 1;
        var classId = 101;
        var otherClassId = 102;
        var termId = 10;
        var farTermId = 11;
        var donViId = 1;

        context.HocKys.AddRange(
            new HocKy { MaHocKy = termId, NgayBatDau = todayVnDateOnly.AddDays(-10), NgayKetThuc = todayVnDateOnly.AddDays(30), MaDonVi = donViId },
            new HocKy { MaHocKy = farTermId, NgayBatDau = todayVnDateOnly.AddDays(8), NgayKetThuc = todayVnDateOnly.AddDays(100), MaDonVi = donViId } // Far term, should be hidden
        );

        context.KhoaHocs.AddRange(
            new KhoaHoc { MaKhoaHoc = 1, MaHocKy = termId, MaLop = classId, TrangThai = "da_xuat_ban", MaDonVi = donViId },
            new KhoaHoc { MaKhoaHoc = 2, MaHocKy = termId, MaLop = otherClassId, TrangThai = "da_xuat_ban", MaDonVi = donViId },
            new KhoaHoc { MaKhoaHoc = 3, MaHocKy = farTermId, MaLop = classId, TrangThai = "da_xuat_ban", MaDonVi = donViId }
        );

        context.ThoiKhoaBieus.AddRange(
            new ThoiKhoaBieu { MaTkb = 1, MaKhoaHoc = 1, TrangThai = "da_xuat_ban" }, // Own class
            new ThoiKhoaBieu { MaTkb = 2, MaKhoaHoc = 2, TrangThai = "da_xuat_ban" }, // Other class
            new ThoiKhoaBieu { MaTkb = 3, MaKhoaHoc = 3, TrangThai = "da_xuat_ban" }  // Far term
        );

        context.BuoiHocs.AddRange(
            // Own class today session
            new BuoiHoc { MaBuoiHoc = 1, MaKhoaHoc = 1, MaTkb = 1, NgayHoc = todayVnDateOnly, TrangThaiBuoi = "du_kien", MaCaHoc = 1 },
            // Own class cancelled session today
            new BuoiHoc { MaBuoiHoc = 2, MaKhoaHoc = 1, MaTkb = 1, NgayHoc = todayVnDateOnly, TrangThaiBuoi = "da_huy", MaCaHoc = 2 },
            // Own class next session tomorrow
            new BuoiHoc { MaBuoiHoc = 3, MaKhoaHoc = 1, MaTkb = 1, NgayHoc = todayVnDateOnly.AddDays(1), TrangThaiBuoi = "du_kien", MaCaHoc = 1 },
            // Other class session today
            new BuoiHoc { MaBuoiHoc = 4, MaKhoaHoc = 2, MaTkb = 2, NgayHoc = todayVnDateOnly, TrangThaiBuoi = "du_kien", MaCaHoc = 1 }
        );

        context.CaHocs.AddRange(
            new CaHoc { MaCaHoc = 1, GioBatDau = new TimeOnly(7, 30, 0), GioKetThuc = new TimeOnly(9, 30, 0) },
            new CaHoc { MaCaHoc = 2, GioBatDau = new TimeOnly(9, 45, 0), GioKetThuc = new TimeOnly(11, 45, 0) }
        );

        await context.SaveChangesAsync();

        var service = CreateService(context, todayUtc);

        var summary = await service.GetScheduleSummaryAsync(studentId, classId);

        // Verify terms
        Assert.That(summary.CurrentTerm, Is.Not.Null);
        Assert.That(summary.CurrentTerm.MaHocKy, Is.EqualTo(termId));
        Assert.That(summary.UpcomingTerm, Is.Null); // far term is > 7 days away

        // Verify Today Sessions
        // Should only have own class today sessions that are NOT cancelled
        Assert.That(summary.TodaySessions, Has.Count.EqualTo(1));
        Assert.That(summary.TodaySessions[0].MaBuoiHoc, Is.EqualTo(1));
        Assert.That(summary.TodaySessionCount, Is.EqualTo(1));

        // Verify Next Session
        Assert.That(summary.NextSession, Is.Not.Null);
        // It could be session 1 if now is before 7:30, or session 3 if now is after 9:30
        // Because todayUtc is 00:00:00 UTC -> 07:00:00 VN, which is before 7:30, so next session is session 1
        Assert.That(summary.NextSession.MaBuoiHoc, Is.EqualTo(1));

        // Verify counts
        // Weekly count should include session 1 and 3 (and possibly others depending on week boundary, but we just check it doesn't include other class)
        Assert.That(summary.WeeklySessionCount, Is.EqualTo(2)); // Session 1 and 3 (Session 2 is cancelled)
        Assert.That(summary.ActiveCourseCount, Is.EqualTo(1));
    }
}
