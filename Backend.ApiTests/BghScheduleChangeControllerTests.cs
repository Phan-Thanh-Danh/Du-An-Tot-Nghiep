using System.Text.Json;
using Backend.Constants;
using Backend.Controllers;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.Models;
using Backend.Services.Bgh;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Backend.ApiTests;

public class BghScheduleChangeControllerTests
{
    [Test]
    public async Task ApproveAndRejectScheduleChanges_ShouldPersistOperationalState()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new ApplicationDbContext(options);

        var course = new KhoaHoc
        {
            MaKhoaHoc = 1,
            MaDonVi = 1,
            MaGiaoVien = 10,
            MaMonHoc = 20,
            TieuDe = "Lớp học phần thật",
            TrangThai = "da_xuat_ban"
        };
        var schedule = new ThoiKhoaBieu
        {
            MaTkb = 1,
            MaKhoaHoc = course.MaKhoaHoc,
            MaCaHoc = 1,
            MaPhong = 1,
            ThuTrongTuan = 2,
            TrangThai = "da_xuat_ban",
            KhoaHoc = course
        };
        var proposal = JsonSerializer.Serialize(new
        {
            NewDate = new DateOnly(2026, 9, 10),
            NewShiftId = 2,
            NewShiftName = "Ca 2",
            NewRoomId = 2,
            NewRoomCode = "A102",
            NewTeacherId = 11,
            NewTeacherName = "Giảng viên dạy thay"
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var approveLesson = CreatePendingChange(1, course, schedule, proposal);
        var rejectLesson = CreatePendingChange(2, course, schedule, proposal);

        context.KhoaHocs.Add(course);
        context.ThoiKhoaBieus.Add(schedule);
        context.BuoiHocs.AddRange(approveLesson, rejectLesson);
        await context.SaveChangesAsync();

        var controller = CreatePrincipalController(context, 1);
        var approveResult = await controller.ApproveScheduleChange(approveLesson.MaBuoiHoc);
        var rejectResult = await controller.RejectScheduleChange(rejectLesson.MaBuoiHoc);

        Assert.That(approveResult.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(rejectResult.Result, Is.TypeOf<OkObjectResult>());
        Assert.Multiple(() =>
        {
            Assert.That(approveLesson.TrangThaiBuoi, Is.EqualTo("doi_lich"));
            Assert.That(approveLesson.NgayHoc, Is.EqualTo(new DateOnly(2026, 9, 10)));
            Assert.That(approveLesson.MaCaHoc, Is.EqualTo(2));
            Assert.That(approveLesson.MaPhong, Is.EqualTo(2));
            Assert.That(approveLesson.MaGiaoVienDayThay, Is.EqualTo(11));
            Assert.That(approveLesson.LyDoThayDoi, Does.StartWith("[Đã duyệt]"));
            Assert.That(rejectLesson.TrangThaiBuoi, Is.EqualTo("du_kien"));
            Assert.That(rejectLesson.MaCaHoc, Is.EqualTo(schedule.MaCaHoc));
            Assert.That(rejectLesson.MaPhong, Is.EqualTo(schedule.MaPhong));
            Assert.That(rejectLesson.MaGiaoVienDayThay, Is.Null);
            Assert.That(rejectLesson.LyDoThayDoi, Does.StartWith("[Từ chối]"));
        });
    }

    private static BuoiHoc CreatePendingChange(
        int id,
        KhoaHoc course,
        ThoiKhoaBieu schedule,
        string proposal) => new()
    {
        MaBuoiHoc = id,
        MaTkb = schedule.MaTkb,
        MaKhoaHoc = course.MaKhoaHoc,
        MaCaHoc = schedule.MaCaHoc,
        MaPhong = schedule.MaPhong,
        MaGiaoVien = course.MaGiaoVien,
        NgayHoc = new DateOnly(2026, 9, 1).AddDays(id),
        TrangThaiBuoi = "du_kien",
        LoaiThayDoi = "doi_phong",
        LyDoThayDoi = "Đổi phòng để phù hợp thiết bị.",
        GhiChu = proposal,
        NgayTao = DateTime.UtcNow,
        KhoaHoc = course,
        Tkb = schedule
    };

    private static BghAcademicController CreatePrincipalController(ApplicationDbContext context, int campusId)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items["CurrentUser"] = new CurrentUserContext
        {
            UserId = 99,
            Email = "principal@test.local",
            Role = AuthRoles.Principal,
            CampusId = campusId,
            Status = "hoat_dong"
        };
        var cache = new BghPerformanceCache(
            new MemoryCache(new MemoryCacheOptions { SizeLimit = 1_000 }),
            NullLogger<BghPerformanceCache>.Instance);
        return new BghAcademicController(context, cache)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }
}
